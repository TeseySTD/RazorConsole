// Copyright (c) RazorConsole. All rights reserved.

using Spectre.Console.Rendering;
using RazorConsole.Core.Utilities;

namespace RazorConsole.Core.Renderables;

public sealed class OverlayRenderable(IRenderable background, IEnumerable<OverlayItem> overlays) : IRenderable
{
    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        return background.Measure(options, maxWidth);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidthh)
    {
        foreach (var segment in background.Render(options, maxWidthh))
        {
            yield return segment;
        }

        foreach (var overlay in overlays.OrderBy(o => o.ZIndex))
        {
            yield return new Segment(AnsiSequences.CUP(overlay.Top + 1, overlay.Left + 1));

            foreach (var segment in overlay.Renderable.Render(options, maxWidthh))
            {
                yield return segment;
            }
        }
    }
}

public record OverlayItem(IRenderable Renderable, int Top, int Left, int ZIndex);
