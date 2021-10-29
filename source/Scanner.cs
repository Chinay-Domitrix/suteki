using System;
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
        public Token MakeNumberToken()
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

            return new Token(TokenType.Number, number, Line, Column);
        }

        // Make string Token
        public Token MakeStringToken()
        {
            while (Source[Current] != '"')
                Advance();

            if (Source[Current] == '\0')
                return new Token(TokenType.Error, "Unterminated string.", Line, Column);

            Advance();
            return new Token(TokenType.String, Source.Substring(Start, Current - Start), Line, Column);
        }

        // Make identifier Token
        public Token MakeIdentifierToken()
        {
            while (char.IsLetterOrDigit(Source[Current]) || Source[Current] == '_')
                Advance();

            string identifier = Source.Substring(Start, Current - Start);

            if (Keywords.ContainsKey(identifier))
                return new Token(Keywords[identifier], Line, Column);

            return new Token(TokenType.Identifier, identifier, Line, Column);
        }

        // Scan Token
        public Token Scan()
        {
            SkipWhitespace();
            Start = Current;

            // Source end?
            if (Source[Current] == '\0')
                return new Token(TokenType.End, Line, Column);

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
                    return new Token(TokenType.LeftParenthesis, Line, Column);

                case ')':
                    return new Token(TokenType.RightParenthesis, Line, Column);

                case '{':
                    return new Token(TokenType.LeftBrace, Line, Column);

                case '}':
                    return new Token(TokenType.RightBrace, Line, Column);

                case ',':
                    return new Token(TokenType.Comma, Line, Column);

                case ';':
                    return new Token(TokenType.Semicolon, Line, Column);

                case '"':
                    return MakeStringToken();
            }

            // Unexpected character
            return new Token(TokenType.Error, "Unexpected character.", Line, Column);
        }
    }
}