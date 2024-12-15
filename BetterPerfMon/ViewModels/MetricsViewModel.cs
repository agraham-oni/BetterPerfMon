using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using BetterPerfMon.Models;
using BetterPerfMon.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

namespace BetterPerfMon.ViewModels;

public class MetricsViewModel
{
    private IMetricsService _metricsService = new ThreadedMetricsService();
    
    private ObservableCollection<float> _cpuPercents = new();
    private ObservableCollection<float> _memPercents = new();
    private ObservableCollection<float> _gpuPercents = new();
    
    private ObservableCollection<float> _memGb = new();
    private ObservableCollection<float> _vramGb = new();

    public ObservableCollection<ISeries> CpuSeries { get; } = new();
    public ObservableCollection<ISeries> MemSeries { get; } = new();
    public ObservableCollection<ISeries> GpuSeries { get; } = new();
    
    public ObservableCollection<ISeries> MemGbSeries { get; } = new();
    public ObservableCollection<ISeries> VramGbSeries { get; } = new();
    
    public LabelVisual CpuTitle { get; } = _GetLabelVisual("CPU Usage (%)");
    public LabelVisual MemTitle { get; } = _GetLabelVisual("Memory Usage (%)");
    public LabelVisual GpuTitle { get; } = _GetLabelVisual("GPU Usage (%)");
    
    public LabelVisual MemGbTitle { get; } = _GetLabelVisual("Memory (GB)");
    public LabelVisual VramGbTitle { get; } = _GetLabelVisual("VRAM (GB)");

    public Axis[] XAxis { get; } = _GetAxis();
    public Axis[] YAxis { get; } = _GetAxis();

    public MetricsViewModel()
    {
        CpuSeries.Add(_GetLineSeries(_cpuPercents));
        MemSeries.Add(_GetLineSeries(_memPercents));
        GpuSeries.Add(_GetLineSeries(_gpuPercents));    
        
        MemGbSeries.Add(_GetLineSeries(_memGb));
        VramGbSeries.Add(_GetLineSeries(_vramGb));
    }

    public void OnMetricsStartClicked()
    {
        _metricsService.StartCollecting(_OnMetricsReceived);
    }
    
    public void OnMetricsStopClicked()
    {
        _metricsService.StopCollecting();
    }

    private void _OnMetricsReceived(MetricsSet metrics)
    {
        Console.WriteLine(metrics.ToString());
        _cpuPercents.Add(metrics.CpuPercent);
        _memPercents.Add(metrics.MemoryPercent);
        _gpuPercents.Add(metrics.GpuPercent);
        _memGb.Add(metrics.MemoryGb);
        _vramGb.Add(metrics.VramGb);
    }

    private static LabelVisual _GetLabelVisual(string title)
    {
        return new()
        {
            Text = title,
            TextSize = 10,
            Padding = new LiveChartsCore.Drawing.Padding(15),
            Paint = new SolidColorPaint(_GetTextColor())
        };
    }

    private static LineSeries<float, CircleGeometry> _GetLineSeries(ObservableCollection<float> values)
    {
        return new()
        {
            Values = values,
            Fill = null,
            GeometrySize = 5
        };
    }

    private static Axis[] _GetAxis()
    {
        return
        [
            new()
            {
                LabelsPaint = new SolidColorPaint(_GetTextColor())
            }
        ];
    }

    private static SKColor _GetTextColor()
    {
        if (_IsMacOS())
        {
            return SKColors.White;
        }
        return SKColors.Black;
    }

    private static bool _IsMacOS()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }
}
