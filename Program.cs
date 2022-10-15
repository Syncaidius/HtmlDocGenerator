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
        const string PACKAGE_STORE_PATH = "packages\\";

        static NugetManager _nuget;
        static HtmlContext _context;

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
            _context = HtmlContext.Load("config.xml");
            if (_context == null)
                return;

            DocGenerator generator = new DocGenerator();
            DocParser parser = new DocParser();
            _nuget = new NugetManager(PACKAGE_STORE_PATH);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            foreach (NugetDefinition nd in _context.Packages)
                _nuget.LoadPackage(nd);

            foreach (string def in _context.Definitions)
                parser.ParseXml(_context, def);

            string destPath = _context.DestinationPath;
            if(!Path.IsPathFullyQualified(destPath))
                destPath = Path.GetFullPath(destPath);

            //generator.Generate(context, docs, $"{destPath}\\", $"{destPath}\\index.html");
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string[] aParts = args.Name.Split(',');
            string aName = aParts[0];

            if (!_context.Assemblies.TryGetValue(aName, out DocAssembly da))
            {
                FileInfo requestingInfo = new FileInfo(args.RequestingAssembly.Location);
                FileInfo assemblyInfo = new FileInfo($"{requestingInfo.Directory.FullName}\\{aName}.dll");

                if (assemblyInfo.Exists)
                {
                    da = new DocAssembly()
                    {
                        Name = aName,
                        FilePath = assemblyInfo.FullName,
                        Assembly = Assembly.LoadFile(assemblyInfo.FullName)
                    };

                    _context.Assemblies.Add(aName, da);
                }
                else
                {
                    // Check nuget instead
                    if (_nuget.TryGetAssembly(aName, out Assembly nugetAssembly))
                    {
                        da = new DocAssembly()
                        {
                            Name = aName,
                            FilePath = nugetAssembly.Location,
                            Assembly = nugetAssembly
                        };
                    }
                    else
                    {
                        Console.WriteLine($"Error: Missing assembly '{args.Name}'");
                    }
                }
            }

            return da?.Assembly;
        }
    }
}