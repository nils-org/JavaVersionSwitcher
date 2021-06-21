using System;
using System.Diagnostics;
using JavaVersionSwitcher.Logging;

namespace JavaVersionSwitcher.Adapters
{
    // some of this was inspired by https://stackoverflow.com/a/2336322/180156
    public class ShellAdapter : IShellAdapter
    {
        private readonly ILogger _logger;

        public ShellAdapter(ILogger logger)
        {
            _logger = logger;
        }
        
        public ShellType GetShellType()
        {
            try
            {
                var proc = GetParentProcess(Process.GetCurrentProcess());
                // when calling "dotnet jvs" the parent is "dotnet" - not sure if that's the case for dotnet-jvs.exe 
                if ("dotnet".Equals(proc.ProcessName, StringComparison.OrdinalIgnoreCase))
                {
                    proc = GetParentProcess(proc);
                }

                _logger.LogVerbose("Parent process name is: " + proc.ProcessName);
                var name = proc.ProcessName.ToLowerInvariant();
                switch (name)
                {
                    case "pwsh":
                    case "powershell":
                        return ShellType.PowerShell;
                    case "cmd":
                        return ShellType.CommandPrompt;
                    default:
                        return ShellType.Unknown;
                }
            }
            catch(Exception e)
            {
                _logger.LogVerbose($"{e.GetType().Name} while finding parent process: {e.Message}");
                return ShellType.Unknown;
            }
        }
        
        private static Process GetParentProcess(Process process) {
            return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
        }
        
        private static string FindIndexedProcessName(int pid) {
            var processName = Process.GetProcessById(pid).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string processIndexedName = null;

            for (var index = 0; index < processesByName.Length; index++) {
                processIndexedName = index == 0 ? processName : processName + "#" + index;
                var processId = new PerformanceCounter("Process", "ID Process", processIndexedName);
                if ((int) processId.NextValue() == pid) {
                    return processIndexedName;
                }
            }

            return processIndexedName;
        }

        private static Process FindPidFromIndexedProcessName(string indexedProcessName) {
            var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
            return Process.GetProcessById((int) parentId.NextValue());
        }
    }
}