using System.Collections.Generic;

namespace Suteki
{
    class Scanner 
    {
        public string                        Source;
        public int                           Start;
        public int                           Current;
        public uint                          Line;
        public uint                          Column;
        public Dictionary<string, TokenType> Keywords;
        public Token                         PreviousToken;
        public Token                         CurrentToken;

        // Initialize the Scanner
        public Scanner(string source)
        {
            Source   = source;
            Start    = 0;
            Current  = 0;
            Line     = 1;
            Column   = 1;
            Keywords = new Dictionary<string, TokenType>();

            Keywords.Add("void",   TokenType.Void);
            Keywords.Add("bool",   TokenType.Bool);
            Keywords.Add("ubyte",  TokenType.UByte);
            Keywords.Add("ushort", TokenType.UShort);
            Keywords.Add("uint",   TokenType.UInt);
            Keywords.Add("ulong",  TokenType.ULong);
            Keywords.Add("byte",   TokenType.Byte);
            Keywords.Add("short",  TokenType.Short);
            Keywords.Add("int",    TokenType.Int);
            Keywords.Add("long",   TokenType.Long);
            Keywords.Add("single", TokenType.Single);
            Keywords.Add("double", TokenType.Double);

            Keywords.Add("export", TokenType.Export);
            Keywords.Add("import", TokenType.Import);

            Keywords.Add("return", TokenType.Return);
        }

        // Advance current character
        public char Advance()
        {
            ++Column;

            if (Current <= Source.Length)
                ++Current;

            return Source[Current - 1];
        }

        // Skip whitespace
        public void SkipWhitespace()
        {
            for (;;)
            {
                switch (Source[Current])
                {
                    case '\n':
                    {
                        ++Line;
                        Column = 0;

                        Advance();
                        break;
                    }

                    case ' ':
                    case '\r':
                    case '\t':
                    {
                        Advance();
                        break;
                    }

                    default:
                        return;
                }
            }
        }

        // Make Token
        public TokenType MakeToken(TokenType type)
        {
            CurrentToken = new Token(type, Source.Substring(Start, Current - Start), Line, Column);
            return type;
        }

        // Make Token with custom data
        public TokenType MakeToken(TokenType type, object data)
        {
            CurrentToken = new Token(type, data, Line, Column);
            return type;
        }

        // Make error Token
        public TokenType MakeToken(string message)
        {
            CurrentToken = new Token(TokenType.Error, message, Line, Column);
            return TokenType.Error;
        }

        // Make number Token
        public TokenType MakeNumberToken()
        {
            while (char.IsDigit(Source[Current]))
                Advance();

            if (Source[Current] == '.')
            {
                Advance();

                while (char.IsDigit(Source[Current]))
                    Advance();
            }

            string numberAsString = Source.Substring(Start, Current - Start);
            double number         = double.Parse(numberAsString);

            return MakeToken(TokenType.Number, number);
        }

        // Make string Token
        public TokenType MakeStringToken()
        {
            while (Source[Current] != '"')
                Advance();

            if (Source[Current] == '\0')
                return MakeToken("Unterminated string.");

            Advance();
            return MakeToken(TokenType.String);
        }

        // Make identifier Token
        public TokenType MakeIdentifierToken()
        {
            while (char.IsLetterOrDigit(Source[Current]) || Source[Current] == '_')
                Advance();

            string identifier = Source.Substring(Start, Current - Start);

            if (Keywords.ContainsKey(identifier))
                return MakeToken(Keywords[identifier]);

            return MakeToken(TokenType.Identifier);
        }

        // Scan Token
        public TokenType Scan()
        {
            SkipWhitespace();
            Start         = Current;
            PreviousToken = CurrentToken;

            // Source end?
            if (Source[Current] == '\0')
                return MakeToken(TokenType.End);

            char character = Advance();

            // Make multiple character tokens
            if (char.IsDigit(character))
                return MakeNumberToken();

            if (char.IsLetter(character) || character == '_')
                return MakeIdentifierToken();

            // Make single character tokens
            switch (character)
            {
                case '(':
                    return MakeToken(TokenType.LeftParenthesis);

                case ')':
                    return MakeToken(TokenType.RightParenthesis);

                case '{':
                    return MakeToken(TokenType.LeftBrace);

                case '}':
                    return MakeToken(TokenType.RightBrace);

                case ',':
                    return MakeToken(TokenType.Comma);

                case ';':
                    return MakeToken(TokenType.Semicolon);

                case '.':
                    return MakeToken(TokenType.Dot);

                case '=':
                    return MakeToken(TokenType.Equal);

                case '"':
                    return MakeStringToken();
            }

            // Unexpected character
            return MakeToken("Unexpected character.");
        }
    }
}