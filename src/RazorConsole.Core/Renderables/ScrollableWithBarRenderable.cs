// Copyright (c) RazorConsole. All rights reserved.

using System.Text;
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
    private readonly char _trackChar;
    private readonly char _thumbChar;
    private readonly int _minThumbHeight;
    private readonly bool _isEmbeddedScrollbarMode;

    public ScrollableWithBarRenderable(
        IEnumerable<IRenderable> items,
        int totalItems,
        int offset,
        int pageSize,
        bool enableEmbeddedScrollbar,
        Color? trackColor = null,
        Color? thumbColor = null,
        char? trackChar = null,
        char? thumbChar = null,
        int minThumbHeight = 1)
    {
        _items = items.ToList();
        _compositeContent = new Rows(_items);
        _totalItems = totalItems;
        _offset = offset;
        _pageSize = pageSize;
        _trackColor = trackColor ?? Color.Grey;
        _thumbColor = thumbColor ?? Color.White;
        _trackChar = trackChar ?? '│';
        _thumbChar = thumbChar ?? '█';
        _minThumbHeight = minThumbHeight;

        var tables = _items.OfType<Table>();
        var panels = _items.OfType<Panel>();
        _isEmbeddedScrollbarMode = tables.Count() + panels.Count() == 1 && enableEmbeddedScrollbar;
    }

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        if (_isEmbeddedScrollbarMode)
        {
            var contentMeasure = _compositeContent.Measure(options, maxWidth - 1); // For scrollbar

            return new Measurement(contentMeasure.Min + 1, contentMeasure.Max + 1);
        }
        else
        {
            var contentMeasure = _compositeContent.Measure(options, maxWidth - 2); // For scrollbar and space before

            return new Measurement(contentMeasure.Min + 2, contentMeasure.Max + 2);
        }
    }


    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        if (_isEmbeddedScrollbarMode)
        {
            var tables = _items.OfType<Table>().ToList();
            var panels = _items.OfType<Panel>().ToList();
            var targetTable = tables.FirstOrDefault();
            var targetPanel = panels.FirstOrDefault();
            var isFirst = true;

            foreach (var item in _items)
            {
                if (!isFirst)
                {
                    yield return Segment.LineBreak;
                }
                isFirst = false;

                if (targetTable != null && ReferenceEquals(item, targetTable))
                {
                    foreach (var segment in RenderTableWithScrollBar(targetTable, options, maxWidth))
                    {
                        yield return segment;
                    }
                }
                else if (targetPanel != null && ReferenceEquals(item, targetPanel))
                {
                    foreach (var segment in RenderPanelWithScrollBar(targetPanel, options, maxWidth))
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

    private IEnumerable<Segment> RenderPanelWithScrollBar(Panel originalPanel, RenderOptions options, int maxWidth)
    {
        var tempLines = GetRenderedLines(originalPanel, options, maxWidth - 1);
        if (tempLines.Count == 0)
        {
            yield break;
        }

        var hasBorder = originalPanel.Border != BoxBorder.None;
        var hasTitle = originalPanel.Header is not null;
        var dataStart = hasTitle || hasBorder ? 1 : 0;
        var dataEnd = hasBorder ? tempLines.Count - 1 : tempLines.Count;

        // Check if there are any items
        if (dataStart >= dataEnd || _totalItems == 0)
        {
            foreach (var segment in RenderRawLines(tempLines))
            {
                yield return segment;
            }
            yield break;
        }

        var scrollBarHeight = Math.Max(1, dataEnd - dataStart);
        var scrollBarLines = CreateScrollBarLines(options, scrollBarHeight);

        for (var i = 0; i < tempLines.Count; i++)
        {
            var lineSegments = tempLines[i].ToList();

            // Insert bottom border part to ensure proper spacing for the scrollbar
            var borderIndex = lineSegments.Count - 1;
            if (borderIndex >= 0)
            {
                lineSegments.Insert(borderIndex, new Segment(originalPanel.Border.GetPart(BoxBorderPart.Bottom)));
            }

            if (i < dataStart || i >= dataEnd)
            {
                foreach (var seg in lineSegments)
                {
                    yield return seg;
                }
                if (i < tempLines.Count - 1)
                {
                    yield return Segment.LineBreak;
                }
                continue;
            }

            var scrollBarIndex = i - dataStart;
            var scrollBarSeg = GetScrollBarSegment(scrollBarLines, scrollBarIndex);

            foreach (var s in InjectScrollBar(lineSegments, scrollBarSeg, hasBorder))
            {
                yield return s;
            }

            if (i < tempLines.Count - 1)
            {
                yield return Segment.LineBreak;
            }
        }
    }

    private IEnumerable<Segment> RenderTableWithScrollBar(Table originalTable, RenderOptions options, int maxWidth)
    {
        var tempLines = GetRenderedLines(originalTable, options, maxWidth - 1);
        if (tempLines.Count == 0)
        {
            yield break;
        }

        var (dataStart, dataEnd) = FindTableContentRange(tempLines, originalTable);
        // Check if there are any items
        if (dataStart >= dataEnd || _totalItems == 0)
        {
            foreach (var segment in RenderRawLines(tempLines))
            {
                yield return segment;
            }
            yield break;
        }

        var scrollBarHeight = Math.Max(1, dataEnd - dataStart);
        var scrollBarLines = CreateScrollBarLines(options, scrollBarHeight);

        for (var i = 0; i < tempLines.Count; i++)
        {
            var lineSegments = tempLines[i].ToList();

            if (i < dataStart || i >= dataEnd)
            {
                foreach (var seg in lineSegments)
                {
                    yield return seg;
                }
                if (i < tempLines.Count - 1)
                {
                    yield return Segment.LineBreak;
                }
                continue;
            }

            var scrollBarIndex = i - dataStart;
            var scrollBarSeg = GetScrollBarSegment(scrollBarLines, scrollBarIndex);

            foreach (var s in InjectScrollBar(lineSegments, scrollBarSeg, hasBorder: true))
            {
                yield return s;
            }

            if (i < tempLines.Count - 1)
            {
                yield return Segment.LineBreak;
            }
        }
    }

    private IEnumerable<Segment> RenderSideScrollBar(RenderOptions options, int maxWidth)
    {
        var tempLines = GetRenderedLines(_compositeContent, options, maxWidth - 2); // 2 because reserving place for bar and space before
        if (tempLines.Count == 0)
        {
            yield break;
        }

        var maxContentLineWidth = tempLines.Max(line => line.Sum(s => s.CellCount()));
        var scrollBarLines = CreateScrollBarLines(options, tempLines.Count);

        for (var i = 0; i < tempLines.Count; i++)
        {
            foreach (var segment in tempLines[i])
            {
                yield return segment;
            }

            var padding = maxContentLineWidth + 1 - tempLines[i].Sum(s => s.CellCount());
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

            if (i < tempLines.Count - 1)
            {
                yield return Segment.LineBreak;
            }
        }
    }

    private static IEnumerable<Segment> InjectScrollBar(List<Segment> lineSegments, Segment? scrollBarSeg, bool hasBorder)
    {
        var targetIndex = hasBorder ? lineSegments.Count - 2 : lineSegments.Count - 1;
        targetIndex = Math.Clamp(targetIndex, 0, lineSegments.Count - 1);

        for (var j = 0; j < targetIndex; j++)
        {
            yield return lineSegments[j];
        }

        var targetSeg = lineSegments[targetIndex];

        if (targetSeg.Text.Length > 1)
        {
            yield return new Segment(targetSeg.Text[..^1], targetSeg.Style);
        }

        if (scrollBarSeg != null)
        {
            yield return scrollBarSeg;
        }
        else if (targetSeg.Text.Length > 0)
        {
            yield return new Segment(targetSeg.Text[^1..], targetSeg.Style);
        }
        else
        {
            yield return new Segment(" ");
        }

        if (hasBorder && lineSegments.Count > 0)
        {
            yield return lineSegments[^1];
        }
    }

    private static Segment? GetScrollBarSegment(List<SegmentLine> scrollBarLines, int index)
    {
        if (index >= 0 && index < scrollBarLines.Count && scrollBarLines[index].Count > 0)
        {
            return scrollBarLines[index][0];
        }
        return null;
    }

    private (int start, int end) FindTableContentRange(List<SegmentLine> lines, Table table)
    {
        if (lines.Count == 0)
        {
            return (0, 0);
        }

        string? searchMarker = null;
        var isTitleSearch = false;
        var lineOffset = 1;

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

        var top = 0;
        if (!string.IsNullOrEmpty(searchMarker))
        {
            for (var i = 0; i < lines.Count - 1; i++)
            {
                var lineText = GetLineText(lines[i]);
                if (string.IsNullOrEmpty(lineText))
                {
                    continue;
                }

                if (isTitleSearch)
                {
                    var endOfTitleIndex = FindLastLineOfPhrase(lines, i, searchMarker);
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


        var bottom = table.Border != TableBorder.None ? lines.Count - 1 : lines.Count;
        return top >= bottom ? (0, 0) : (top, bottom);
    }

    private static int FindLastLineOfPhrase(List<SegmentLine> textLines, int startIndex, string phrase)
    {
        var cleanPhrase = Markup.Remove(phrase);
        var targetSignature = cleanPhrase.Replace(" ", "").Replace("\t", "");
        var accumulatedText = new StringBuilder();

        for (var i = startIndex; i < textLines.Count; i++)
        {
            var lineText = GetLineText(textLines[i]);
            accumulatedText.Append(lineText.Replace(" ", "").Replace("\t", ""));

            if (accumulatedText.ToString().Contains(targetSignature))
            {
                return i;
            }
        }

        return startIndex;
    }

    private static string GetLineText(SegmentLine line) => string.Concat(line.Select(s => s.Text));

    private static List<SegmentLine> GetRenderedLines(IRenderable renderable, RenderOptions options, int maxWidth) =>
        Segment.SplitLines(renderable.Render(options, maxWidth).ToList());

    private static IEnumerable<Segment> RenderRawLines(List<SegmentLine> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            foreach (var segment in lines[i])
            {
                yield return segment;
            }
            if (i < lines.Count - 1)
            {
                yield return Segment.LineBreak;
            }
        }
    }

    private List<SegmentLine> CreateScrollBarLines(RenderOptions options, int height)
    {
        var scrollBar = new ScrollBarRenderable(
            _totalItems,
            _offset,
            _pageSize,
            height,
            _trackChar,
            _thumbChar,
            _trackColor,
            _thumbColor,
            minThumbHeight: _minThumbHeight
        );

        return Segment.SplitLines(scrollBar.Render(options, 1).ToList());
    }
}
