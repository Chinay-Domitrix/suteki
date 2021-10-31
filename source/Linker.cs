using System;
using System.IO;

namespace Suteki
{
    class Linker
    {
        public static string FileNote = $"// Generated by Suteki Compiler {Program.Version}\n";

        // Get path 
        public static string getPath(string path, bool makeDirectory = true)
        {
            string fileName = Path.GetFileName(path);

            // Format the path
            path = path.Replace("../", "").Replace(fileName, "");

            if (!path.StartsWith('_'))
                path = '_' + path;

            path = path + "_" + fileName.Replace(".su", "");

            // Make the directory
            if (makeDirectory)
            {
                string directory = Program.OutputPath + Path.GetDirectoryName(path);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }

            return path;
        }

        public static void Start(Compiler compiler)
        {
            string outputPath = (Program.OutputPath.EndsWith("/") ? Program.OutputPath : Program.OutputPath + "/");
            string globalFile = FileNote;
            string entryFile  = FileNote;

            // Write files
            foreach (FileInput input in Program.Inputs)
            {
                // Get input information
                string fileName   = getPath(input.Path);
                string headerName = fileName.Replace("/", "");
                string path       = outputPath + fileName;

                // No export?
                if (input.ModuleName == "")
                    globalFile += $"#include <{fileName}.h>\n";
                
                // Add header guards
                string newHeaderOutput = FileNote;
                newHeaderOutput += $"#ifndef {headerName.ToUpper()}_H\n#define {headerName.ToUpper()}_H\n\n";
                newHeaderOutput +=  "#include <global.h>\n\n";
                newHeaderOutput +=  input.HeaderOutput;
                newHeaderOutput +=  "\n#endif";

                // Add includes
                string newSourceOutput  =  FileNote;
                newSourceOutput        += $"#include <{fileName}.h>\n";

                foreach (string moduleName in input.Imports)
                {
                    FileInput moduleFile = Program.Inputs[compiler.Modules[moduleName].FileInput];
                    string    modulePath = getPath(moduleFile.Path);

                    newSourceOutput += $"#include <{modulePath}.h>\n";
                }

                newSourceOutput += '\n' + input.SourceOutput;

                // Remove the spacing at the end
                if (newSourceOutput.EndsWith("\n\n"))
                    newSourceOutput = newSourceOutput.Substring(0, newSourceOutput.Length - 2);
                else if (newSourceOutput.EndsWith('\n'))
                    newSourceOutput = newSourceOutput.Substring(0, newSourceOutput.Length - 1);

                // Write files
                File.WriteAllText(path + ".h", newHeaderOutput);
                File.WriteAllText(path + ".c", newSourceOutput);
            }

            // Include main file and global file
            string mainFilePath = getPath(Program.Inputs[compiler.MainFile].Path, false);

            entryFile +=  "#include <global.h>\n";
            entryFile += $"#include <{mainFilePath}.h>\n\n";

            // Write main function
            entryFile += "int main()\n{\n";
            entryFile += "\treturn su_main();\n";
            entryFile += "}";

            // Write global file
            File.WriteAllText(outputPath + "global.h", globalFile.Substring(0, globalFile.Length - 1));

            // Write entry file
            File.WriteAllText(outputPath + "entry.c", entryFile);
        }
    }
}