namespace Suteki
{
    class FileInput
    {
        public string Path;
        public string Source;
        public string ModuleName;
        public string HeaderOutput;
        public string SourceOutput;

        // Initialize the FileInput
        public FileInput(string path, string source)
        {
            Path         = path;
            Source       = source;
            ModuleName   = "";
            HeaderOutput = "";
            SourceOutput = "";
        }
    }
}