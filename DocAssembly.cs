using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HtmlDocGenerator
{
    public class DocAssembly
    {
        public string Name { get; set; }

        public Assembly Assembly { get; set; }

        public string FilePath { get; set; }

        /// <summary>
        /// The root node of the XML documentation associated with the current <see cref="DocAssembly"/>.
        /// </summary>
        public XmlNode XmlRoot { get; set; }

        public bool Load(DocContext context)
        {
            if (!File.Exists(FilePath))
                return false;

            FilePath = Path.GetFullPath(FilePath);
            FileInfo assemblyInfo = new FileInfo(FilePath);

            if (Assembly == null)
            {
                if (assemblyInfo.Exists)
                {
                    try
                    {
                        Assembly = Assembly.LoadFile(FilePath);
                    }
                    catch (Exception ex)
                    {
                        context.Error($"Failed to load assembly '{Name}': {ex.Message}");
                        return false;
                    }
                }
                else
                {
                    context.Error($"Failed to load assembly '{Name}': '{FilePath}' does not exist");
                }
            }

            return true;
        }
    }
}
