using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class NugetDownloader
    {
        string _apiUrl;
        HttpClient _http;
        string _storePath;
        static readonly string[] dotNetVersions =
        {
            "netstandard2.0",
            "netstandard1.3",
            "netstandard1.0",
            "net4.5",
            "net4.0",
            "net3.5",
            "net2.0",
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageStorePath">The directory path used for storing downloaded nuget packages.</param>
        public NugetDownloader(string packageStorePath, string apiUrl = "https://api.nuget.org/v3-flatcontainer/")
        {
            _http = new HttpClient();
            _apiUrl = apiUrl;
            _storePath = packageStorePath;
        }

        public async Task<string> GetPackage(NugetDefinition def)
        {
            string name = def.Name.ToLower();
            string version = def.Version.ToLower();
            int bufferSize = 31768;
            byte[] buffer = new byte[bufferSize];

            string pathVersion = version.Replace('.', '_');
            string destPath = $"{_storePath}\\{name}\\{pathVersion}\\";
            string nuspecPath = $"{destPath}\\{name}.nuspec";


            if (File.Exists(nuspecPath))
            {
                Console.WriteLine($"Found existing package for '{def.Name}'");
                // TODO check for any missing package files by parsing .nuspec dependencies targetframework attribute with lib\\target framework folder name\\packageName.dll
                return destPath;
            }

            // TODO check if we've already downloaded the package.

            string packageUrl = $"{_apiUrl}{name}/{version}/{name}.{version}.nupkg";

            using (MemoryStream result = new MemoryStream())
            {
                // Get response header first so we can get the size of the download
                using (HttpResponseMessage response = await _http.GetAsync(packageUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    long? byteCount = response.Content.Headers.ContentLength;
                    long totalBytesRead = 0;

                    using (Task<Stream> packageStream = response.Content.ReadAsStreamAsync())
                    {
                        if (packageStream.Result != null)
                        {
                            int bytesRead = 0;
                            while ((bytesRead = await packageStream.Result.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await result.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                            }

                            Console.WriteLine($"Downloading {name} - {version}: {totalBytesRead} / {byteCount} bytes");
                        }

                        Console.WriteLine($"Finished {name} - {version}: {totalBytesRead} / {byteCount} bytes");
                    }
                }

                // Unpack downloaded .nupkg file.
                using (ZipArchive archive = new ZipArchive(result, ZipArchiveMode.Read))
                {
                    foreach(ZipArchiveEntry entry in archive.Entries)
                    {
                        FileInfo info = new FileInfo($"{destPath}{entry.FullName}");

                        if (!info.Directory.Exists)
                            info.Directory.Create();

                        entry.ExtractToFile(info.FullName, true);

                        Console.WriteLine($"Extracted '{entry.FullName}' - {entry.Length}  ({entry.CompressedLength} compressed) bytes");
                    }
                }

                return destPath;
            }
        }
    }
}
