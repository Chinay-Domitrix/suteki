namespace Suteki
{
    class FileInput
    {
        public string Path;
        public string Source;
        public string ModuleName;

        // Initialize the FileInput
        public FileInput(string path, string source)
        {
            Path       = path;
            Source     = source;
            ModuleName = "";
        }
    }
}