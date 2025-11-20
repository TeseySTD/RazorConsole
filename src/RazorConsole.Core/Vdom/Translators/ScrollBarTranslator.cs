using RazorConsole.Core.Renderables;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class ScrollBarTranslator : IVdomElementTranslator
{
    public int Priority => 10;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("data-scrollbar", out var value) ||
            !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("data-items-count", out var itemsCountStr) ||
            !int.TryParse(itemsCountStr, out var itemsCount))
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("data-offset", out var offsetStr) ||
            !int.TryParse(offsetStr, out var offset))
        {
            return false;
        }
        if (!node.Attributes.TryGetValue("data-page-size", out var pageSizeStr) ||
            !int.TryParse(pageSizeStr, out var pageSize))
        {
            return false;
        }

        // Optional viewport height
        int? viewportHeight = null;
        if (node.Attributes.TryGetValue("data-viewport-height", out var viewportHeightStr) &&
            int.TryParse(viewportHeightStr, out var vh))
        {
            viewportHeight = vh;
        }

        // Extract styling parameters
        var trackChar = VdomSpectreTranslator.GetAttribute(node, "data-track-char") ?? "│";
        var thumbChar = VdomSpectreTranslator.GetAttribute(node, "data-thumb-char") ?? "█";

        var trackColor = Color.Grey;
        if (node.Attributes.TryGetValue("data-track-color", out var trackColorStr) &&
            !string.IsNullOrEmpty(trackColorStr) &&
            Color.TryFromHex(trackColorStr, out var tc))
        {
            trackColor = tc;
        }

        var thumbColor = Color.White;
        if (node.Attributes.TryGetValue("data-thumb-color", out var thumbColorStr) &&
            !string.IsNullOrEmpty(thumbColorStr) &&
            Color.TryFromHex(thumbColorStr, out var thc))
        {
            thumbColor = thc;
        }

        var minThumbHeight = 1;
        if (node.Attributes.TryGetValue("data-min-thumb-height", out var minThumbHeightStr) &&
            int.TryParse(minThumbHeightStr, out var mth))
        {
            minThumbHeight = Math.Max(1, mth);
        }

        // If viewport height is not specified, use a default value
        var actualViewportHeight = viewportHeight ?? 10;

        renderable = new ScrollBarRenderable(
            itemsCount,
            offset,
            pageSize,
            actualViewportHeight,
            trackChar,
            thumbChar,
            trackColor,
            thumbColor,
            minThumbHeight
        );

        return true;
    }
}

