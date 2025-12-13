// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Rendering.Renderables;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Vdom.Translators;

public class ScrollableTranslator : IVdomElementTranslator
{
    public int Priority => 30;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;
        if (!string.Equals(node.TagName, "scrollable", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var itemsCount = VdomSpectreTranslator.TryGetIntAttribute(node, "data-items-count", 0);

        if (!int.TryParse(VdomSpectreTranslator.GetAttribute(node, "data-offset"), out var offset))
        {
            return false;
        }

        if (!VdomSpectreTranslator.TryParsePositiveInt(VdomSpectreTranslator.GetAttribute(node, "data-page-size"),
                out var pageSize))
        {
            return false;
        }


        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context,
                out var contentRenderable))
        {
            return false;
        }

        renderable = new ScrollableWithBarRenderable(contentRenderable, itemsCount, offset, pageSize);

        return true;
    }
}
