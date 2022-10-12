using HtmlDocGenerator;
using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Xml.Linq;

namespace HtmlDocGenerator // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();
        const string PACKAGE_STORE_PATH = "packages\\";

        static NugetDownloader _nuget;

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
            HtmlContext context = HtmlContext.Load("config.xml");
            if (context == null)
                return;

            DocGenerator generator = new DocGenerator();
            DocParser parser = new DocParser();
            _nuget = new NugetDownloader(PACKAGE_STORE_PATH);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            foreach (NugetDefinition nd in context.Packages)
                LoadNugetPackage(nd);

            List<DocData> docs = new List<DocData>();

            foreach (string def in context.Definitions)
            {
                FileInfo info = new FileInfo(def);
                if (!info.Exists)
                {
                    Console.WriteLine($"The specified file does not exist: {info.FullName}");
                    return;
                }

                DocData doc = null;
                using (FileStream stream = new FileStream(info.FullName, FileMode.Open, FileAccess.Read))
                    doc = parser.ParseXml(stream);

                doc.Assembly = LoadAssembly($"{info.Directory}\\{doc.AssemblyName}.dll");
                if (doc.Assembly != null)
                    parser.ScanAssembly(context, doc, doc.Assembly);

                docs.Add(doc);
            }

            string destPath = context.DestinationPath;
            if(!Path.IsPathFullyQualified(destPath))
                destPath = Path.GetFullPath(destPath);

            generator.Generate(context, docs, $"{destPath}\\", $"{destPath}\\index.html");
        }

        private static void LoadNugetPackage(NugetDefinition nd)
        {
            Task<string> packageTask = _nuget.GetPackage(nd);
            packageTask.Wait();
            try
            {
                string aPath = $"{packageTask.Result}lib\\{nd.Framework}\\{nd.Name}.dll";
                using (FileStream stream = new FileStream(aPath, FileMode.Open, FileAccess.Read))
                {
                    byte[] assemblyBytes = new byte[stream.Length];
                    stream.Read(assemblyBytes, 0, (int)stream.Length);

                    Assembly packageAssembly = Assembly.Load(assemblyBytes);
                    _loadedAssemblies.Add(nd.Name, packageAssembly);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Failed to load package '{nd.Name}' for framework '{nd.Framework}': {ex.Message}");
            }
        }

        private static Assembly LoadAssembly(string path)
        {
            if (!File.Exists(path))
                return null;

            path = Path.GetFullPath(path);
            FileInfo assemblyInfo = new FileInfo(path);
            string aFileName = assemblyInfo.Name.Replace(assemblyInfo.Extension, "");

            if (!_loadedAssemblies.TryGetValue(aFileName, out Assembly assembly))
            {
                if (assemblyInfo.Exists)
                {
                    assembly = Assembly.LoadFile(path);
                    string[] aParts = assembly.FullName.Split(',');
                    string aName = aParts[0];

                    _loadedAssemblies.Add(aName, assembly);
                }
                else
                {

                }
            }

            return assembly;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string[] aParts = args.Name.Split(',');
            string aName = aParts[0];

            if (!_loadedAssemblies.TryGetValue(aName, out Assembly assembly))
            {
                FileInfo requestingInfo = new FileInfo(args.RequestingAssembly.Location);
                string aVersion = aParts[1].Replace("Version=", "");
                FileInfo assemblyInfo = new FileInfo($"{requestingInfo.Directory.FullName}\\{aName}.dll");

                if (assemblyInfo.Exists)
                {
                    assembly = Assembly.LoadFile(assemblyInfo.FullName);
                    _loadedAssemblies.Add(aName, assembly);
                    return assembly;
                }
                else
                {
                    // TODO notify of missing assembly.
                    Console.WriteLine($"Error: Missing assembly '{args.Name}'");
                }

                return null;
            }

            return assembly;
        }
    }
}