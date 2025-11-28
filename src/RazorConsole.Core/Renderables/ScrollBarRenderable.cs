// Copyright (c) RazorConsole. All rights reserved.

using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Renderables;

internal sealed class ScrollBarRenderable(
    int itemsCount,
    int offset,
    int pageSize,
    int viewportHeight,
    string trackChar,
    string thumbChar,
    Color trackColor,
    Color thumbColor,
    int minThumbHeight)
    : IRenderable
{
    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        // ScrollBar is always 1 character wide
        return new Measurement(1, 1);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var height = viewportHeight;

        var trackStyle = new Style(foreground: trackColor);
        var thumbStyle = new Style(foreground: thumbColor);
        // If items fit completely in viewport, show just track
        // if (itemsCount <= height)
        // {
        //     for (int i = 0; i < height; i++)
        //     {
        //         yield return new Segment(trackChar, trackStyle);
        //         if (i < height - 1)
        //         {
        //             yield return Segment.LineBreak;
        //         }
        //     }
        //     yield break;
        // }

        // Calculate thumb size and position
        var thumbSize = CalculateThumbSize(height, itemsCount, pageSize, minThumbHeight);
        var thumbPosition = CalculateThumbPosition(height, itemsCount, pageSize, offset, thumbSize);


        for (int i = 0; i < height; i++)
        {
            if (i >= thumbPosition && i < thumbPosition + thumbSize)
            {
                yield return new Segment(thumbChar, thumbStyle);
            }
            else
            {
                yield return new Segment(trackChar, trackStyle);
            }

            if (i < height - 1)
            {
                yield return Segment.LineBreak;
            }
        }
    }

    private static int CalculateThumbSize(int viewportHeight, int itemsCount, int pageSize, int minThumbHeight)
    {
        // Thumb size represents the proportion of visible items to total items
        var proportionalSize = (int)Math.Ceiling((double)pageSize / itemsCount * viewportHeight);
        return Math.Max(minThumbHeight, Math.Min(proportionalSize, viewportHeight));
    }

    private static int CalculateThumbPosition(int viewportHeight, int itemsCount, int pageSize, int offset, int thumbSize)
    {
        // Maximum offset is when the last page is shown
        var maxOffset = Math.Max(0, itemsCount - pageSize);
        if (maxOffset == 0)
        {
            return 0;
        }

        // Calculate available space for thumb movement
        var availableSpace = viewportHeight - thumbSize;
        if (availableSpace <= 0)
        {
            return 0;
        }

        // Calculate position based on scroll percentage
        // Thumb should reach the end (availableSpace) when offset reaches maxOffset
        var scrollPercentage = (double)offset / maxOffset;
        var position = (int)Math.Floor(scrollPercentage * availableSpace);

        return Math.Clamp(position, 0, availableSpace);
    }
}

