using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using DifficultyOfSortingAlgorithms.Core.Algorithms;
using DifficultyOfSortingAlgorithms.Core.Benchmarking;
using DifficultyOfSortingAlgorithms.ViewModels;

namespace DifficultyOfSortingAlgorithms.Views;

/// <summary>
/// Line chart of time (ms) against array size, with an optional logarithmic time axis. Hovering a point highlights
/// its line and shows the measurement's details.
/// </summary>
public sealed class PerformanceChart : Control
{
    public static readonly StyledProperty<IReadOnlyList<ChartSeries>?> SeriesProperty =
        AvaloniaProperty.Register<PerformanceChart, IReadOnlyList<ChartSeries>?>(nameof(Series));

    public static readonly StyledProperty<bool> LogScaleProperty =
        AvaloniaProperty.Register<PerformanceChart, bool>(nameof(LogScale));

    public static readonly StyledProperty<int> RevisionProperty =
        AvaloniaProperty.Register<PerformanceChart, int>(nameof(Revision));

    private static readonly IBrush Background = new SolidColorBrush(Color.Parse("#0D1016"));
    private static readonly IPen GridPen = new Pen(new SolidColorBrush(Color.Parse("#222835")), 1);
    private static readonly IPen AxisPen = new Pen(new SolidColorBrush(Color.Parse("#4A5468")), 1);
    private static readonly IBrush LabelBrush = new SolidColorBrush(Color.Parse("#8B95A5"));
    private static readonly Typeface LabelFace = new("Inter");
    private const double LabelSize = 11;
    private static readonly IBrush TooltipBackground = new SolidColorBrush(Color.Parse("#EE1A1F29"));
    private static readonly IPen TooltipBorder = new Pen(new SolidColorBrush(Color.Parse("#4A5468")), 1);
    private static readonly IBrush TooltipText = new SolidColorBrush(Color.Parse("#E8ECF2"));
    private const double HoverRadius = 14;

    // Screen positions of every point from the last render, for hit testing.
    private readonly List<(Point Position, ChartSeries Series, Measurement Measurement)> _hitPoints = [];
    private Point? _pointer;
    private (ChartSeries Series, Measurement Measurement, Point Position)? _hovered;

    private const double MarginLeft = 64, MarginBottom = 36, MarginTop = 16, MarginRight = 20;

    static PerformanceChart() => AffectsRender<PerformanceChart>(SeriesProperty, LogScaleProperty, RevisionProperty);

    public IReadOnlyList<ChartSeries>? Series { get => GetValue(SeriesProperty); set => SetValue(SeriesProperty, value); }
    public bool LogScale { get => GetValue(LogScaleProperty); set => SetValue(LogScaleProperty, value); }
    public int Revision { get => GetValue(RevisionProperty); set => SetValue(RevisionProperty, value); }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _pointer = e.GetPosition(this);
        UpdateHover();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _pointer = null;
        UpdateHover();
    }

    private void UpdateHover()
    {
        (ChartSeries, Measurement, Point)? best = null;
        if (_pointer is { } pointer)
        {
            var bestDistance = HoverRadius;
            foreach (var (position, series, measurement) in _hitPoints)
            {
                var distance = Point.Distance(position, pointer);
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = (series, measurement, position);
                }
            }
        }

        if (best?.Item2 != _hovered?.Measurement)
        {
            _hovered = best;
            Cursor = best is null ? Cursor.Default : new Cursor(StandardCursorType.Hand);
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        _hitPoints.Clear();
        var bounds = new Rect(Bounds.Size);
        context.DrawRectangle(Background, null, bounds, 8, 8);

        var plot = new Rect(MarginLeft, MarginTop,
            Math.Max(1, bounds.Width - MarginLeft - MarginRight), Math.Max(1, bounds.Height - MarginTop - MarginBottom));

        var points = (Series ?? []).SelectMany(s => s.Points).ToList();
        var hovered = _hovered;
        if (hovered is { } h && !(Series ?? []).Any(s => s.Measurements.Contains(h.Measurement)))
            hovered = _hovered = null;
        if (points.Count == 0)
        {
            DrawText(context, "Results appear here while the benchmark runs.", plot.Center, center: true);
            return;
        }

        var maxX = Math.Max(1, points.Max(p => p.X));
        var positive = points.Where(p => p.Y > 0).Select(p => p.Y).DefaultIfEmpty(0.001);
        var minY = LogScale ? Math.Pow(10, Math.Floor(Math.Log10(positive.Min()))) : 0;
        var maxY = LogScale ? Math.Pow(10, Math.Ceiling(Math.Log10(Math.Max(positive.Max(), minY * 10)))) : NiceCeiling(points.Max(p => p.Y));

        double MapX(double x) => plot.Left + x / maxX * plot.Width;
        double MapY(double y)
        {
            var t = LogScale
                ? (Math.Log10(Math.Max(y, minY)) - Math.Log10(minY)) / (Math.Log10(maxY) - Math.Log10(minY))
                : y / maxY;
            return plot.Bottom - t * plot.Height;
        }

        // Y grid: decades on a log axis, five even steps otherwise.
        var yTicks = new List<double>();
        if (LogScale)
            for (var y = minY; y <= maxY * 1.0001; y *= 10) yTicks.Add(y);
        else
            for (var i = 0; i <= 5; i++) yTicks.Add(maxY * i / 5);

        foreach (var y in yTicks)
        {
            var py = MapY(y);
            context.DrawLine(GridPen, new Point(plot.Left, py), new Point(plot.Right, py));
            DrawText(context, FormatMs(y), new Point(plot.Left - 8, py), alignRight: true);
        }

        for (var i = 0; i <= 5; i++)
        {
            var x = maxX * i / 5;
            var px = MapX(x);
            context.DrawLine(GridPen, new Point(px, plot.Top), new Point(px, plot.Bottom));
            DrawText(context, FormatCount(x), new Point(px, plot.Bottom + 12), center: true);
        }

        context.DrawLine(AxisPen, plot.BottomLeft, plot.BottomRight);
        context.DrawLine(AxisPen, plot.TopLeft, plot.BottomLeft);
        DrawText(context, "elements", new Point(plot.Right, plot.Bottom + 26), alignRight: true);

        using (context.PushClip(plot.Inflate(4)))
        {
            // Draw the hovered line last so it sits on top; dim the others while hovering.
            var ordered = (Series ?? []).OrderBy(s => s == hovered?.Series).ToList();
            foreach (var series in ordered)
            {
                if (series.Measurements.Count == 0)
                    continue;

                var isHovered = series == hovered?.Series;
                var opacity = hovered is null || isHovered ? 1.0 : 0.25;
                var brush = new SolidColorBrush(series.Color, opacity);
                var pen = new Pen(brush, isHovered ? 3.5 : 2, dashStyle: series.IsDashed ? new DashStyle(AlgorithmStyle.Dashes, 0) : null,
                    lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

                var screen = series.Measurements.Select(m => new Point(MapX(m.Size), MapY(m.Milliseconds))).ToList();
                var geometry = new StreamGeometry();
                using (var g = geometry.Open())
                {
                    g.BeginFigure(screen[0], false);
                    foreach (var p in screen.Skip(1))
                        g.LineTo(p);
                    g.EndFigure(false);
                }

                context.DrawGeometry(null, pen, geometry);
                for (var i = 0; i < screen.Count; i++)
                {
                    // Bojalib points are hollow rings, originals filled dots.
                    if (series.IsDashed)
                        context.DrawEllipse(Background, new Pen(brush, 1.5), screen[i], 3, 3);
                    else
                        context.DrawEllipse(brush, null, screen[i], 3, 3);
                    _hitPoints.Add((screen[i], series, series.Measurements[i]));
                }
            }

            if (hovered is { } hov)
            {
                var at = new Point(MapX(hov.Measurement.Size), MapY(hov.Measurement.Milliseconds));
                context.DrawEllipse(null, new Pen(Brushes.White, 2), at, 6, 6);
                _hovered = (hov.Series, hov.Measurement, at);
            }
        }

        if (_hovered is { } tip)
            DrawTooltip(context, tip.Series, tip.Measurement, tip.Position, bounds);
    }

    private static void DrawTooltip(DrawingContext context, ChartSeries series, Measurement m, Point anchor, Rect bounds)
    {
        var lines = new List<(string Text, bool Bold)>
        {
            (series.Name, true),
            (series.Algorithm.Origin == AlgorithmOrigin.Bojalib ? "Bojalib · " + series.Algorithm.Complexity : "Original · " + series.Algorithm.Complexity, false),
            ($"{m.Size:N0} elements", false),
            ($"Time: {FormatMs(m.Milliseconds)}", false),
            ($"Per element: {m.Milliseconds * 1_000_000 / Math.Max(1, m.Size):0.#} ns", false),
        };
        if (m.Stats != SortStats.None)
        {
            lines.Add(($"Comparisons: {m.Stats.Comparisons:N0}", false));
            lines.Add(($"Writes: {m.Stats.Writes:N0}", false));
        }

        var texts = lines.Select(l => new FormattedText(l.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            l.Bold ? new Typeface("Inter", FontStyle.Normal, FontWeight.SemiBold) : LabelFace, l.Bold ? 13 : 12, l.Bold ? TooltipText : LabelBrush)).ToList();

        const double pad = 10, gap = 3, swatch = 14;
        var width = texts.Max(t => t.Width) + pad * 2;
        width = Math.Max(width, texts[0].Width + swatch + 8 + pad * 2);
        var height = texts.Sum(t => t.Height) + gap * (texts.Count - 1) + pad * 2;

        // Prefer right of and above the point; flip when that would leave the control.
        var x = anchor.X + 14;
        if (x + width > bounds.Right - 4) x = anchor.X - 14 - width;
        var y = anchor.Y - height - 10;
        if (y < bounds.Top + 4) y = anchor.Y + 14;
        x = Math.Clamp(x, bounds.Left + 4, Math.Max(bounds.Left + 4, bounds.Right - width - 4));
        y = Math.Clamp(y, bounds.Top + 4, Math.Max(bounds.Top + 4, bounds.Bottom - height - 4));

        var box = new Rect(x, y, width, height);
        context.DrawRectangle(TooltipBackground, TooltipBorder, box, 6, 6);

        var cursor = y + pad;
        for (var i = 0; i < texts.Count; i++)
        {
            var left = x + pad;
            if (i == 0)
            {
                var mid = cursor + texts[0].Height / 2;
                var pen = new Pen(series.Brush, 3, dashStyle: series.IsDashed ? new DashStyle(AlgorithmStyle.Dashes, 0) : null);
                context.DrawLine(pen, new Point(left, mid), new Point(left + swatch, mid));
                left += swatch + 8;
            }

            context.DrawText(texts[i], new Point(left, cursor));
            cursor += texts[i].Height + gap;
        }
    }

    private static double NiceCeiling(double value)
    {
        if (value <= 0) return 1;
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
        foreach (var step in new[] { 1, 2, 2.5, 5, 10 })
            if (step * magnitude >= value) return step * magnitude;
        return 10 * magnitude;
    }

    private static string FormatMs(double ms) => ms switch
    {
        >= 1000 => (ms / 1000).ToString("0.##", CultureInfo.InvariantCulture) + " s",
        >= 1 or 0 => ms.ToString("0.##", CultureInfo.InvariantCulture) + " ms",
        _ => (ms * 1000).ToString("0.##", CultureInfo.InvariantCulture) + " µs",
    };

    private static string FormatCount(double n) => n switch
    {
        >= 1_000_000 => (n / 1_000_000).ToString("0.#", CultureInfo.InvariantCulture) + "M",
        >= 1_000 => (n / 1_000).ToString("0.#", CultureInfo.InvariantCulture) + "k",
        _ => n.ToString("0", CultureInfo.InvariantCulture),
    };

    private static void DrawText(DrawingContext context, string text, Point anchor, bool alignRight = false, bool center = false)
    {
        var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, LabelFace, LabelSize, LabelBrush);
        var x = alignRight ? anchor.X - formatted.Width : center ? anchor.X - formatted.Width / 2 : anchor.X;
        context.DrawText(formatted, new Point(x, anchor.Y - formatted.Height / 2));
    }
}
