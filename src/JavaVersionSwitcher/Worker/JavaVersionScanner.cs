using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

#pragma warning disable CA1822
namespace JavaVersionSwitcher.Worker
{
    public class JavaInstallationScanner
    {
        public async Task<IEnumerable<JavaInstallation>> Scan(bool force = false)
        {
            if (!force && HasRecentCacheData())
            {
                try
                {
                    return await LoadCacheData();
                }
                catch {/* TODO: Log?? */}
            }

            var data = (await ForceScan()).ToList();

            try
            {
                await SaveCacheData(data);
            } catch {/* TODO: Log?? */}

            return data;
        }

        private async Task SaveCacheData(IEnumerable<JavaInstallation> data)
        {
            var file = GetCacheFileName();
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            var serializer = new XmlSerializer(typeof(JavaInstallation[]));
            await using var stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None);
            await using var writer = new StreamWriter(stream, Encoding.UTF8);
            
            serializer.Serialize(writer, data.ToArray());
        }

        private async Task<IEnumerable<JavaInstallation>> LoadCacheData()
        {
            var file = GetCacheFileName();
            var serializer = new XmlSerializer(typeof(JavaInstallation[]));
            await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            
            return (JavaInstallation[])serializer.Deserialize(reader);
        }

        private bool HasRecentCacheData()
        {
            var fileName = GetCacheFileName();
            var file = new FileInfo(fileName);
            if (!file.Exists)
            {
                return false;
            }

            // TODO: Configure Timeout?
            return file.LastWriteTime.AddDays(7) >= DateTime.Now;
        }

        private string GetCacheFileName()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var app = typeof(JavaInstallation).Assembly.GetName().Name ?? "JavaVersionSwitcher";
            const string file = "installations.xml";
            
            return Path.Combine(appData, app, file);
        }

        private async Task<IEnumerable<JavaInstallation>> ForceScan()
        {
            // todo configurable start paths?
            var start = new[]
                {
                    Environment.ExpandEnvironmentVariables("%ProgramW6432%"),
                    Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%")
                }.Distinct()
                .Where(x => !string.IsNullOrEmpty(x));
            var javaExeFiles = await FindFileRecursive(start);
            var result = new List<JavaInstallation>();
            foreach (var javaExeFile in javaExeFiles)
            {
                var (version, fullVersion) = await GetVersion(javaExeFile).ConfigureAwait(false); 
                var installation = new JavaInstallation
                {
                    Location = Directory.GetParent(javaExeFile)?.Parent?.FullName,
                    Version = version,
                    FullVersion = fullVersion
                };
                result.Add(installation);
            }

            return result;
        }

        private Task<IEnumerable<string>> FindFileRecursive(IEnumerable<string> startPaths)
        {
            return Task.Factory.StartNew<IEnumerable<string>>(() =>
            {
                var queue = new Queue<string>();
                startPaths.ToList().ForEach(queue.Enqueue);

                var results = new List<string>();
                while (queue.TryDequeue(out var item))
                {
                    // log ? Console.WriteLine("Checking: "+item);
                    try
                    {
                        results.AddRange(Directory.GetFiles(item, "java.exe", SearchOption.TopDirectoryOnly));
                        var subfolders = Directory.GetDirectories(item);
                        foreach (var subfolder in subfolders)
                        {
                            queue.Enqueue(subfolder);
                        }
                    }
                    catch
                    {
                        // log?!
                    }
                }

                return results;
            });
        }
        
        private async Task<Tuple<string, string>> GetVersion(string javaExePath)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = javaExePath,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();

            string version = null;
            var fullVersion = new StringBuilder();
            while (!proc.StandardError.EndOfStream)
            {
                var line = await proc.StandardError.ReadLineAsync().ConfigureAwait(false);
                fullVersion.AppendLine(line);
                if (string.IsNullOrEmpty(version))
                {
                    version = line;
                }
            }

            proc.WaitForExit();
            return new Tuple<string, string>(version, fullVersion.ToString());
        }

        [Serializable]
        [DebuggerDisplay("{" + nameof(Version) + "}")]
        public class JavaInstallation
        {
            public string Location { get; set; }

            [XmlAttribute]
            public string Version { get; set; }
            
            public string FullVersion { get; set; }
        }
    }
}
#pragma warning restore CA1822
