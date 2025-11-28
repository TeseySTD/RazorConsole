// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Renderables;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Renderables;

internal sealed class ScrollableWithBarRenderable : IRenderable
{
    private readonly IRenderable _compositeContent;
    private readonly int _totalItems;
    private readonly int _offset;
    private readonly int _pageSize;

    private readonly Color _trackColor = Color.Grey;
    private readonly Color _thumbColor = Color.White;

    private readonly string _trackChar = "│";
    private readonly string _thumbChar = "█";

    public ScrollableWithBarRenderable(
        IEnumerable<IRenderable> items,
        int totalItems,
        int offset,
        int pageSize)
    {
        _compositeContent = new Rows(items);
        _totalItems = totalItems;
        _offset = offset;
        _pageSize = pageSize;
    }

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        var contentMeasure = _compositeContent.Measure(options, maxWidth - 1);
        return new Measurement(contentMeasure.Min + 1, contentMeasure.Max + 1);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var contentSegments = _compositeContent.Render(options, maxWidth - 1).ToList();
        var lines = Segment.SplitLines(contentSegments);
        int renderedHeight = lines.Count;

        if (renderedHeight == 0)
        {
            yield break;
        }

        int maxContentLineWidth = 0;
        foreach (var line in lines)
        {
            int lineWidth = line.Sum(s => s.CellCount());
            if (lineWidth > maxContentLineWidth)
            {
                maxContentLineWidth = lineWidth;
            }
        }

        var scrollBar = new ScrollBarRenderable(
            _totalItems,
            _offset,
            _pageSize,
            renderedHeight,
            trackColor: _trackColor,
            thumbColor: _thumbColor,
            trackChar: _trackChar,
            thumbChar: _thumbChar,
            minThumbHeight: 1
        );

        var scrollBarSegments = scrollBar.Render(options, 1).ToList();
        var scrollBarLines = Segment.SplitLines(scrollBarSegments);

        for (int i = 0; i < renderedHeight; i++)
        {
            foreach (var segment in lines[i])
            {
                yield return segment;
            }

            int currentLineWidth = lines[i].Sum(s => s.CellCount());
            int padding = maxContentLineWidth + 1 - currentLineWidth;

            if (padding > 0)
            {
                yield return new Segment(new string(' ', padding));
            }

            if (i < scrollBarLines.Count)
            {
                foreach (var sbSegment in scrollBarLines[i])
                {
                    yield return sbSegment;
                }
            }
            else
            {
                yield return new Segment(" ");
            }

            if (i < renderedHeight - 1)
            {
                yield return Segment.LineBreak;
            }
        }
    }
}
