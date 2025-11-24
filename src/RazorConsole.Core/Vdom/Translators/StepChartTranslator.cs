namespace RazorConsole.Core.Vdom.Translators;
using RazorConsole.Core.Rendering.Vdom;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Text;

public sealed class StepChartTranslator : IVdomElementTranslator
{
    public int Priority => 95;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element || node.TagName != "stepchart")
        {
            return false;
        }

        // Parse dimensions
        var width = VdomSpectreTranslator.TryGetIntAttribute(node, "data-width", 60);
        var height = VdomSpectreTranslator.TryGetIntAttribute(node, "data-height", 20);
        var showAxes = VdomSpectreTranslator.TryGetBoolAttribute(node, "data-show-axes", out var axes) && axes;
        var title = VdomSpectreTranslator.GetAttribute(node, "data-title");

        // Parse series
        var seriesList = new List<SeriesData>();
        foreach (var child in node.Children)
        {
            if (child.TagName != "chartseries")
            {
                continue;
            }

            var color = VdomSpectreTranslator.GetAttribute(child, "data-color") ?? "white";
            var pointsStr = VdomSpectreTranslator.GetAttribute(child, "data-points") ?? "";
            var points = ParsePoints(pointsStr);
            seriesList.Add(new SeriesData(color, points));
        }

        if (seriesList.Count == 0)
        {
            return false;
        }

        var chartText = RenderStepChart(seriesList, width, height, showAxes, title);
        renderable = new Markup(chartText);
        return true;
    }

    private List<(double X, double Y)> ParsePoints(string data)
    {
        var points = new List<(double, double)>();
        if (string.IsNullOrEmpty(data))
        {
            return points;
        }

        foreach (var pair in data.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split(',');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out var x) &&
                double.TryParse(parts[1], out var y))
            {
                points.Add((x, y));
            }
        }
        return points.OrderBy<(double, double), object>(p => p.Item1).ToList();
    }

    private string RenderStepChart(
        List<SeriesData> series,
        int width, int height,
        bool showAxes, string? title)
    {
        // Calculate bounds
        var allPoints = series.SelectMany(s => s.Points).ToList();
        if (allPoints.Count == 0)
        {
            return "[grey]No data[/]";
        }

        var minX = allPoints.Min(p => p.X);
        var maxX = allPoints.Max(p => p.X);
        var minY = Math.Min(0, allPoints.Min(p => p.Y));
        var maxY = allPoints.Max(p => p.Y);

        // Adjust for axes
        var chartWidth = showAxes ? width - 3 : width;
        var chartHeight = showAxes ? height - 2 : height;

        // Create canvas [col, row] -> (char, color)
        var canvas = new (char Ch, string Color)[chartWidth, chartHeight];
        for (int x = 0; x < chartWidth; x++)
        {
            for (int y = 0; y < chartHeight; y++)
            {
                canvas[x, y] = (' ', "white");
            }
        }


        // Unicode box drawing chars
        // ReSharper disable InconsistentNaming
        const char HORIZONTAL = '─';  // ─
        const char VERTICAL = '│';    // │
        const char CORNER_UP = '┘';   // ┘ (going up)
        const char CORNER_DOWN = '┐'; // ┐ (going down)


        // Plot each series
        foreach (var s in series)
        {
            var points = s.Points;
            for (int i = 0; i < points.Count; i++)
            {
                var (px, py) = points[i];

                // Map to canvas coordinates
                int col = (int)Math.Round((px - minX) / (maxX - minX) * (chartWidth - 1));
                int row = (int)Math.Round((py - minY) / (maxY - minY) * (chartHeight - 1));

                col = Math.Clamp(col, 0, chartWidth - 1);
                row = Math.Clamp(row, 0, chartHeight - 1);

                // Draw horizontal line to next point (or edge)
                int nextCol = i < points.Count - 1
                    ? (int)Math.Round((points[i + 1].X - minX) / (maxX - minX) * (chartWidth - 1))
                    : chartWidth - 1;
                nextCol = Math.Clamp(nextCol, 0, chartWidth - 1);

                for (int c = col; c <= nextCol; c++)
                {
                    canvas[c, row] = (HORIZONTAL, s.Color);
                }

                // Draw vertical connector to next point
                if (i < points.Count - 1)
                {
                    var (_, nextY) = points[i + 1];
                    int nextRow = (int)Math.Round((nextY - minY) / (maxY - minY) * (chartHeight - 1));
                    nextRow = Math.Clamp(nextRow, 0, chartHeight - 1);

                    if (nextRow != row)
                    {
                        // Draw vertical line
                        int fromRow = Math.Min(row, nextRow);
                        int toRow = Math.Max(row, nextRow);
                        for (int r = fromRow; r <= toRow; r++)
                        {
                            canvas[nextCol, r] = (VERTICAL, s.Color);
                        }

                        // Add corners
                        canvas[nextCol, row] = (nextRow > row ? CORNER_DOWN : CORNER_UP, s.Color);
                    }
                }
            }
        }

        // Build output
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(title))
        {
            sb.AppendLine(title);
        }

        // Y-axis arrow at top
        if (showAxes)
        {
            sb.AppendLine("[grey]   ▲[/]");
        }

        for (int row = chartHeight - 1; row >= 0; row--)
        {
            // Y-axis label with tick mark
            if (showAxes)
            {
                var yVal = minY + (maxY - minY) * row / (chartHeight - 1);
                // Show tick marks extending left from axis
                sb.Append($"[grey]{yVal,2:0}─┤[/]");
            }

            // Chart row
            for (int col = 0; col < chartWidth; col++)
            {
                var (ch, color) = canvas[col, row];
                if (ch != ' ')
                {
                    sb.Append($"[{color}]{ch}[/]");
                }
                else
                {
                    sb.Append(' ');
                }
            }
            sb.AppendLine();
        }

        // X-axis
        if (showAxes)
        {
            // Axis line with arrow (aligned: 2 digits + tick "─┤" = 4 chars, then corner)
            sb.Append("[grey]   └");
            sb.Append(new string('─', chartWidth));
            sb.AppendLine("▶[/]");

            // X labels with tick marks above
            // First: tick marks line
            sb.Append("[grey]    ");
            for (int col = 0; col < chartWidth; col++)
            {
                // Place tick at each label position
                bool isTick = false;
                for (int i = 0; i <= 8; i++)
                {
                    var pos = (int)(chartWidth * i / 8.0);
                    if (col == pos) { isTick = true; break; }
                }
                sb.Append(isTick ? "┬" : " ");
            }
            sb.AppendLine("[/]");

            // X labels
            sb.Append("[grey]    ");
            int lastPos = 0;
            for (int i = 0; i <= 8; i++)
            {
                var xVal = minX + (maxX - minX) * i / 8;
                var pos = (int)(chartWidth * i / 8.0);
                var label = $"{xVal:0}";
                var padding = pos - lastPos;
                sb.Append(label.PadLeft(Math.Max(1, padding)));
                lastPos = pos + label.Length;
            }
            sb.AppendLine("[/]");
        }

        return sb.ToString();
    }

    private record SeriesData(string Color, List<(double X, double Y)> Points);
}
