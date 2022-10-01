using HtmlDocGenerator;
using System;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Run(args);

#if DEBUG
            Console.WriteLine("Press any key to exit...");
#endif
            Console.ReadKey();
        }

        private static void Run(string[] args)
        {
            DocParser parser = new DocParser();
            Array.Resize(ref args, 1);
            args[0] = "Molten.Engine.xml";

            if (args.Length == 0)
            {
                Console.WriteLine("No arguments provided");
                return;
            }

            FileInfo info = new FileInfo(args[0]);
            if (!info.Exists)
            {
                Console.WriteLine($"The specified file does not exist: {info.FullName}");
                return;
            }

            // Check if an index.html template exists in the same directory
            string indexPath = $"{info.Directory.FullName}\\index.html";
            if (!File.Exists(indexPath))
            {
                Console.WriteLine($"Index.html template not found: {indexPath}");
                return;
            }

            DocData doc = null;
            using (FileStream stream = new FileStream(info.FullName, FileMode.Open, FileAccess.Read))
                doc = parser.Parse(stream);
        }
    }
}