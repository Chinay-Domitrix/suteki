using System;
using System.Collections.Generic;
using System.IO;

namespace Suteki
{
    class Program
    {
        public static List<FileInput> Inputs = new List<FileInput>();

        // Parse arguments
        public static bool ParseArguments(string[] arguments)
        {
            for (int i = 0; i < arguments.Length; ++i)
            {
                string argument = arguments[i];

                // Parse argument
                if (argument.Contains(".su"))
                    Inputs.Add(new FileInput(argument, File.ReadAllText(argument) + '\0'));
                else
                {
                    Utilities.WriteColor(ConsoleColor.Red, "Error: ", ConsoleColor.White, $"Invalid option '{argument}'.\n");
                    return false;
                }
            }

            // Make sure we have inputs
            if (Inputs.Count == 0)
            {
                Utilities.WriteColor(ConsoleColor.Red, "Error: ", ConsoleColor.White, "No input files.\n");
                return false;
            }

            return true;
        }

        static void Main(string[] args)
        {
            // Parse command line arguments
            if (!ParseArguments(args))
                return;

            // Compile
            Compiler compiler = new Compiler();
            
            if (compiler.Compile())
                Utilities.WriteColor(ConsoleColor.Green, "Everything was successfully compiled.\n");
        }
    }
}
