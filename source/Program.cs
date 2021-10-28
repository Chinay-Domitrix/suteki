using System;

namespace Suteki
{
    class Program
    {
        static void Main(string[] args)
        {
            Scanner scanner = new Scanner("export");

            Token t = scanner.Scan();
            Console.WriteLine($"[{t.Line}:{t.Column}] {t.Type} {t.Data}");

            Console.WriteLine("Hello World!");
        }
    }
}
