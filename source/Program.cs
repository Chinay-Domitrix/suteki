using System;
using System.Collections.Generic;
using System.IO;

namespace Suteki
{
    class Program
    {
        public static List<FileInput> Inputs     = new List<FileInput>();
        public static string          OutputPath = "";
        public static string          Version    = "v0.1.0";

        // Parse arguments
        public static bool ParseArguments(string[] arguments)
        {
            for (int i = 0; i < arguments.Length; ++i)
            {
                string argument = arguments[i];

                // Parse argument
                if (argument == "--output")
                {
                    if (OutputPath != "")
                    {
                        Utilities.WriteColor(ConsoleColor.Red, "Error: ", ConsoleColor.White, $"Output path was already specified.\n");
                        return false;
                    }

                    OutputPath = arguments[++i];

                    if (!Directory.Exists(OutputPath))
                    {
                        Utilities.WriteColor(ConsoleColor.Red, "Error: ", ConsoleColor.White, $"Directory '{OutputPath}' does not exists.\n");
                        return false;
                    }
                }
                else if (argument.Contains(".su"))
                {
                    if (!File.Exists(argument))
                    {
                        Utilities.WriteColor(ConsoleColor.Red, "Error: ", ConsoleColor.White, $"File '{argument}' does not exists.\n");
                        return false;
                    }

                    Inputs.Add(new FileInput(argument, File.ReadAllText(argument) + '\0'));
                }
                else
                {
                    Utilities.WriteColor(ConsoleColor.Red, "Error: ", ConsoleColor.White, $"Invalid argument '{argument}'.\n");
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
            
            if (compiler.Start())
                Utilities.WriteColor(ConsoleColor.Green, "Everything was successfully compiled.\n");

            // Link
            Linker.Start(compiler);
        }
    }
}
