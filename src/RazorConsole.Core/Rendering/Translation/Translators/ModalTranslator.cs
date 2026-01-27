// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Abstractions.Rendering;
using RazorConsole.Core.Rendering.Translation.Contexts;
using RazorConsole.Core.Rendering.Vdom;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Translation.Translators;

public class ModalTranslator : ITranslationMiddleware
{
    public IRenderable Translate(TranslationContext context, TranslationDelegate next, VNode node)
    {
        if (!IsModal(node))
        {
            return next(node);
        }
        var zindex = VdomSpectreTranslator.TryGetIntAttribute(node, "zindex", 9999);

        // Wrap in div because modal tag is not translated in middleware
        var nodeToRender = VNode.CreateElement("div");
        foreach (var child in node.Children)
        {
            nodeToRender.AddChild(child);
        }

        var renderable = next(nodeToRender);

        context.CollectedOverlays.Add(new OverlayItem(
            renderable,
            null,
            null,
            null,
            null,
            zindex,
            IsCentered: true
        ));

        return new Markup(string.Empty);
    }

    private static bool IsModal(VNode node)
        => node.Kind == VNodeKind.Element
           && string.Equals(node.TagName, "modal", StringComparison.OrdinalIgnoreCase);
}
