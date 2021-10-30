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
    }
}