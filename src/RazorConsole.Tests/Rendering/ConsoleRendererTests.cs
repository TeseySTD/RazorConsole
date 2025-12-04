// Copyright (c) RazorConsole. All rights reserved.

#pragma warning disable BL0006 // RenderTree types are "internal-ish"; acceptable for console renderer tests.
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RazorConsole.Core.Rendering;

namespace RazorConsole.Tests.Rendering;

public sealed class ConsoleRendererTests
{
    [Fact]
    public async Task ElementCapturesComponentChildren_AsRenderableNodes()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        var snapshot = await renderer.MountComponentAsync<ContainerComponent>(ParameterView.Empty, CancellationToken.None);

        var root = snapshot.Root.ShouldBeOfType<Core.Vdom.VNode>();
        root.Kind.ShouldBe(Core.Vdom.VNodeKind.Element);
        root.TagName.ShouldBe("div");
        Enumerate(root).Skip(1).ShouldNotContain(node => node.Kind == Core.Vdom.VNodeKind.Component);

        root.Children.ShouldHaveSingleItem();
        var span = root.Children.Single();
        span.Kind.ShouldBe(Core.Vdom.VNodeKind.Element);
        span.TagName.ShouldBe("span");
        span.Children.ShouldHaveSingleItem();
        var text = span.Children.Single();
        text.Kind.ShouldBe(Core.Vdom.VNodeKind.Text);
        text.Text.ShouldBe("child");
    }

    [Fact]
    public async Task MountComponentAsync_WithValidComponent_ReturnsSnapshot()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        var snapshot = await renderer.MountComponentAsync<SimpleComponent>(ParameterView.Empty, CancellationToken.None);

        snapshot.Root.ShouldNotBeNull();
        snapshot.Renderable.ShouldNotBeNull();
    }

    [Fact]
    public async Task MountComponentAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        using var renderer = TestHelpers.CreateTestRenderer();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await renderer.MountComponentAsync<SimpleComponent>(ParameterView.Empty, cts.Token));
    }

    [Fact]
    public async Task MountComponentAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var renderer = TestHelpers.CreateTestRenderer();
        renderer.Dispose();

        await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await renderer.MountComponentAsync<SimpleComponent>(ParameterView.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task MountComponentAsync_WithParameters_PassesParametersToComponent()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            { "Text", "Test" }
        });

        var snapshot = await renderer.MountComponentAsync<ParameterComponent>(parameters, CancellationToken.None);

        var root = snapshot.Root.ShouldBeOfType<Core.Vdom.VNode>();
        root.Kind.ShouldBe(Core.Vdom.VNodeKind.Element);
        root.TagName.ShouldBe("div");
        root.Children.ShouldHaveSingleItem();

        var text = root.Children.Single();
        text.Kind.ShouldBe(Core.Vdom.VNodeKind.Text);
        text.Text.ShouldBe("Test");
    }

    [Fact]
    public async Task UpdatesAttribute_InsideRegion_AppliesCorrectly()
    {
        // Arrange
        using var renderer = TestHelpers.CreateTestRenderer();
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            { "Value", "Start" }
        });

        // Act
        var snapshot = await renderer.MountComponentAsync<RegionTestComponent>(parameters, CancellationToken.None);

        var element = FindDiv(snapshot.Root!);
        element.ShouldNotBeNull().Attributes["data-val"].ShouldBe("Start");

        // Prepare for Update: catch the snapshot where value becomes "End"
        var tcs = new TaskCompletionSource<ConsoleRenderer.RenderSnapshot>();
        using var sub = renderer.Subscribe(new SimpleObserver(s =>
        {
            var el = FindDiv(s.Root);
            if (el != null && el.Attributes.TryGetValue("data-val", out var val) && val == "End")
            {
                tcs.TrySetResult(s);
            }
        }));

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            // Update via captured instance
            await RegionTestComponent.Instance!.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                { "Value", "End" }
            }));
        });

        // Assert
        var updatedSnapshot = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        var updatedElement = FindDiv(updatedSnapshot.Root!);
        updatedElement.ShouldNotBeNull().Attributes["data-val"].ShouldBe("End");
    }

    [Fact]
    public async Task RemovesAttribute_InsideRegion_AppliesCorrectly()
    {
        // Arrange
        using var renderer = TestHelpers.CreateTestRenderer();
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            { "Value", "Start" },
            { "HasAttribute", true }
        });

        await renderer.MountComponentAsync<RegionTestComponent>(parameters, CancellationToken.None);

        // Act
        var tcs = new TaskCompletionSource<ConsoleRenderer.RenderSnapshot>();
        using var sub = renderer.Subscribe(new SimpleObserver(s =>
        {
            var el = FindDiv(s.Root);
            if (el != null && !el.Attributes.ContainsKey("data-val"))
            {
                tcs.TrySetResult(s);
            }
        }));

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await RegionTestComponent.Instance!.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                { "Value", "Start" },
                { "HasAttribute", false } // Trigger RemoveAttribute
            }));
        });

        // Assert
        var updatedSnapshot = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        var updatedElement = FindDiv(updatedSnapshot.Root!);
        updatedElement.ShouldNotBeNull().Attributes.ContainsKey("data-val").ShouldBeFalse();
    }

    [Fact]
    public async Task UpdatesText_InsideNestedRegions_AppliesRecursively()
    {
        // Arrange
        using var renderer = TestHelpers.CreateTestRenderer();
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            { "Text", "Initial" }
        });

        // Act 1: Initial Render
        var snapshot = await renderer.MountComponentAsync<TextRegionComponent>(parameters, CancellationToken.None);

        var initialTexts = FindTextNodes(snapshot.Root).ToList();
        initialTexts.Count.ShouldBe(2);
        initialTexts.All(t => t.Text == "Initial").ShouldBeTrue();

        // Prepare for Update
        var tcs = new TaskCompletionSource<ConsoleRenderer.RenderSnapshot>();
        using var sub = renderer.Subscribe(new SimpleObserver(s =>
        {
            // Wait until tree has two "Updated"
            var currentTexts = FindTextNodes(s.Root).ToList();
            if (currentTexts.Any(t => t.Text == "Updated"))
            {
                tcs.TrySetResult(s);
            }
        }));

        // Act 2: Update Parameter
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await TextRegionComponent.Instance!.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                { "Text", "Updated" }
            }));
        });

        // Assert
        var updatedSnapshot = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        var finalTexts = FindTextNodes(updatedSnapshot.Root).ToList();
        finalTexts.Count.ShouldBe(2);
        finalTexts.All(t => t.Text == "Updated").ShouldBeTrue();
    }

    // Helpers
    private IEnumerable<Core.Vdom.VNode> FindTextNodes(Core.Vdom.VNode? node)
    {
        if (node == null)
        {
            yield break;
        }

        if (node.Kind == Core.Vdom.VNodeKind.Text)
        {
            yield return node;
        }

        foreach (var child in node.Children)
        {
            foreach (var textNode in FindTextNodes(child))
            {
                yield return textNode;
            }
        }
    }

    private sealed class TextRegionComponent : ComponentBase
    {
        public static TextRegionComponent? Instance;
        public TextRegionComponent() => Instance = this;

        [Parameter] public string? Text { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");

            builder.OpenRegion(1);         // External region
            builder.AddContent(2, Text);   // Text 1

            builder.OpenRegion(3);         // Nested region
            builder.AddContent(4, Text);   // Text 2 (nested)
            builder.CloseRegion();

            builder.CloseRegion();
            builder.CloseElement();
        }
    }

    private Core.Vdom.VNode? FindDiv(Core.Vdom.VNode? node)
    {
        if (node == null)
        {
            return null;
        }

        if (node.Kind == Core.Vdom.VNodeKind.Element && node.TagName == "div")
        {
            return node;
        }

        foreach (var child in node.Children)
        {
            var found = FindDiv(child);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    private sealed class RegionTestComponent : ComponentBase
    {
        public static RegionTestComponent? Instance;
        public RegionTestComponent() => Instance = this;

        [Parameter] public string? Value { get; set; }
        [Parameter] public bool HasAttribute { get; set; } = true;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenRegion(0);
            builder.OpenElement(1, "div");
            if (HasAttribute)
            {
                builder.AddAttribute(2, "data-val", Value);
            }
            builder.CloseElement();
            builder.CloseRegion();
        }
    }

    private sealed class SimpleObserver(Action<ConsoleRenderer.RenderSnapshot> onNext) : IObserver<ConsoleRenderer.RenderSnapshot>
    {
        public void OnCompleted() { }
        public void OnError(Exception error) { }
        public void OnNext(ConsoleRenderer.RenderSnapshot value) => onNext(value);
    }
    private static IEnumerable<Core.Vdom.VNode> Enumerate(Core.Vdom.VNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        {
            foreach (var descendant in Enumerate(child))
            {
                yield return descendant;
            }
        }
    }

    private sealed class ContainerComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.OpenComponent<ChildComponent>(1);
            builder.CloseComponent();
            builder.CloseElement();
        }
    }

    private sealed class ChildComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "span");
            builder.AddContent(1, "child");
            builder.CloseElement();
        }
    }

    private sealed class SimpleComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, "Simple");
            builder.CloseElement();
        }
    }

    private sealed class ParameterComponent : ComponentBase
    {
        [Parameter]
        public string? Text { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, Text ?? "Empty");
            builder.CloseElement();
        }
    }
}
#pragma warning restore BL0006

