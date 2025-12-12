// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Renderables;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Renderables;

internal sealed class ScrollableWithBarRenderable : IRenderable
{
    private readonly List<IRenderable> _items;
    private readonly IRenderable _compositeContent;

    private readonly int _totalItems;
    private readonly int _offset;
    private readonly int _pageSize;

    private readonly Color _trackColor;
    private readonly Color _thumbColor;
    private readonly string _trackChar;
    private readonly string _thumbChar;

    public ScrollableWithBarRenderable(
        IEnumerable<IRenderable> items,
        int totalItems,
        int offset,
        int pageSize,
        Color? trackColor = null,
        Color? thumbColor = null,
        string? trackChar = null,
        string? thumbChar = null)
    {
        _items = items.ToList();
        _compositeContent = new Rows(_items);

        _totalItems = totalItems;
        _offset = offset;
        _pageSize = pageSize;

        _trackColor = trackColor ?? Color.Grey;
        _thumbColor = thumbColor ?? Color.White;
        _trackChar = trackChar ?? "│";
        _thumbChar = thumbChar ?? "█";
    }

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        var contentMeasure = _compositeContent.Measure(options, maxWidth);
        return new Measurement(contentMeasure.Min + 1, contentMeasure.Max + 1);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var directTable = _items.OfType<Table>().Count() == 1 ? _items.OfType<Table>().FirstOrDefault() : null;

        if (directTable != null)
        {
            bool isFirst = true;

            foreach (var item in _items)
            {
                if (!isFirst)
                {
                    yield return Segment.LineBreak;
                }
                isFirst = false;

                if (ReferenceEquals(item, directTable))
                {
                    foreach (var segment in RenderTableWithScrollBar(directTable, options, maxWidth))
                    {
                        yield return segment;
                    }
                }
                else
                {
                    foreach (var segment in item.Render(options, maxWidth))
                    {
                        yield return segment;
                    }
                }
            }
        }
        else
        {
            foreach (var segment in RenderSideScrollBar(options, maxWidth))
            {
                yield return segment;
            }
        }
    }

    private IEnumerable<Segment> RenderTableWithScrollBar(Table originalTable, RenderOptions options, int maxWidth)
    {
        var renderableTable = (IRenderable)originalTable;

        var tempSegments = renderableTable.Render(options, maxWidth).ToList();
        var tempLines = Segment.SplitLines(tempSegments);
        int totalHeight = tempLines.Count;

        if (totalHeight == 0)
        {
            yield break;
        }

        var (dataStart, dataEnd) = FindTableContentRange(tempLines, originalTable);
        int scrollBarHeight = Math.Max(1, dataEnd - dataStart);

        var scrollBar = new ScrollBarRenderable(
            _totalItems,
            _offset,
            _pageSize,
            scrollBarHeight,
            _trackChar,
            _thumbChar,
            _trackColor,
            _thumbColor,
            minThumbHeight: 1
        );

        var scrollBarSegments = scrollBar.Render(options, 1).ToList();
        var scrollBarLines = Segment.SplitLines(scrollBarSegments);

        for (int i = 0; i < totalHeight; i++)
        {
            var lineSegments = tempLines[i].ToList();

            if (i < dataStart || i >= dataEnd)
            {
                foreach (var seg in lineSegments)
                {
                    yield return seg;
                }

                if (i < totalHeight - 1)
                {
                    yield return Segment.LineBreak;
                }

                continue;
            }

            int scrollBarIndex = i - dataStart;
            Segment? scrollBarSeg = null;
            if (scrollBarIndex >= 0 && scrollBarIndex < scrollBarLines.Count &&
                scrollBarLines[scrollBarIndex].Count > 0)
            {
                scrollBarSeg = scrollBarLines[scrollBarIndex][0];
            }

            // Paste logic
            if (lineSegments.Count >= 2)
            {
                var borderSeg = lineSegments.Last();
                var contentSeg = lineSegments[^2];

                for (int j = 0; j < lineSegments.Count - 2; j++)
                {
                    yield return lineSegments[j];
                }

                string text = contentSeg.Text;
                if (text.Length > 0)
                {
                    if (text.Length > 1)
                    {
                        yield return new Segment(text.Substring(0, text.Length - 1), contentSeg.Style);
                    }
                }

                if (scrollBarSeg != null)
                {
                    yield return scrollBarSeg;
                }
                else
                {
                    if (text.Length > 0)
                    {
                        yield return new Segment(text.Substring(text.Length - 1), contentSeg.Style);
                    }
                    else
                    {
                        yield return new Segment(" ");
                    }
                }

                yield return borderSeg;
            }
            else
            {
                foreach (var seg in lineSegments)
                {
                    yield return seg;
                }
            }

            if (i < totalHeight - 1)
            {
                yield return Segment.LineBreak;
            }
        }
    }
    private (int start, int end) FindTableContentRange(List<SegmentLine> lines, Table table)
    {
        if (lines.Count == 0)
        {
            return (0, 0);
        }

        string? searchMarker = null;
        bool isTitleSearch = false;
        int lineOffset = 1;

        if (table.Border != TableBorder.None)
        {
            if (table.ShowHeaders)
            {
                searchMarker = table.Border.GetPart(TableBorderPart.HeaderBottomLeft) +
                               table.Border.GetPart(TableBorderPart.HeaderBottom);
            }
            else
            {
                searchMarker = table.Border.GetPart(TableBorderPart.HeaderTopLeft);
                lineOffset = 2;
            }
        }
        else if (table.Title != null)
        {
            searchMarker = table.Title.Text;
            isTitleSearch = true;
            lineOffset = 2;
        }

        int top = 0;

        if (!string.IsNullOrEmpty(searchMarker))
        {
            for (int i = 0; i < lines.Count - 1; i++)
            {
                var lineText = GetLineText(lines[i]);

                if (string.IsNullOrEmpty(lineText))
                {
                    continue;
                }

                if (isTitleSearch)
                {
                    int endOfTitleIndex = FindLastLineOfPhrase(lines, i, searchMarker);
                    if (endOfTitleIndex != i)
                    {
                        top = endOfTitleIndex + lineOffset;
                        break;
                    }
                }
                else if (lineText.StartsWith(searchMarker))
                {
                    top = i + lineOffset;
                    break;
                }
            }
        }

        int bottom = (table.Border != TableBorder.None) ? lines.Count - 1 : lines.Count;

        return (top >= bottom) ? (0, lines.Count) : (top, bottom);
    }

    private static string GetLineText(SegmentLine line) => string.Concat(line.Select(s => s.Text));

    private int FindLastLineOfPhrase(List<SegmentLine> textLines, int startIndex, string phrase)
    {
        string cleanPhrase = Markup.Remove(phrase);
        string targetSignature = cleanPhrase.Replace(" ", "").Replace("\t", "");

        var accumulatedText = new System.Text.StringBuilder();

        for (int i = startIndex; i < textLines.Count; i++)
        {
            string lineText = GetLineText(textLines[i]);

            accumulatedText.Append(lineText.Replace(" ", "").Replace("\t", ""));

            if (accumulatedText.ToString().Contains(targetSignature))
            {
                return i;
            }
        }

        return startIndex;
    }

    private IEnumerable<Segment> RenderSideScrollBar(RenderOptions options, int maxWidth)
    {
        var contentSegments = _compositeContent.Render(options, maxWidth).ToList();
        var lines = Segment.SplitLines(contentSegments);
        int renderedHeight = lines.Count;

        if (renderedHeight == 0)
        {
            yield break;
        }

        int maxContentLineWidth = 0;
        foreach (var line in lines)
        {
            int w = line.Sum(s => s.CellCount());
            if (w > maxContentLineWidth)
            {
                maxContentLineWidth = w;
            }
        }

        var scrollBar = new ScrollBarRenderable(
            _totalItems,
            _offset,
            _pageSize,
            renderedHeight,
            _trackChar,
            _thumbChar,
            _trackColor,
            _thumbColor,
            1
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
