using System;

namespace Suteki
{
    public static class Utilities
    {
        public static void WriteColor(params dynamic[] content)
        {
            for (int i = 0; i < content.Length; ++i)
            {
                Console.ForegroundColor = (ConsoleColor)content[i];
                Console.Write(content[++i].ToString());
            }

            Console.ResetColor();
        }
    }
}