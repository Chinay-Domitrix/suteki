namespace Suteki
{
    enum SymbolType
    {
        Function,
        Variable,
    }

    class Symbol
    {
        public SymbolType Type;
        public string     Name;

        // Initialize the Symbol
        public Symbol()
        {
        }

        public Symbol(SymbolType type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}