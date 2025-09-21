using System;

namespace BestHTTP.Addons.cURLParser.Editor.Utils
{
    public struct BeginEndPeak : IDisposable
    {
        private PeakableCharacterStream stream;
        public BeginEndPeak(PeakableCharacterStream stream)
        {
            this.stream = stream;
            this.stream?.BeginPeek();
        }

        public void Dispose()
        {
            this.stream?.EndPeek();
        }
    }

    public sealed class PeakableCharacterStream
    {
        public int Length => this._original.Length;
        public bool IsEOF => this._position >= this._original.Length;

        public int Position { get => this._position; set => this._position = value; }
        private int _position;

        public string OriginalString { get => this._original; }
        public char[] Characters { get => this._chars; }

        public char Current { get => this._chars[this._position]; }

        private int _savedPosition = -1;

        private string _original;
        private char[] _chars;

        public PeakableCharacterStream(string originalString)
        {
            if (originalString == null)
                throw new NullReferenceException(nameof(originalString));

            this._original = originalString;
            this._chars = originalString.ToCharArray();
        }

        public void BeginPeek()
        {
            if (this._savedPosition < 0)
                this._savedPosition = this._position;
        }

        public void EndPeek()
        {
            if (this._savedPosition >= 0)
            {
                this._position = this._savedPosition;
                this._savedPosition = -1;
            }
        }

        public char Advance()
        {
            if (this._position >= this._original.Length)
                throw new IndexOutOfRangeException($"{this._position} >= {this._original.Length}");

            return this._original[this._position++];
        }

        public void SkipWhiteSpace()
        {
            while (this._position < this.Length && char.IsWhiteSpace(this._chars[this._position]))
                this._position++;
        }

        public void SkipUntilWhiteSpace()
        {
            while (this._position < this.Length && !char.IsWhiteSpace(this._chars[this._position]))
                this._position++;
        }
    }
}
