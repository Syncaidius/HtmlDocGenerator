using HtmlDocGenerator;
using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();
        const string PACKAGE_STORE_PATH = "packages\\";

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
            GeneratorConfig config = GeneratorConfig.Load("config.xml");
            DocParser parser = new DocParser();
            HtmlGenerator generator = new HtmlGenerator();
            NugetDownloader nuget = new NugetDownloader(PACKAGE_STORE_PATH);

            foreach (NugetDefinition nd in config.Packages)
                nuget.GetPackage(nd);

            Array.Resize(ref args, 1);
            args[0] = "src\\Molten.Engine.xml";

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
            string indexPath = $"docs\\template.html";
            if (!File.Exists(indexPath))
            {
                Console.WriteLine($"Index.html template not found: {indexPath}");
                return;
            }

            DocData doc = null;
            using (FileStream stream = new FileStream(info.FullName, FileMode.Open, FileAccess.Read))
                doc = parser.ParseXml(stream);

            Assembly assembly = ScanAssembly($"{info.Directory}\\{doc.Assembly.Name}.dll");
            if(assembly != null)
                parser.ScanAssembly(doc, assembly);

            FileInfo exeInfo = new FileInfo(Assembly.GetEntryAssembly().Location);

            generator.Generate(doc, indexPath, $"{exeInfo.DirectoryName}\\docs\\");
        }

        private static Assembly ScanAssembly(string path)
        {
            FileInfo assemblyInfo = new FileInfo(path);
            if (assemblyInfo.Exists)
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                Assembly assembly = Assembly.LoadFile(assemblyInfo.FullName);

                return assembly;
            }

            return null;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (!_loadedAssemblies.TryGetValue(args.Name, out Assembly assembly))
            {
                FileInfo requestingInfo = new FileInfo(args.RequestingAssembly.Location);

                string[] aParts = args.Name.Split(',');
                FileInfo assemblyInfo = new FileInfo($"{requestingInfo.Directory.FullName}\\{aParts[0]}.dll");

                if (assemblyInfo.Exists)
                {
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                    assembly = Assembly.LoadFile(assemblyInfo.FullName);
                    _loadedAssemblies.Add(assembly.FullName, assembly);
                    return assembly;
                }

                return null;
            }

            return assembly;
        }
    }
}