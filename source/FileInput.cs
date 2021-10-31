using System.Collections.Generic;

namespace Suteki
{
    class FileInput
    {
        public string       Path;
        public string       Source;
        public string       ModuleName;
        public string       HeaderOutput;
        public string       SourceOutput;
        public List<string> Imports;

        // Initialize the FileInput
        public FileInput(string path, string source)
        {
            Path         = path;
            Source       = source;
            ModuleName   = "";
            HeaderOutput = "";
            SourceOutput = "";
            Imports      = new List<string>();
        }
    }
}