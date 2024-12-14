using System;
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
        while (!_stopCollecting.IsSet)
        {
            var metrics = new MetricsSet
            {
                Timestamp = DateTime.Now,
                CpuPercent = 50.0f,
                MemoryPercent = 100.0f,
                GpuPercent = 100.0f,
            };
            messageCallback(metrics);
            Thread.Sleep(1000);
        }
    }
}
