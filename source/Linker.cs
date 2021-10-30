using System;
using System.IO;

namespace Suteki
{
    class Linker
    {
        public static void Start(Compiler compiler)
        {
            string outputPath = (Program.OutputPath.EndsWith("/") ? Program.OutputPath : Program.OutputPath + "/");

            // Write files
            foreach (FileInput input in Program.Inputs)
            {
                // Get input information
                string headerName = input.ModuleName.Replace(".", "_");
                string fileName   = Path.GetFileName(input.Path).Replace(".su", "");
                string path       = outputPath + fileName;

                // Make sure header name isn't empty
                if (headerName == "")
                    headerName = fileName;
                
                // Add header guards
                string newHeaderOutput = $"#ifndef {headerName.ToUpper()}_H\n#define {headerName.ToUpper()}_H\n\n";
                newHeaderOutput += input.HeaderOutput;
                newHeaderOutput += "\n#endif";

                // Add includes
                string newSourceOutput = $"#include \"{fileName}.h\"";
                newSourceOutput += input.SourceOutput;

                // Write files
                File.WriteAllText(path + ".h", newHeaderOutput);
                File.WriteAllText(path + ".c", newSourceOutput);
            }
        }
    }
}