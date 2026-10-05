// Copyright (c) RazorConsole. All rights reserved.

using Spectre.Console;

namespace RazorConsole.Core.Layout;

public sealed class RuleWidget : Widget
{
    public RuleWidget(
        string vnodeId,
        Style? style = null,
        string? key = null,
        IReadOnlyDictionary<string, string?>? attributes = null,
        int zIndex = 0)
        : base(vnodeId, key, attributes, zIndex: zIndex)
    {
        Style = style ?? new Style(Color.Grey);
    }

    public Style Style { get; }

    protected override LayoutSize MeasureCore(LayoutContext context, BoxConstraints constraints)
        => constraints.Constrain(new LayoutSize(constraints.MaxWidth, constraints.MaxHeight == 0 ? 0 : 1));

    protected override void ArrangeCore(LayoutContext context, LayoutRect bounds)
    {
    }

    protected override void PaintCore(PaintContext context)
    {
        if (Bounds.IsEmpty)
        {
            return;
        }

        context.Canvas.Fill(new LayoutRect(Bounds.X, Bounds.Y, Bounds.Width, 1), '─', Style);
    }
}
