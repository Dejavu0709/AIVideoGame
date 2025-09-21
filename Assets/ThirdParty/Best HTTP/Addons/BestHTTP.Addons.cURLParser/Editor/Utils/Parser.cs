using System;
using System.Collections.Generic;
using System.Text;

using BestHTTP.Authentication;

namespace BestHTTP.Addons.cURLParser.Editor.Utils
{
    public struct ParsedOption
    {
        public string option;
        public LongShort alias;
        public string value;

        public TokenTypes tokenType;
    }

    public enum TokenTypes : int
    {
        None,
        SingleDashed,
        DoubleDashed,
        Quoted,
        LineBreak
    }

    public static class Parser
    {
        public static List<ParsedOption> ParseCURLCommand(string command)
        {
            PeakableCharacterStream stream = new PeakableCharacterStream(command);

            stream.SkipWhiteSpace();

            if (HasCURLPrefix(stream))
                stream.SkipUntilWhiteSpace();

            var next = ReadNext(stream);

            var result = new List<ParsedOption>();
            while (next.Key != null)
            {
                foreach (var nextOption in Parse(stream, next))
                {
                    if (nextOption.option == null)
                        goto EXIT;

                    result.Add(nextOption);
                }

                next = ReadNext(stream);
            }

          EXIT:
            result.Sort((a, b) =>
            {
                if (a.alias.lname == null)
                    return -1;
                else if (b.alias.lname == null)
                    return 1;

                return b.alias.lname.CompareTo(a.alias.lname);
            });
            return result;
        }

        public static IEnumerable<ParsedOption> Parse(PeakableCharacterStream stream, KeyValuePair<string, TokenTypes> token)
        {
            switch (token.Value)
            {
                case TokenTypes.SingleDashed:
                    {
                        // Short version options that don't need any additional values can be used immediately next to each other, like for example you can specify all the options -O, -L and -v at once as -OLv.
                        foreach (char option in token.Key)
                        {
                            var alias = cURLAliases.FindByShort(option);

                            switch (alias.desc)
                            {
                                case ARGTypes.ARG_STRING:
                                case ARGTypes.ARG_FILENAME:

                                    var nextToken = ReadNext(stream);
                                    if (nextToken.Value != TokenTypes.None && nextToken.Value != TokenTypes.Quoted)
                                        throw new Exception($"Unexpected token! '{nextToken.Key}': {nextToken.Value}");

                                    if (alias.desc == ARGTypes.ARG_FILENAME)
                                        nextToken = new KeyValuePair<string, TokenTypes>(PrepareFileName(nextToken.Key), nextToken.Value);

                                    yield return new ParsedOption { option = option.ToString(), tokenType = token.Value, value = nextToken.Key, alias = alias };

                                    break;

                                default:
                                    yield return new ParsedOption { option = option.ToString(), tokenType = token.Value, value = null, alias = alias };
                                    break;
                            }
                        }
                        break;
                    }

                case TokenTypes.DoubleDashed:
                    {
                        var alias = cURLAliases.FindByLong(token.Key);

                        switch (alias.desc)
                        {
                            case ARGTypes.ARG_STRING:
                            case ARGTypes.ARG_FILENAME:
                                var nextToken = ReadNext(stream);
                                if (nextToken.Value != TokenTypes.None && nextToken.Value != TokenTypes.Quoted)
                                    throw new Exception($"Unexpected token! '{nextToken.Key}': {nextToken.Value}");

                                if (alias.desc == ARGTypes.ARG_FILENAME)
                                    nextToken = new KeyValuePair<string, TokenTypes>(PrepareFileName(nextToken.Key), nextToken.Value);

                                yield return new ParsedOption { option = token.Key, tokenType = token.Value, value = nextToken.Key, alias = alias };

                                break;

                            default:
                                yield return new ParsedOption { option = token.Key, tokenType = token.Value, value = null, alias = alias };
                                break;
                        }
                        break;
                    }

                default:
                    yield return new ParsedOption { option = token.Key, tokenType = token.Value, value = null, alias = LongShort.Empty };
                    break;
            }

            yield break;
        }

        private static string PrepareFileName(string filePathAndName)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < filePathAndName.Length; i++)
            {
                char ch = filePathAndName[i];
                if (ch == '\\' && i < filePathAndName.Length - 1)
                {
                    if (filePathAndName[i + 1] != '\\' && (i == 0 || filePathAndName[i - 1] != '\\'))
                        builder.Append('\\');
                }

                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static KeyValuePair<string, TokenTypes> ReadNext(PeakableCharacterStream stream)
        {
            stream.SkipWhiteSpace();

            if (stream.IsEOF)
                return new KeyValuePair<string, TokenTypes>(null, TokenTypes.None);

            TokenTypes tokenType = TokenTypes.None;

            using (new BeginEndPeak(stream))
            {
                switch (stream.Current)
                {
                    case '-':
                        stream.Advance();
                        tokenType = stream.Current == '-' ? TokenTypes.DoubleDashed : TokenTypes.SingleDashed;
                        break;

                    case '\"':
                    case '\'':
                        tokenType = TokenTypes.Quoted;
                        break;

                    case '\\':
                        tokenType = TokenTypes.LineBreak;
                        break;
                }
            }

            string readToken = null;
            switch (tokenType)
            {
                case TokenTypes.SingleDashed:
                case TokenTypes.DoubleDashed:
                    readToken = ReadDashed(stream, tokenType);
                    break;

                case TokenTypes.Quoted:
                    readToken = ReadQuoted(stream);
                    break;

                case TokenTypes.None:
                    int beginPos = stream.Position;
                    stream.SkipUntilWhiteSpace();
                    readToken = new string(stream.Characters, beginPos, stream.Position - beginPos);
                    break;

                case TokenTypes.LineBreak:
                    stream.Advance();
                    return ReadNext(stream);
            }

            return new KeyValuePair<string, TokenTypes>(readToken, tokenType);
        }

        public static string ReadQuoted(PeakableCharacterStream stream)
        {
            char quote = stream.Advance();
            char current = stream.Current;
            if (current == quote)
                return string.Empty;

            bool foundEndQuote = false;
            bool isEscaped = false;

            int beginPos = stream.Position;

            while (!foundEndQuote && !stream.IsEOF)
            {
                if (current == '\\')
                    isEscaped = true;
                else
                {
                    if (current == quote && !isEscaped)
                        foundEndQuote = true;

                    isEscaped = false;
                }

                if (!foundEndQuote)
                    current = stream.Advance();
            }

            StringBuilder sb = new StringBuilder();
            int endPos = stream.Position - 1;

            for (int i = beginPos; i < endPos; ++i)
            {
                char ch = stream.Characters[i];
                if (ch == '\\' && i < endPos - 1 && stream.Characters[i + 1] == '\"')
                {
                    sb.Append("\"\"");
                    i++;
                }
                else if (ch == '\"' && i < endPos - 1 && stream.Characters[i + 1] != '\"')
                {
                    sb.Append("\"\"");
                }
                else
                    sb.Append(ch);
            }

            return sb.ToString();

            //return new string(stream.Characters, beginPos, stream.Position - beginPos - 1);
        }

        public static string ReadDashed(PeakableCharacterStream stream, TokenTypes tokenType)
        {
            // skip the dashes
            if (tokenType == TokenTypes.SingleDashed || tokenType == TokenTypes.DoubleDashed)
                stream.Position += (int)tokenType;

            int beginPos = stream.Position;

            if (tokenType == TokenTypes.SingleDashed)
            {
                var option = cURLAliases.FindByShort(stream.Current);
                if (option.desc != ARGTypes.ARG_BOOL && option.desc != ARGTypes.ARG_NONE)
                {
                    bool endShort = false;
                    using (new BeginEndPeak(stream))
                    {
                        stream.Advance();
                        if (!char.IsWhiteSpace(stream.Current))
                            endShort = true;
                    }

                    if (endShort)
                    {
                        stream.Advance();
                        return new string(stream.Characters, beginPos, stream.Position - beginPos);
                    }
                }
            }
            stream.SkipUntilWhiteSpace();

            return new string(stream.Characters, beginPos, stream.Position - beginPos);
        }

        public static bool HasCURLPrefix(PeakableCharacterStream stream)
        {
            // "curl "
            if (stream.Length - stream.Position < 5)
                return false;

            stream.BeginPeek();
            try
            {
                stream.SkipWhiteSpace();

                char c = char.ToLowerInvariant(stream.Advance());
                char u = char.ToLowerInvariant(stream.Advance());
                char r = char.ToLowerInvariant(stream.Advance());
                char l = char.ToLowerInvariant(stream.Advance());

                return (c == 'c' || u == 'u' || r == 'r' || l == 'l');
            }
            finally
            {
                stream.EndPeek();
            }
        }

        /// <summary>
        /// This function expects user and password separated with a semicolon: "user:password"
        /// </summary>
        public static Credentials ParseCredentials(string userAndPassword)
        {
            if (string.IsNullOrEmpty(userAndPassword))
                return null;

            int colonIndex = userAndPassword.IndexOf(':');
            if (colonIndex < 0)
                return new Credentials(userAndPassword, null);

            return new Credentials(userAndPassword.Substring(0, colonIndex), userAndPassword.Substring(colonIndex + 1));
        }
    }
}
