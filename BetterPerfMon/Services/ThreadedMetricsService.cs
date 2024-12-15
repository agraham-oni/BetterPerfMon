using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using BetterPerfMon.Models;

namespace BetterPerfMon.Services;

public class ThreadedMetricsService : IMetricsService
{
    private Thread? _collectionThread;
    private readonly ManualResetEventSlim _stopCollecting = new ManualResetEventSlim(false);

    public void StartCollecting(Action<MetricsSet> messageCallback)
    {
        _collectionThread = new Thread(() => _CollectMetrics(messageCallback));   
        _collectionThread.Start();
    }

    public void StopCollecting()
    {
        _stopCollecting.Set();
        _collectionThread?.Join();
        _stopCollecting.Reset();
    }

    private void _CollectMetrics(Action<MetricsSet> messageCallback)
    {
        ulong totalMemory = _GetTotalPhysicalMemoryMb();
        
        // Get total time CPU spends executing threads of a process.
        PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available Bytes");
        
        while (!_stopCollecting.IsSet)
        {
            var memGbAvailable = ramCounter.NextValue() / (1024 * 1024 * 1024);
            var metrics = new MetricsSet
            {
                Timestamp = DateTime.Now,
                CpuPercent = cpuCounter.NextValue(),
                MemoryPercent = ((totalMemory - memGbAvailable) / totalMemory) * 100,
                MemoryGb = totalMemory - memGbAvailable
            };
            messageCallback(metrics);
            Thread.Sleep(1000);
        }
    }

    private ulong _GetTotalPhysicalMemoryMb()
    {
        ObjectQuery query = new ObjectQuery("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

        foreach (ManagementObject obj in searcher.Get())
        {
            ulong totalPhysicalMemory = (ulong)obj["TotalPhysicalMemory"];
            return totalPhysicalMemory / (1024 * 1024 * 1024);
        }

        return 0;
    }
}
