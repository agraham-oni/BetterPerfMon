namespace BetterPerfMon.Services.OS;

public interface IOsQueryer
{
    ulong GetTotalPhysicalMemoryGb();
    ulong GetTotalVramGb();
    float GetCpuUtilizedTick();
    float GetMemoryUtilizedTick();
    float GetGpuUtilizedTick();
}
