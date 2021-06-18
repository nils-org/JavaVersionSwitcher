using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JavaVersionSwitcher.Logging;
using JavaVersionSwitcher.Models;
using Spectre.Console;

namespace JavaVersionSwitcher.Adapters
{
    /// <inheritdoc cref="IJavaInstallationsAdapter"/>
    public class JavaInstallationsAdapter : IJavaInstallationsAdapter
    {
        private readonly ILogger _logger;

        public JavaInstallationsAdapter(ILogger logger)
        {
            _logger = logger;
        }
        
        /// <inheritdoc cref="IJavaInstallationsAdapter.GetJavaInstallations"/>
        public async Task<IEnumerable<JavaInstallation>> GetJavaInstallations(bool forceReScan = false)
        {
            if (!forceReScan && HasRecentCacheData())
            {
                try
                {
                    return await LoadCacheData();
                }
                catch (Exception ex)
                {
                    _logger.LogVerbose($"{ex.GetType().Name} while reading cached data.");
                }
            }

            var data = (await ForceScan()).ToList();

            try
            {
                await SaveCacheData(data);
            } 
            catch (Exception ex)
            {
                _logger.LogVerbose($"{ex.GetType().Name} while writing data cache.");
            }

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
            var result = new List<JavaInstallation>();
            await AnsiConsole.Status()
                .StartAsync("Initializing...", async ctx => 
                {
                    ctx.Status("Scanning for java installations");
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    // todo configurable start paths?
                    var start = new[]
                        {
                            Environment.ExpandEnvironmentVariables("%ProgramW6432%"),
                            Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%")
                        }.Distinct()
                        .Where(x => !string.IsNullOrEmpty(x));
                    _logger.LogVerbose(
                        $@"Scanning for installations in:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", start)}");
                    
                    var javaExeFiles = await FindFileRecursive(start);
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
                }).ConfigureAwait(false);
            
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
                    try
                    {
                        results.AddRange(Directory.GetFiles(item, "java.exe", SearchOption.TopDirectoryOnly));
                        var subfolders = Directory.GetDirectories(item);
                        foreach (var subfolder in subfolders)
                        {
                            queue.Enqueue(subfolder);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogVerbose($"{ex.GetType().Name} while accessing {item}");
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
    }
}
