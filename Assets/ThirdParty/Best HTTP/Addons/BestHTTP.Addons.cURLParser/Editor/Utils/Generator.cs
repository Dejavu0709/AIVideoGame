using System;
using System.IO;
using System.Text;

using BestHTTP.Authentication;

namespace BestHTTP.Addons.cURLParser.Editor.Utils
{
    public sealed class GeneratorContext
    {
        public string indent = "    ";
        
        public string method = null;
        public string url = string.Empty;
        public Credentials credentials = null;
        
        public DataContext dataContext = new DataContext();
        public ProxySettings proxy = new ProxySettings();

        public void Apply(StringBuilder sb)
        {
            this.dataContext.Apply(sb, this);
        }
    }

    public static class Generator
    {
        static StringBuilder preRequestBuilder = new StringBuilder();
        static StringBuilder requestBuilder = new StringBuilder();

        public static string Generate(string cURLCommand, RequestUsageTypes requestUsageType)
        {
            preRequestBuilder.Clear();
            requestBuilder.Clear();
            GeneratorContext generatorContext = new GeneratorContext();

            foreach (var option in Parser.ParseCURLCommand(cURLCommand))
            {
                switch (option.alias.lname)
                {
                    // https://curl.se/docs/manpage.html#-r
                    case "range":
                        // TODO
                        break;

                    // https://curl.se/docs/manpage.html#-T
                    case "upload-file":
                        // If there is no file part in the specified URL, curl will append the local file name.
                        // NOTE that you must use a trailing / on the last directory to really prove to Curl that
                        // there is no file name or curl will think that your last directory name is the remote file name to use.
                        // That will most likely cause the upload operation to fail.
                        if (generatorContext.url.EndsWith("/")) {
                            generatorContext.url += Path.GetFileName(option.value);

                            // If this is used on an HTTP(S) server, the PUT command will be used.
                            if (generatorContext.method == null && generatorContext.url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                                generatorContext.method = "HTTPMethods.Put";
                        }
                        requestBuilder.AppendLine($"{generatorContext.indent}request.UploadStream = new FileStream(\"{option.value}\", FileMode.Open);");
                        break;

                    // https://curl.se/docs/manpage.html#--retry
                    case "retry":
                        requestBuilder.AppendLine($"{generatorContext.indent}request.MaxRetries = {option.value};");
                        break;

                    // https://curl.se/docs/manpage.html#--connect-timeout
                    case "connect-timeout":
                        requestBuilder.AppendLine($"{generatorContext.indent}request.ConnectTimeout = TimeSpan.FromSeconds({option.value})");
                        break;

                    // https://curl.se/docs/manpage.html#--basic
                    case "basic":
                        if (generatorContext.credentials == null)
                            generatorContext.credentials = new Credentials(AuthenticationTypes.Basic, null, null);
                        else
                            generatorContext.credentials = new Credentials(AuthenticationTypes.Basic, generatorContext.credentials.UserName, generatorContext.credentials.Password);
                        break;

                    // https://curl.se/docs/manpage.html#--digest
                    case "digest":
                        if (generatorContext.credentials == null)
                            generatorContext.credentials = new Credentials(AuthenticationTypes.Digest, null, null);
                        else
                            generatorContext.credentials = new Credentials(AuthenticationTypes.Digest, generatorContext.credentials.UserName, generatorContext.credentials.Password);
                        break;

                    // https://curl.se/docs/manpage.html#-u
                    case "user":
                        {
                            var cred = Parser.ParseCredentials(option.value);

                            if (generatorContext.credentials == null)
                                generatorContext.credentials = cred;
                            else
                                generatorContext.credentials = new Credentials(generatorContext.credentials.Type, cred.UserName, cred.Password);
                            break;
                        }

                    // https://curl.se/docs/manpage.html#--max-redirs
                    case "max-redirs":
                        requestBuilder.AppendLine($"{generatorContext.indent}request.MaxRedirects = {option.value}");
                        break;

                    // https://curl.se/docs/manpage.html#-e
                    case "referer":
                        requestBuilder.AppendLine($"{generatorContext.indent}request.SetHeader(\"Referer\", \"{option.value}\");");
                        break;

                    // https://curl.se/docs/manpage.html#-G
                    case "get":
                        generatorContext.method = "HTTPMethods.Get";
                        break;

                    // https://curl.se/docs/manpage.html#-I
                    case "head":
                        generatorContext.method = "HTTPMethods.Head";
                        break;

                    // https://curl.se/docs/manpage.html#-X
                    case "request":
                        for (int i = 0; i < HTTPRequest.MethodNames.Length; ++i)
                        {
                            if (HTTPRequest.MethodNames[i].Equals(option.value, StringComparison.OrdinalIgnoreCase))
                            {
                                generatorContext.method = "HTTPMethods." + Enum.GetName(typeof(HTTPMethods), (HTTPMethods)i);
                                break;
                            }
                        }
                        break;

                    // https://curl.se/docs/manpage.html#-H
                    case "header":
                        {
                            int splitIdx = option.value.IndexOf(":");
                            string header = option.value.Substring(0, splitIdx);
                            string value = option.value.Substring(splitIdx + 1).TrimStart();

                            requestBuilder.AppendLine($"{generatorContext.indent}request.AddHeader(\"{header}\", \"{value}\");");

                            if (header.Equals("content-type", StringComparison.OrdinalIgnoreCase))
                            {
                                generatorContext.dataContext.hadContentTypeHeader = true;
                            }
                            break;
                        }

                    // https://curl.se/docs/manpage.html#-d
                    // https://curl.se/docs/httpscripting.html#post
                    case "data":
                    case "data-ascii":
                    // https://curl.se/docs/manpage.html#--data-binary
                    case "data-binary":
                    // https://curl.se/docs/manpage.html#--data-raw
                    case "data-raw":
                        // Sends the specified data in a POST request to the HTTP server,
                        // in the same way that a browser does when a user has filled in an HTML form and presses the submit button.
                        // This will cause curl to pass the data to the server using the content-type application/x-www-form-urlencoded. Compare to -F, --form.

                        // --data-raw is almost the same but does not have a special interpretation of the @ character.
                        // To post data purely binary, you should instead use the --data-binary option.
                        // To URL-encode the value of a form field you may use --data-urlencode.

                        // If you repeat --data several times on the command line, curl will concatenate all the given data pieces - and put a & symbol between each data segment.

                        // curl --data "birthyear=1905&press=%20OK%20" http://www.example.com/when.cgi
                        // curl --data-urlencode "name=I am Daniel" http://www.example.com

                        generatorContext.dataContext.dataOptions.Add(option);
                        generatorContext.dataContext.addContentTypeIfNotPresent = "application/x-www-form-urlencoded";
                        generatorContext.dataContext.dataHandler = new RawUrlEncodedDataHandler();

                        if (generatorContext.method == null)
                            generatorContext.method = "HTTPMethods.Post";
                        break;

                    // https://curl.se/docs/manpage.html#--data-urlencode
                    case "data-urlencode":
                        generatorContext.dataContext.dataOptions.Add(option);
                        generatorContext.dataContext.dataHandler = new UrlEncodedFormDataHandler();

                        if (generatorContext.method == null)
                            generatorContext.method = "HTTPMethods.Post";
                        break;

                    // https://curl.se/docs/manpage.html#--form
                    case "form":
                        generatorContext.dataContext.dataOptions.Add(option);
                        generatorContext.dataContext.dataHandler = new MultipartFormDataHandler();

                        if (generatorContext.method == null)
                            generatorContext.method = "HTTPMethods.Post";
                        break;

                    // https://curl.se/docs/manpage.html#-A
                    case "user-agent":
                        requestBuilder.AppendLine($"{generatorContext.indent}request.SetHeader(\"User-Agent\", \"{option.value}\");");
                        break;

                    // https://curl.se/docs/manpage.html#-v
                    case "version":
                        requestBuilder.AppendLine($"{generatorContext.indent}Debug.Log(HTTPManager.UserAgent);");
                        break;

                    // https://curl.se/docs/manpage.html#-v
                    case "verbose":
                        requestBuilder.AppendLine($"{generatorContext.indent}HTTPManager.Logger.Level = BestHTTP.Logger.Loglevels.Information;");
                        break;

                    // https://curl.se/docs/manpage.html#--trace
                    // https://curl.se/docs/manpage.html#--trace-ascii
                    case "trace":
                    case "trace-ascii":
                        requestBuilder.AppendLine($"{generatorContext.indent}HTTPManager.Logger.Level = BestHTTP.Logger.Loglevels.All;");
                        break;

                    // https://curl.se/docs/manpage.html#-s
                    case "silent":
                        requestBuilder.AppendLine($"{generatorContext.indent}HTTPManager.Logger.Level = BestHTTP.Logger.Loglevels.None;");
                        break;

                    // https://curl.se/docs/manpage.html#--noproxy
                    case "noproxy":
                        generatorContext.proxy.noProxyList = option.value;
                        break;

                    // https://curl.se/docs/manpage.html#--proxy-anyauth
                    case "proxy-anyauth":
                        generatorContext.proxy.auth = AuthenticationTypes.Unknown;
                        break;

                    // https://curl.se/docs/manpage.html#--proxy-basic
                    case "proxy-basic":
                        generatorContext.proxy.auth = AuthenticationTypes.Basic;
                        break;

                    // https://curl.se/docs/manpage.html#--proxy-digest
                    case "proxy-digest":
                        generatorContext.proxy.auth = AuthenticationTypes.Digest;
                        break;

                    // https://curl.se/docs/manpage.html#--U
                    case "proxy-user": // <user:password>
                        {
                            var cred = Parser.ParseCredentials(option.value);

                            generatorContext.proxy.user = cred.UserName;
                            generatorContext.proxy.password = cred.Password;
                            break;
                        }

                    // https://curl.se/docs/manpage.html#-p
                    case "proxytunnel":
                        generatorContext.proxy.isTransparent = "true";
                        break;

                    // https://curl.se/docs/manpage.html#-x
                    case "proxy": // [protocol://]host[:port]
                        generatorContext.proxy.address = option.value;
                        if (generatorContext.proxy.address.IndexOf("://") < 0)
                            generatorContext.proxy.address = "http://" + generatorContext.proxy.address;

                        if (generatorContext.proxy.proxyType == null)
                        {
                            if (generatorContext.proxy.address.StartsWith("socks", StringComparison.OrdinalIgnoreCase))
                                generatorContext.proxy.proxyType = "SOCKSProxy";
                            else
                                generatorContext.proxy.proxyType = "HTTPProxy";
                        }
                        break;

                    // https://curl.se/docs/manpage.html#--socks5-hostname
                    case "socks5-hostname":
                    // https://curl.se/docs/manpage.html#--socks5
                    case "socks5":
                        generatorContext.proxy.address = option.value;
                        if (generatorContext.proxy.address.IndexOf("://") < 0)
                            generatorContext.proxy.address = "socks://" + generatorContext.proxy.address;

                        generatorContext.proxy.proxyType = "SOCKSProxy";
                        break;

                    // https://curl.se/docs/manpage.html#--url
                    case "url":
                    case null:
                        string address = option.value ?? option.option;
                        if (!address.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                            address = "http://" + address;

                        Uri uri = new Uri(address);
                        if (!string.IsNullOrEmpty(uri.UserInfo))
                        {
                            var cred = Parser.ParseCredentials(uri.UserInfo);

                            if (generatorContext.credentials == null)
                                generatorContext.credentials = cred;
                            else
                                generatorContext.credentials = new Credentials(generatorContext.credentials.Type, cred.UserName, cred.Password);

                            address = uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.Unescaped);
                        }

                        generatorContext.url = address;
                        break;
                }
            }

            string requestLine = $"{generatorContext.indent}var request = new HTTPRequest(new Uri(\"{generatorContext.url}\")";
            
            if (generatorContext.method != null && !generatorContext.method.ToUpper().EndsWith(HTTPRequest.MethodNames[(int)HTTPMethods.Get]))
                requestLine += ", " + generatorContext.method;

            if (requestUsageType == RequestUsageTypes.Callback)
                requestLine += ", OnRequestFinished";

            requestLine += ");\n\n";

            requestBuilder.Insert(0, requestLine);

            generatorContext.Apply(requestBuilder);

            if (generatorContext.credentials != null)
                requestBuilder.AppendLine($"{generatorContext.indent}request.Credentials = new Credentials(AuthenticationTypes.{generatorContext.credentials.Type.ToString()}, \"{generatorContext.credentials.UserName}\", \"{generatorContext.credentials.Password}\");");

            requestBuilder.AppendLine();
            switch (requestUsageType)
            {
                case RequestUsageTypes.Callback:
                    requestBuilder.AppendLine($"{generatorContext.indent}request.Send();");
                    break;

                case RequestUsageTypes.Coroutine:
                    requestBuilder.AppendLine($"{generatorContext.indent}yield return request.Send();");
                    break;

                case RequestUsageTypes.AsyncAwait:

                    requestBuilder.AppendLine($"{generatorContext.indent}CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();");
                    requestBuilder.AppendLine($"{generatorContext.indent}try");
                    requestBuilder.AppendLine($"{generatorContext.indent}{{");
                    requestBuilder.AppendLine($"{generatorContext.indent}{generatorContext.indent}var response = await request.GetHTTPResponseAsync(cancellationTokenSource.Token);");
                    requestBuilder.AppendLine($"{generatorContext.indent}}}");
                    requestBuilder.AppendLine($"{generatorContext.indent}catch (AsyncHTTPException ex)");
                    requestBuilder.AppendLine($"{generatorContext.indent}{{");
                    requestBuilder.AppendLine($"{generatorContext.indent}{generatorContext.indent}Debug.Log(\"Status Code: \" + ex.StatusCode);");
                    requestBuilder.AppendLine($"{generatorContext.indent}{generatorContext.indent}Debug.Log(\"Message: \" + ex.Message);");
                    requestBuilder.AppendLine($"{generatorContext.indent}{generatorContext.indent}Debug.Log(\"Content: \" + ex.Content);");
                    requestBuilder.AppendLine($"{generatorContext.indent}}}");
                    requestBuilder.AppendLine($"{generatorContext.indent}catch (TaskCanceledException)");
                    requestBuilder.AppendLine($"{generatorContext.indent}{{");
                    requestBuilder.AppendLine($"{generatorContext.indent}{generatorContext.indent}Debug.LogWarning(\"Request Canceled!\");");
                    requestBuilder.AppendLine($"{generatorContext.indent}}}");
                    requestBuilder.AppendLine($"{generatorContext.indent}catch (Exception ex)");
                    requestBuilder.AppendLine($"{generatorContext.indent}{{");
                    requestBuilder.AppendLine($"{generatorContext.indent}{generatorContext.indent}Debug.LogException(ex);");
                    requestBuilder.AppendLine($"{generatorContext.indent}}}");
                    requestBuilder.AppendLine($"{generatorContext.indent}finally");
                    requestBuilder.AppendLine($"{generatorContext.indent}{{");
                    requestBuilder.AppendLine($"{generatorContext.indent}{generatorContext.indent}cancellationTokenSource.Dispose();");
                    requestBuilder.AppendLine($"{generatorContext.indent}}}");
                    break;
            }

            var functionNameFragment = !string.IsNullOrEmpty(generatorContext.url) ? new Uri(generatorContext.url)
                                        .GetComponents(UriComponents.Host | UriComponents.Path, UriFormat.Unescaped)
                                        .Replace("/", "_")
                                        .Replace(".", "")
                                        .Replace("-", "_")
                                        .Replace(":", "_") :
                                        "Parsed_cURL_Command";

            switch (requestUsageType)
            {
                case RequestUsageTypes.Callback:
                    preRequestBuilder.AppendLine($"void SendRequest_{functionNameFragment}()");
                    requestBuilder.AppendLine("}");
                    requestBuilder.AppendLine();
                    requestBuilder.Append(OnRequestFinishedHandler);
                    break;

                case RequestUsageTypes.Coroutine:
                    preRequestBuilder.AppendLine($"IEnumerator SendRequest_{functionNameFragment}()");
                    requestBuilder.AppendLine("}");
                    break;

                case RequestUsageTypes.AsyncAwait:
                    preRequestBuilder.AppendLine($"async Task SendRequest_{functionNameFragment}()");
                    requestBuilder.AppendLine("}");
                    break;
            }
            preRequestBuilder.AppendLine("{");

            AddProxy(preRequestBuilder, generatorContext);

            return preRequestBuilder.ToString() + requestBuilder.ToString();
        }

        private static void AddProxy(StringBuilder preRequestBuilder, GeneratorContext generatorContext)
        {
            if (generatorContext.proxy.address != null)
            {
                string proxyCredentials = "null";

                Uri proxyUri = new Uri(generatorContext.proxy.address);
                if (!string.IsNullOrEmpty(proxyUri.UserInfo))
                {
                    var cred = Parser.ParseCredentials(proxyUri.UserInfo);
                    if (cred != null)
                    {
                        generatorContext.proxy.user = cred.UserName;
                        generatorContext.proxy.password = cred.Password;
                    }

                    generatorContext.proxy.address = proxyUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
                }

                if (generatorContext.proxy.user != null)
                    proxyCredentials = $"new Credentials(AuthenticationTypes.{generatorContext.proxy.auth.ToString()}, \"{generatorContext.proxy.user}\", \"{generatorContext.proxy.password}\")";

                preRequestBuilder.AppendLine($"{generatorContext.indent}HTTPManager.Proxy = new {generatorContext.proxy.proxyType}(new Uri(\"{generatorContext.proxy.address}\"), {proxyCredentials}, {generatorContext.proxy.isTransparent});");

                if (generatorContext.proxy.noProxyList != null)
                {
                    preRequestBuilder.Append($"{generatorContext.indent}HTTPManager.Proxy.Exceptions = new List<string> {{ ");

                    string[] exceptions = generatorContext.proxy.noProxyList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < exceptions.Length; ++i)
                    {
                        if (i > 0)
                            preRequestBuilder.Append(", ");
                        preRequestBuilder.Append($"\"{exceptions[i]}\"");
                    }
                    preRequestBuilder.AppendLine("};");
                }

                preRequestBuilder.AppendLine();
            }
        }

        private static string OnRequestFinishedHandler = @"private void OnRequestFinished(HTTPRequest req, HTTPResponse resp)
{
    switch (req.State)
    {
        // The request finished without any problem.
        case HTTPRequestStates.Finished:
            if (resp.IsSuccess)
            {
                // Everything went as expected!
            }
            else
            {
                Debug.LogWarning(string.Format(""Request finished Successfully, but the server sent an error.Status Code: {0}-{1} Message: {2}"",
                                                resp.StatusCode,
                                                resp.Message,
                                                resp.DataAsText));
            }
            break;

        // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
        case HTTPRequestStates.Error:
            Debug.LogError(""Request Finished with Error! "" + (req.Exception != null ? (req.Exception.Message + ""\n"" + req.Exception.StackTrace) : ""No Exception""));
            break;

        // The request aborted, initiated by the user.
        case HTTPRequestStates.Aborted:
            Debug.LogWarning(""Request Aborted!"");
            break;

        // Connecting to the server is timed out.
        case HTTPRequestStates.ConnectionTimedOut:
            Debug.LogError(""Connection Timed Out!"");
            break;

        // The request didn't finished in the given time.
        case HTTPRequestStates.TimedOut:
            Debug.LogError(""Processing the request Timed Out!"");
            break;
    }
}";
    }
}
