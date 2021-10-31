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

        Void,
        Bool,
        UByte,
        UShort,
        UInt,
        ULong,
        Byte,
        Short,
        Int,
        Long,
        Single,
        Double,

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
}