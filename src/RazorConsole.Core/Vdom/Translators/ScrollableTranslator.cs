// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Rendering.Renderables;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console;
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

        var scrollbars = node.Children.Where(n => VdomSpectreTranslator.TryGetBoolAttribute(n, "data-scrollbar", out var value) && value).ToList();
        // Accept only one scrollbar
        if (scrollbars.Count == 1)
        {
            var scrollbarNode = scrollbars.Single();

            // Extract styling parameters
            if (!char.TryParse(VdomSpectreTranslator.GetAttribute(scrollbarNode, "data-track-char"), out var trackChar))
            {
                return false;
            }

            if (!char.TryParse(VdomSpectreTranslator.GetAttribute(scrollbarNode, "data-thumb-char"), out var thumbChar))
            {
                return false;
            }

            if (!scrollbarNode.Attributes.TryGetValue("data-track-color", out var trackColorStr) ||
                string.IsNullOrEmpty(trackColorStr) ||
                !Color.TryFromHex(trackColorStr, out var trackColor))
            {
                return false;
            }

            if (!scrollbarNode.Attributes.TryGetValue("data-thumb-color", out var thumbColorStr) ||
                string.IsNullOrEmpty(thumbColorStr) ||
                !Color.TryFromHex(thumbColorStr, out var thumbColor))
            {
                return false;
            }

            if (!VdomSpectreTranslator.TryParsePositiveInt(
                    VdomSpectreTranslator.GetAttribute(scrollbarNode, "data-min-thumb-height"), out var minThumbHeight))
            {
                return false;
            }

            // Scrollbar cannot be translated explicitly
            // node.RemoveChildAt(node.Children.ToList().IndexOf(scrollbarNode));
            if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context,
                    out var contentRenderable))
            {
                return false;
            }

            renderable = new ScrollableWithBarRenderable(
                contentRenderable, itemsCount, offset, pageSize,
                trackColor: trackColor,
                thumbColor: thumbColor,
                trackChar: trackChar,
                thumbChar: thumbChar,
                minThumbHeight: minThumbHeight
            );
        }
        // If there is not any scrollbar - render just rows layout
        else if (!scrollbars.Any())
        {
            if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context,
                    out var contentRenderable))
            {
                return false;
            }

            renderable = new Rows(contentRenderable);
        }
        else // If there are many scrollbars - then component cannot be translated
        {
            return false;
        }

        return true;
    }
}
