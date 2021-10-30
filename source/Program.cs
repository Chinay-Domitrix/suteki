using System;
using System.Collections.Generic;
using System.IO;

namespace Suteki
{
    class Program
    {
        public static List<FileInput> Inputs;

        static void Main(string[] args)
        {
            // Read inputs
            Inputs = new List<FileInput>();
            Inputs.Add(new FileInput("tests/main.su",    File.ReadAllText("../tests/main.su")    + '\0'));
            Inputs.Add(new FileInput("tests/another.su", File.ReadAllText("../tests/another.su") + '\0'));

            // Compile
            Compiler compiler = new Compiler();
            
            if (compiler.Compile())
                Utilities.WriteColor(ConsoleColor.Green, "Everything was successfully compiled.\n");
        }
    }
}
