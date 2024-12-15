using System;

namespace BetterPerfMon.Services.OS;

// Currently faked for testring
public class MacQueryer : IOsQueryer
{
    private Random _randomGenerator = new();
    
    public ulong GetTotalPhysicalMemoryGb()
    {
        return 16;
    }

    public ulong GetTotalVramGb()
    {
        return 0;
    }

    public float GetCpuUtilizedTick()
    {
        return (float)(_randomGenerator.NextDouble() * 100);
    }

    public float GetMemoryUtilizedTick()
    {
        return (float)(_randomGenerator.NextDouble() * 16);
    }

    public float GetGpuUtilizedTick()
    {
        return 0;
    }
    
    public float GetVramUtilizedTick()
    {
        return 0;
    }
}
