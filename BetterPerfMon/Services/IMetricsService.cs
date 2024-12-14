using System;
using BetterPerfMon.Models;

namespace BetterPerfMon.Services;

public interface IMetricsService
{
    void StartCollecting(Action<MetricsSet> messageCallback);
    void StopCollecting();
}
