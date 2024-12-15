using System;
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
        IOsQueryer osQueryer;

#if WINDOWS
        osQueryer = new WindowsQueryer();
#elif OSX
        osQueryer = new MacQueryer();
#else
        throw new PlatformNotSupportedException("This platform is not supported.");
#endif
        
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
}
