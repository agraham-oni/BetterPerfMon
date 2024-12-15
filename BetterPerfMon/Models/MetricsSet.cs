using System;

namespace BetterPerfMon.Models;

public struct MetricsSet
{
    public DateTime Timestamp { get; set; }
    public float CpuPercent { get; set; }
    public float GpuPercent { get; set; }
    public float MemoryPercent { get; set; }
    public float MemoryGb { get; set; }
    public float VramPercent { get; set; }
    public float VramGb { get; set; }
    
    public override string ToString()
    {
        string result = "";
        result += $"Time: {Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")} \n";
        result += $"CPU: {CpuPercent}%\n";
        result += $"GPU: {GpuPercent}%\n";
        result += $"Memory: {MemoryPercent}%\n";
        result += $"MemoryGB: {MemoryGb}GB\n";
        result += $"VRAM: {VramPercent}%\n";
        result += $"VRAMGB: {VramGb}GB\n";
        return result;
    }
}
