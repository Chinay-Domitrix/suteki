using System.Collections.Generic;

namespace Suteki
{
    class Module
    {
        public Dictionary<string, Symbol> Symbols;
        public int                        FileInput;

        // Initialize the Module
        public Module()
        {
            Symbols   = new Dictionary<string, Symbol>();
            FileInput = -1;
        }
    }
}