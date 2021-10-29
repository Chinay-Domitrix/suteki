using System.Collections.Generic;

namespace Suteki
{
    enum TokenType
    {
        Error,
        End,

        Identifier,
        String,
        Number,

        LeftParenthesis,
        RightParenthesis,
        LeftBrace,
        RightBrace,
        Comma,
        Semicolon,
        Dot,

        Export,
        Import,

        Return,
    }

    class Token
    {
        public TokenType Type;
        public object    Data;
        public uint      Line;
        public uint      Column;

        // Initialize the Token
        public Token(TokenType type, object data, uint line, uint column)
        {
            Type     = type;
            Data     = data;
            Line     = line;
            Column   = column;
        }

        // Initialize the Token
        public Token(TokenType type, uint line, uint column)
        {
            Type   = type;
            Line   = line;
            Column = column;
        }
    }

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

            CurrentToken = new Token(TokenType.Number, number, Line, Column);
            return TokenType.Number;
        }

        // Make string Token
        public TokenType MakeStringToken()
        {
            while (Source[Current] != '"')
                Advance();

            if (Source[Current] == '\0')
            {
                CurrentToken = new Token(TokenType.Error, "Unterminated string.", Line, Column);
                return TokenType.Error;
            }

            Advance();
            CurrentToken = new Token(TokenType.String, Source.Substring(Start, Current - Start), Line, Column);
            return TokenType.String;
        }

        // Make identifier Token
        public TokenType MakeIdentifierToken()
        {
            while (char.IsLetterOrDigit(Source[Current]) || Source[Current] == '_')
                Advance();

            string identifier = Source.Substring(Start, Current - Start);

            if (Keywords.ContainsKey(identifier))
            {
                CurrentToken = new Token(Keywords[identifier], Line, Column);
                return Keywords[identifier];
            }

            CurrentToken = new Token(TokenType.Identifier, identifier, Line, Column);
            return TokenType.Identifier;
        }

        // Scan Token
        public TokenType Scan()
        {
            SkipWhitespace();
            Start         = Current;
            PreviousToken = CurrentToken;

            // Source end?
            if (Source[Current] == '\0')
            {
                CurrentToken = new Token(TokenType.End, Line, Column);
                return TokenType.End;
            }

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
                {
                    CurrentToken = new Token(TokenType.LeftParenthesis, Line, Column);
                    return TokenType.LeftParenthesis;
                }

                case ')':
                {
                    CurrentToken = new Token(TokenType.RightParenthesis, Line, Column);
                    return TokenType.RightParenthesis;
                }

                case '{':
                {
                    CurrentToken = new Token(TokenType.LeftBrace, Line, Column);
                    return TokenType.LeftBrace;
                }

                case '}':
                {
                    CurrentToken = new Token(TokenType.RightBrace, Line, Column);
                    return TokenType.RightBrace;
                }

                case ',':
                {
                    CurrentToken = new Token(TokenType.Comma, Line, Column);
                    return TokenType.Comma;
                }

                case ';':
                {
                    CurrentToken = new Token(TokenType.Semicolon, Line, Column);
                    return TokenType.Semicolon;
                }

                case '.':
                {
                    CurrentToken = new Token(TokenType.Dot, Line, Column);
                    return TokenType.Dot;
                }

                case '"':
                    return MakeStringToken();
            }

            // Unexpected character
            CurrentToken = new Token(TokenType.Error, "Unexpected character.", Line, Column);
            return TokenType.Error;
        }
    }
}