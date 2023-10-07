using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JavaVersionSwitcher.Logging;

namespace JavaVersionSwitcher.Adapters;

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
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ShellType.Unknown;
        }
            
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
        
    private static Process GetParentProcess(Process process) 
    {
        return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
    }
        
#pragma warning disable CA1416
    private static string FindIndexedProcessName(int pid) 
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new NotImplementedException(
                "Accessing parent process is currently only available on windows.");
        }
    
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
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new NotImplementedException(
                "Accessing parent process is currently only available on windows.");
        }

        var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
        return Process.GetProcessById((int) parentId.NextValue());
    }
#pragma warning restore CA1416
}