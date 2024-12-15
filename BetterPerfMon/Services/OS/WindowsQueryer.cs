using System;
using System.Diagnostics;
using System.Management;
using ManagedCuda.Nvml;

namespace BetterPerfMon.Services.OS;

public class WindowsQueryer : IOsQueryer
{
    private const int _BYTES_IN_GB = 1024 * 1024 * 1024;
    private PerformanceCounter _cpuCounter = new("Processor", "% Processor Time", "_Total");
    private PerformanceCounter _ramCounter = new("Memory", "Available Bytes");
    private bool _hasNvidiaGpu = _HasNvidiaGpu();
    private nvmlDevice _gpuDeviceHandle;

    public WindowsQueryer()
    {
        if (_hasNvidiaGpu)
        {
            NvmlNativeMethods.nvmlInit();
            uint deviceCount = 0;
            NvmlNativeMethods.nvmlDeviceGetCount(ref deviceCount);
            if (deviceCount == 0)
            {
                throw new Exception("Nvidia Gpu found but device count was 0.");
            }
            
            // Need to handle if there are multiple GPUs.
            NvmlNativeMethods.nvmlDeviceGetHandleByIndex(0, ref _gpuDeviceHandle);
        }
    }
    
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
        if (_hasNvidiaGpu)
        {
            nvmlMemory memoryInfo = new();
            NvmlNativeMethods.nvmlDeviceGetMemoryInfo(_gpuDeviceHandle, ref memoryInfo);
            return memoryInfo.total / _BYTES_IN_GB;
        }
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
        if (_hasNvidiaGpu)
        {
            nvmlUtilization utilization = new();
            NvmlNativeMethods.nvmlDeviceGetUtilizationRates(_gpuDeviceHandle, ref utilization);
            return utilization.gpu;
        }
        return 0;
    }
    
    public float GetVramUtilizedTick()
    {
        if (_hasNvidiaGpu)
        {
            nvmlMemory memoryInfo = new();
            NvmlNativeMethods.nvmlDeviceGetMemoryInfo(_gpuDeviceHandle, ref memoryInfo);
            return memoryInfo.used / _BYTES_IN_GB;
        }
        return 0;
    }
    
    private static bool _HasNvidiaGpu()
    {
        ObjectQuery query = new ObjectQuery("SELECT Name FROM Win32_VideoController");
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
    
        foreach (ManagementObject obj in searcher.Get())
        {
            string gpuName = obj["Name"]?.ToString();
            if (gpuName != null && gpuName.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
