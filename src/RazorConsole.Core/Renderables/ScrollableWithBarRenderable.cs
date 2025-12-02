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
        var contentMeasure = _compositeContent.Measure(options, maxWidth - 1);
        return new Measurement(contentMeasure.Min + 1, contentMeasure.Max + 1);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var firstItem = _items.FirstOrDefault();
        bool isSingleTable = _items.Count == 1 && firstItem is Table;

        if (isSingleTable)
        {
            foreach (var segment in RenderTableWithScrollBar((Table)firstItem!, options, maxWidth))
            {
                yield return segment;
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
            // SegmentLine реалізує IEnumerable<Segment>, тому перетворюємо в List для доступу за індексом
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

            // --- ЛОГІКА ВСТАВКИ ---
            // Нам потрібно мінімум 2 сегменти: [Контент][Рамка]
            if (lineSegments.Count >= 2)
            {
                var borderSeg = lineSegments.Last();
                var contentSeg = lineSegments[lineSegments.Count - 2];

                // 1. Виводимо все ДО передостаннього
                for (int j = 0; j < lineSegments.Count - 2; j++)
                {
                    yield return lineSegments[j];
                }

                // 2. Обробляємо передостанній (зрізаємо 1 char)
                string text = contentSeg.Text;
                if (text.Length > 0)
                {
                    if (text.Length > 1)
                    {
                        yield return new Segment(text.Substring(0, text.Length - 1), contentSeg.Style);
                    }
                    // Якщо довжина 1, ми просто не виводимо цей сегмент (він замінюється скролбаром)
                }

                // 3. Вставляємо скролбар
                if (scrollBarSeg != null)
                {
                    yield return scrollBarSeg;
                }
                else
                {
                    // Fallback: повертаємо "з'їдений" символ, якщо скролбару тут нема
                    if (text.Length > 0)
                    {
                        yield return new Segment(text.Substring(text.Length - 1), contentSeg.Style);
                    }
                    else
                    {
                        yield return new Segment(" ");
                    }
                }

                // 4. Виводимо рамку
                yield return borderSeg;
            }
            else
            {
                // Edge case: просто виводимо рядок як є
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

        int top = 0;
        int bottom = lines.Count;

        string? markerChar = null;
        bool skipMarkerLine = false;

        if (table.ShowHeaders)
        {
            markerChar = table.Border.GetPart(TableBorderPart.HeaderBottomLeft);

            if (string.IsNullOrEmpty(markerChar))
            {
                markerChar = table.Border.GetPart(TableBorderPart.HeaderBottom);
            }

            skipMarkerLine = true;
        }
        else if (table.Border != TableBorder.None)
        {
            markerChar = table.Border.GetPart(TableBorderPart.HeaderTopLeft);

            if (string.IsNullOrEmpty(markerChar))
            {
                markerChar = table.Border.GetPart(TableBorderPart.HeaderTopLeft);
            }

            skipMarkerLine = true;
        }

        if (!string.IsNullOrEmpty(markerChar))
        {
            for (int i = 0; i < lines.Count - 1; i++)
            {
                var lineText = string.Concat(lines[i].Select(s => s.Text));

                if (string.IsNullOrEmpty(lineText))
                {
                    continue;
                }

                if (lineText.StartsWith(markerChar))
                {
                    top = skipMarkerLine ? i + 1 : i;


                    if (table.ShowHeaders && (table.Border == TableBorder.Ascii || table.Border == TableBorder.Ascii2 || table.Border == TableBorder.Markdown))
                    {
                        bool isLikelyTopBorder = (i == 0) || (i == 1 && !string.IsNullOrWhiteSpace(string.Concat(lines[0].Select(s => s.Text))));

                        if (isLikelyTopBorder)
                        {
                            continue;
                        }
                    }

                    break;
                }
            }
        }


        if (table.Border != TableBorder.None)
        {
            bottom = lines.Count - 1;
        }

        if (top >= bottom)
        {
            return (0, lines.Count);
        }

        return (top, bottom);
    }


    private IEnumerable<Segment> RenderSideScrollBar(RenderOptions options, int maxWidth)
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
            int w = line.Sum(s => s.CellCount());
            if (w > maxContentLineWidth)
            {
                maxContentLineWidth = w;
            }
        }

        // Fix: Правильний порядок аргументів конструктора
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
