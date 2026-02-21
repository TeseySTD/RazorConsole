// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Abstractions.Rendering;
using RazorConsole.Core.Rendering.Renderables;
using RazorConsole.Core.Rendering.Translation.Contexts;
using RazorConsole.Core.Rendering.Translation.Translators;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Tests.Rendering.Translation.Translators;

public sealed class ScrollableTranslatorTests
{
    private readonly ScrollableTranslator _translator = new();
    private readonly TranslationContext _context;
    private readonly TranslationDelegate _next;

    public ScrollableTranslatorTests()
    {
        // Setup a context with a dummy middleware that always returns a renderable.
        // This ensures TranslationHelpers.TryConvertChildrenToRenderables succeeds.
        var dummyMiddleware = Substitute.For<ITranslationMiddleware>();
        dummyMiddleware.Translate(Arg.Any<TranslationContext>(), Arg.Any<TranslationDelegate>(), Arg.Any<VNode>())
            .Returns(Substitute.For<IRenderable>());

        _context = new TranslationContext([dummyMiddleware]);
        _next = Substitute.For<TranslationDelegate>();
    }

    [Fact]
    public void Translate_WithInvalidTagName_CallsNext()
    {
        // Arrange
        var node = VNode.CreateElement("div");

        // Act
        _translator.Translate(_context, _next, node);

        // Assert
        _next.Received(1).Invoke(node);
    }

    [Fact]
    public void Translate_WithMultipleScrollbars_CallsNext()
    {
        // Arrange
        var node = VNode.CreateElement("scrollable");

        var scrollbar1 = VNode.CreateElement("div");
        scrollbar1.SetAttribute("data-scrollbar", "true");

        var scrollbar2 = VNode.CreateElement("div");
        scrollbar2.SetAttribute("data-scrollbar", "true");

        node.AddChild(scrollbar1);
        node.AddChild(scrollbar2);

        // Act
        _translator.Translate(_context, _next, node);

        // Assert
        _next.Received(1).Invoke(node);
    }

    [Fact]
    public void Translate_WithNoScrollbar_ReturnsRows()
    {
        // Arrange
        var node = VNode.CreateElement("scrollable");
        var content = VNode.CreateElement("div");
        node.AddChild(content);

        // Act
        var result = _translator.Translate(_context, _next, node);

        // Assert
        result.ShouldBeOfType<Rows>();
        _next.DidNotReceive().Invoke(Arg.Any<VNode>());
    }

    [Fact]
    public void Translate_WithValidAttributes_ReturnsScrollableWithBarRenderable()
    {
        // Arrange
        var node = CreateValidParentNode();
        var scrollbar = CreateValidScrollbarNode();
        node.AddChild(scrollbar);

        // Act
        var result = _translator.Translate(_context, _next, node);

        // Assert
        result.ShouldBeOfType<ScrollableRenderable>();
        _next.DidNotReceive().Invoke(Arg.Any<VNode>());
    }

    [Theory]
    [InlineData("data-offset", "invalid")]
    [InlineData("data-page-size", "invalid")]
    [InlineData("data-enable-embedded", "not-bool")]
    public void Translate_WithInvalidParentAttributes_CallsNext(string attr, string value)
    {
        // Arrange
        var node = CreateValidParentNode();
        node.SetAttribute(attr, value); // Overwrite with invalid value

        var scrollbar = CreateValidScrollbarNode();
        node.AddChild(scrollbar);

        // Act
        _translator.Translate(_context, _next, node);

        // Assert
        _next.Received(1).Invoke(node);
    }

    [Theory]
    [InlineData("data-track-char", "too-long")]
    [InlineData("data-thumb-char", "")]
    [InlineData("data-track-color", "invalid-color")]
    [InlineData("data-thumb-color", "invalid-color")]
    [InlineData("data-min-thumb-height", "-1")]
    public void Translate_WithInvalidScrollbarAttributes_CallsNext(string attr, string value)
    {
        // Arrange
        var node = CreateValidParentNode();

        var scrollbar = CreateValidScrollbarNode();
        scrollbar.SetAttribute(attr, value); // Overwrite with invalid value

        node.AddChild(scrollbar);

        // Act
        _translator.Translate(_context, _next, node);

        // Assert
        _next.Received(1).Invoke(node);
    }

    // -- Helpers --
    private static VNode CreateValidParentNode()
    {
        var node = VNode.CreateElement("scrollable");
        node.SetAttribute("data-items-count", "10");
        node.SetAttribute("data-offset", "0");
        node.SetAttribute("data-page-size", "5");
        node.SetAttribute("data-enable-embedded", "true");
        return node;
    }

    private static VNode CreateValidScrollbarNode()
    {
        var node = VNode.CreateElement("div");
        node.SetAttribute("data-scrollbar", "true");
        node.SetAttribute("data-track-char", "|");
        node.SetAttribute("data-thumb-char", "#");
        node.SetAttribute("data-track-color", "#808080");
        node.SetAttribute("data-thumb-color", "#FFFFFF");
        node.SetAttribute("data-min-thumb-height", "1");
        return node;
    }
}
