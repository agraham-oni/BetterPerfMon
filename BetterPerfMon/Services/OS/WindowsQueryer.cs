using System.Diagnostics;
using System.Management;

namespace BetterPerfMon.Services.OS;

public class WindowsQueryer : IOsQueryer
{
    private const int _BYTES_IN_GB = 1024 * 1024 * 1024;
    private PerformanceCounter _cpuCounter = new("Processor", "% Processor Time", "_Total");
    private PerformanceCounter _ramCounter = new("Memory", "Available Bytes");
    
    public ulong GetTotalPhysicalMemoryGb()
    {
        ObjectQuery query = new ObjectQuery("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

        foreach (ManagementObject obj in searcher.Get())
        {
            ulong totalPhysicalMemory = (ulong)obj["TotalPhysicalMemory"];
            return totalPhysicalMemory / _BYTES_IN_GB;
        }

        return 0;
    }

    public ulong GetTotalVramGb()
    {
        return 0;
    }

    public float GetCpuUtilizedTick()
    {
        return _cpuCounter.NextValue();
    }

    public float GetMemoryUtilizedTick()
    {
        return _ramCounter.NextValue() / _BYTES_IN_GB;
    }

    public float GetGpuUtilizedTick()
    {
        return 0;
    }
    
    // private bool _HasNvidiaGpu()
    // {
    //     bool nvidiaFound = false;
    //     
    //     ObjectQuery query = new ObjectQuery("SELECT Name FROM Win32_VideoController");
    //     ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
    //
    //     foreach (ManagementObject obj in searcher.Get())
    //     {
    //         string gpuName = obj["Name"]?.ToString();
    //         if (gpuName != null && gpuName.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
    //         {
    //             return true;
    //         }
    //     }
    //
    //     return false;
    // }
}
