using System;
using System.Runtime.InteropServices;
using System.Threading;
using BetterPerfMon.Models;
using BetterPerfMon.Services.OS;

namespace BetterPerfMon.Services;

public class ThreadedMetricsService : IMetricsService
{
    private Thread? _collectionThread;
    private readonly ManualResetEventSlim _stopCollecting = new(false);

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
        IOsQueryer osQueryer = _GetQueryer();
        ulong totalMemory = osQueryer.GetTotalPhysicalMemoryGb();
        
        while (!_stopCollecting.IsSet)
        {
            var memGbAvailable = osQueryer.GetMemoryUtilizedTick();
            var metrics = new MetricsSet
            {
                Timestamp = DateTime.Now,
                CpuPercent = osQueryer.GetCpuUtilizedTick(),
                MemoryPercent = ((totalMemory - memGbAvailable) / totalMemory) * 100,
                MemoryGb = totalMemory - memGbAvailable,
                GpuPercent = osQueryer.GetGpuUtilizedTick(),
                VramGb = osQueryer.GetVramUtilizedTick()
            };
            messageCallback(metrics);
            Thread.Sleep(1000);
        }
    }

    private IOsQueryer _GetQueryer()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException();
        }
        return new WindowsQueryer();
    }
}
