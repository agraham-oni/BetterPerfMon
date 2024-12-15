using System.IO;

namespace BetterPerfMon.Models;

public class MetricStats
{
    public float MedianCpuPercent { get; set; }
    public float CpuPeakPercent { get; set; }
    public float MedianGpuPercent { get; set; }
    public float GpuPeakPercent { get; set; }
    public float MedianRamGb { get; set; }
    public float RamPeakGb { get; set; }
    public float MedianVramGb { get; set; }
    public float PeakVramGb { get; set; }
    public float DurationMinutes { get; set; }

    public void Export()
    {
        string filePath = "output.csv";
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("CPU%,CPUPeak%,GPU%,GPUPeak%,RAMGb,RAMPeakGb,VramGb,VramPeakGb,DurationMinutes");
            writer.WriteLine($"{MedianCpuPercent},{CpuPeakPercent},{MedianGpuPercent},{GpuPeakPercent},{MedianRamGb},{RamPeakGb},{MedianVramGb},{PeakVramGb},{DurationMinutes}");
        }
    }
}
