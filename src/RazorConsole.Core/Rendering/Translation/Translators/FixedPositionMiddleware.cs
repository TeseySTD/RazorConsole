// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Abstractions.Rendering;
using RazorConsole.Core.Renderables;
using RazorConsole.Core.Rendering.Translation.Contexts;
using RazorConsole.Core.Rendering.Vdom;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Translation.Translators;

public class FixedPositionMiddleware : ITranslationMiddleware
{
    public IRenderable Translate(TranslationContext context, TranslationDelegate next, VNode node)
    {
        var position = VdomSpectreTranslator.GetAttribute(node, "position");

        if (!string.Equals(position, "fixed", StringComparison.OrdinalIgnoreCase))
        {
            return next(node);
        }

        var top = VdomSpectreTranslator.GetAttribute(node, "top") != null
            ? int.Parse(VdomSpectreTranslator.GetAttribute(node, "top")!, 0)
            : (int?)null;
        var left = VdomSpectreTranslator.GetAttribute(node, "left") != null
            ? int.Parse(VdomSpectreTranslator.GetAttribute(node, "left")!, 0)
            : (int?)null;
        var bottom = VdomSpectreTranslator.GetAttribute(node, "bottom") != null
            ? int.Parse(VdomSpectreTranslator.GetAttribute(node, "bottom")!, 0)
            : (int?)null;
        var right = VdomSpectreTranslator.GetAttribute(node, "right") != null
            ? int.Parse(VdomSpectreTranslator.GetAttribute(node, "right")!, 0)
            : (int?)null;
        var zIndex = VdomSpectreTranslator.TryGetIntAttribute(node, "z-index", 0);

        var renderable = next(node);

        context.CollectedOverlays.Add(new OverlayItem(renderable, top, left, right, bottom, zIndex));

        // Return empty div in main layout flow
        return new Markup(string.Empty);
    }
}
