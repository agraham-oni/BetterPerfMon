using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    private DateTime _startTime;
    private DateTime _endTime;

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
        _startTime = DateTime.Now;
    }
    
    public void OnMetricsStopClicked()
    {
        _metricsService.StopCollecting();
        _endTime = DateTime.Now;
    }

    public void ExportStats()
    {
        if (_cpuPercents.Count == 0)
        {
            throw new InvalidOperationException("No metrics available to export.");
        }
        var metricStats = new MetricStats()
        {
            MedianCpuPercent = _GetMedium(_cpuPercents.ToList()),
            CpuPeakPercent = _GetPercentile(_cpuPercents.ToList(), 99),
            MedianGpuPercent = _GetMedium(_gpuPercents.ToList()),
            GpuPeakPercent = _GetPercentile(_gpuPercents.ToList(), 99),
            MedianRamGb = _GetMedium(_memGb.ToList()),
            RamPeakGb = _GetPercentile(_memGb.ToList(), 99),
            MedianVramGb = _GetMedium(_vramGb.ToList()),
            PeakVramGb = _GetPercentile(_vramGb.ToList(), 99),
            DurationMinutes = (float)(_endTime - _startTime).TotalMinutes,
            
        };
        metricStats.Export();
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
            Paint = new SolidColorPaint(SKColors.White)
        };
    }

    private static LineSeries<float, CircleGeometry> _GetLineSeries(ObservableCollection<float> values)
    {
        return new()
        {
            Values = values,
            Fill = null,
            GeometrySize = 2
        };
    }

    private static Axis[] _GetAxis()
    {
        return
        [
            new()
            {
                LabelsPaint = new SolidColorPaint(SKColors.White)
            }
        ];
    }
    
    private float _GetPercentile(List<float> values, int percentile)
    {
        values.Sort();
        int n = values.Count;
        float index = percentile / 100f * (n - 1);
        int lower = (int)Math.Floor(index);
        int upper = lower + 1 < n ? lower + 1 : lower;
        float weight = index - lower;
        return values[lower] * (1 - weight) + values[upper] * weight;
    }
    
    private float _GetMedium(List<float> numbers)
    {
        var sortedNumbers = numbers.OrderBy(n => n).ToList();
        int count = sortedNumbers.Count;
        if (count % 2 == 1)
        {
            return sortedNumbers[count / 2];
        }
        
        float mid1 = sortedNumbers[(count / 2) - 1];
        float mid2 = sortedNumbers[count / 2];
        return (mid1 + mid2) / 2;
    }
}
