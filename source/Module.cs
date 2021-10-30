using System.Collections.Generic;

namespace Suteki
{
    class Module
    {
        public Dictionary<string, Symbol> Symbols;

        // Initialize the Module
        public Module()
        {
            Symbols = new Dictionary<string, Symbol>();
        }
    }
}