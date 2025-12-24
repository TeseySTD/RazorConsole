// Copyright (c) RazorConsole. All rights reserved.

using Spectre.Console;
using Spectre.Console.Rendering;
using System.Buffers;

namespace RazorConsole.Core.Renderables;

public sealed class OverlayRenderable(IRenderable background, IEnumerable<OverlayItem> overlays) : IRenderable
{
    private readonly List<OverlayItem> _sortedOverlays = overlays.OrderBy(o => o.ZIndex).ToList();
    private readonly Dictionary<int, List<OverlayPosition>> _overlayMapCache = new();

    public Measurement Measure(RenderOptions options, int maxWidth) => background.Measure(options, maxWidth);

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var bgSegments = background.Render(options, maxWidth);
        var canvas = Segment.SplitLines(bgSegments);

        _overlayMapCache.Clear();

        foreach (var overlay in _sortedOverlays)
        {
            int finalLeft;
            int widthToRender;

            if (overlay.Left.HasValue && overlay.Right.HasValue)
            {
                // CSS-like horizontal stretching
                finalLeft = overlay.Left.Value;
                widthToRender = maxWidth - overlay.Left.Value - overlay.Right.Value;
            }
            else if (overlay.Right.HasValue)
            {
                int constraint = Math.Max(0, maxWidth - overlay.Right.Value);
                var measurement = overlay.Renderable.Measure(options, constraint);
                widthToRender = measurement.Max;
                finalLeft = maxWidth - overlay.Right.Value - widthToRender;
            }
            else
            {
                finalLeft = overlay.Left ?? 0;
                widthToRender = maxWidth - finalLeft;
            }

            var lines = Segment.SplitLines(
                overlay.Renderable.Render(options, Math.Max(0, widthToRender))
            );

            int finalTop = overlay.Top ?? (overlay.Bottom.HasValue
                ? Math.Max(0, canvas.Count - overlay.Bottom.Value - lines.Count)
                : 0);

            for (int i = 0; i < lines.Count; i++)
            {
                int targetY = finalTop + i;
                if (targetY < 0)
                {
                    continue;
                }

                if (!_overlayMapCache.TryGetValue(targetY, out var positions))
                {
                    positions = new List<OverlayPosition>(4);
                    _overlayMapCache[targetY] = positions;
                }

                positions.Add(new OverlayPosition(lines[i], finalLeft));
            }
        }

        var cellPool = ArrayPool<Cell>.Shared;
        Cell[]? lineBuffer = null;

        try
        {
            lineBuffer = cellPool.Rent(maxWidth);

            foreach (var kvp in _overlayMapCache)
            {
                int y = kvp.Key;
                var overlayPositions = kvp.Value;

                while (canvas.Count <= y)
                {
                    canvas.Add(new SegmentLine());
                }

                // Clean up array
                Array.Fill(lineBuffer, new Cell(' ', Style.Plain), 0, maxWidth);

                int backgroundWidth = ToBuffer(canvas[y], lineBuffer, maxWidth);
                int actualWidth = backgroundWidth;

                foreach (var overlayPos in overlayPositions)
                {
                    int endX = MergeToBuffer(overlayPos.Line, lineBuffer, overlayPos.Left, maxWidth);
                    if (endX > actualWidth)
                    {
                        actualWidth = endX;
                    }
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

    private static int MergeToBuffer(IEnumerable<Segment> overlaySegments, Cell[] buffer, int left, int maxWidth)
    {
        int cursor = left;
        foreach (var segment in overlaySegments)
        {
            var text = segment.Text;
            var style = segment.Style;
            for (int i = 0; i < text.Length && cursor < maxWidth; i++)
            {
                if (cursor >= 0)
                {
                    buffer[cursor] = new Cell(text[i], style);
                }

                cursor++;
            }
        }

        return cursor;
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
                result.Add(new Segment(CreateStringFromBuffer(buffer, start, i - start), currentStyle));
                currentStyle = buffer[i].Style;
                start = i;
            }
        }

        result.Add(new Segment(CreateStringFromBuffer(buffer, start, length - start), currentStyle));
        return result;
    }

    private static string CreateStringFromBuffer(Cell[] buffer, int start, int count)
    {
        return string.Create(count, (buffer, start), static (span, state) =>
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = state.buffer[state.start + i].Char;
            }
        });
    }

    private record struct Cell(char Char, Style Style);

    private readonly struct OverlayPosition(SegmentLine line, int left)
    {
        public readonly SegmentLine Line = line;
        public readonly int Left = left;
    }
}

public record OverlayItem(
    IRenderable Renderable,
    int? Top,
    int? Left,
    int? Right,
    int? Bottom,
    int ZIndex
);
