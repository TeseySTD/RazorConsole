// Copyright (c) RazorConsole. All rights reserved.

using Spectre.Console;
using Spectre.Console.Rendering;
using System.Buffers;

namespace RazorConsole.Core.Renderables;

public sealed class OverlayRenderable(IRenderable background, IEnumerable<OverlayItem> overlays) : IRenderable
{
    public Measurement Measure(RenderOptions options, int maxWidth) => background.Measure(options, maxWidth);

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var bgSegments = background.Render(options, maxWidth);
        var canvas = Segment.SplitLines(bgSegments);

        var overlayMap = overlays
            .OrderBy(o => o.ZIndex)
            .SelectMany(o =>
            {
                var lines = Segment.SplitLines(o.Renderable.Render(options, Math.Max(0, maxWidth - o.Left)));
                return lines.Select((line, index) => new { TargetY = o.Top + index, Line = line, o.Left });
            })
            .GroupBy(x => x.TargetY);

        var cellPool = ArrayPool<Cell>.Shared;
        Cell[]? lineBuffer = null;

        try
        {
            lineBuffer = cellPool.Rent(maxWidth);

            foreach (var rowGroup in overlayMap)
            {
                int y = rowGroup.Key;
                if (y < 0)
                {
                    continue;
                }

                while (canvas.Count <= y)
                {
                    canvas.Add(new SegmentLine());
                }

                int actualWidth = ToBuffer(canvas[y], lineBuffer, maxWidth);

                foreach (var ov in rowGroup)
                {
                    actualWidth = MergeToBuffer(ov.Line, lineBuffer, ov.Left, maxWidth, actualWidth);
                }

                canvas[y] = FromBuffer(lineBuffer, actualWidth);
            }
        }
        finally
        {
            if (lineBuffer != null)
            {
                cellPool.Return(lineBuffer);
            }
        }

        for (int i = 0; i < canvas.Count; i++)
        {
            foreach (var segment in canvas[i])
            {
                yield return segment;
            }

            if (i < canvas.Count - 1)
            {
                yield return Segment.LineBreak;
            }
        }
    }

    private static int ToBuffer(IEnumerable<Segment> segments, Cell[] buffer, int maxWidth)
    {
        int cursor = 0;
        foreach (var segment in segments)
        {
            var text = segment.Text;
            var style = segment.Style;
            for (int i = 0; i < text.Length && cursor < maxWidth; i++)
            {
                buffer[cursor++] = new Cell(text[i], style);
            }
        }

        return cursor;
    }

    private static int MergeToBuffer(IEnumerable<Segment> overlaySegments, Cell[] buffer, int left, int maxWidth,
        int currentWidth)
    {
        int cursor = left;
        foreach (var segment in overlaySegments)
        {
            var text = segment.Text;
            var style = segment.Style;
            for (int i = 0; i < text.Length && cursor < maxWidth; i++)
            {
                buffer[cursor++] = new Cell(text[i], style);
            }
        }

        return Math.Max(currentWidth, cursor);
    }

    private static SegmentLine FromBuffer(Cell[] buffer, int length)
    {
        if (length <= 0)
        {
            return new SegmentLine();
        }

        var result = new SegmentLine();
        var currentStyle = buffer[0].Style;
        int start = 0;

        for (int i = 1; i < length; i++)
        {
            if (!buffer[i].Style.Equals(currentStyle))
            {
                result.Add(new Segment(new string(ExtractChars(buffer, start, i - start)), currentStyle));
                currentStyle = buffer[i].Style;
                start = i;
            }
        }

        result.Add(new Segment(new string(ExtractChars(buffer, start, length - start)), currentStyle));
        return result;
    }

    private static char[] ExtractChars(Cell[] buffer, int start, int count)
    {
        char[] chars = new char[count];
        for (int i = 0; i < count; i++)
        {
            chars[i] = buffer[start + i].Char;
        }

        return chars;
    }

    private record struct Cell(char Char, Style Style);
}

public record OverlayItem(IRenderable Renderable, int Top, int Left, int ZIndex);
