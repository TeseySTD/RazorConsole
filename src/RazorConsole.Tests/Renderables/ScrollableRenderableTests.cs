// Copyright (c) RazorConsole. All rights reserved.

using System.Text;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.Renderables;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Tests.Renderables;

public class ScrollableRenderableTests
{
    private static RenderOptions CreateTestRenderOptions(int width = 80)
    {
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out = new AnsiConsoleOutput(TextWriter.Null)
        });
        return new RenderOptions(console.Profile.Capabilities, new Size(width, 25));
    }

    private static ScrollbarSettings CreateScrollbarSettings()
    {
        return new ScrollbarSettings
        {
            TrackChar = '│',
            ThumbChar = '█',
            TrackColor = Color.Grey,
            ThumbColor = Color.White,
            MinThumbHeight = 1
        };
    }

    private static string RenderToString(IRenderable renderable, int width = 80)
    {
        var options = CreateTestRenderOptions(width);
        var segments = renderable.Render(options, width);
        var sb = new StringBuilder();
        foreach (var segment in segments)
        {
            sb.Append(segment.Text);
        }

        return sb.ToString();
    }

    [Fact]
    public void Measure_EmbeddedMode_WithSettings_ReservesOneCharacter()
    {
        // Arrange
        var panel = new Panel("Content");
        var renderable = new ScrollableRenderable(
            [panel],
            totalItems: 10,
            offset: 0,
            pageSize: 5,
            enableEmbeddedScrollbar: true,
            new ScrollableLayoutCoordinator(),
            CreateScrollbarSettings());

        var options = CreateTestRenderOptions();

        // Act
        var measurement = renderable.Measure(options, 80);

        // Assert
        measurement.Max.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Measure_EmbeddedMode_WithoutSettings_ReservesNoCharacters()
    {
        // Arrange
        var panel = new Panel("Content");
        var renderable = new ScrollableRenderable(
            [panel],
            totalItems: 10,
            offset: 0,
            pageSize: 5,
            enableEmbeddedScrollbar: true,
            new ScrollableLayoutCoordinator(),
            null); // Null settings

        var options = CreateTestRenderOptions();

        // Act
        var measurement = renderable.Measure(options, 80);

        // Assert
        measurement.Max.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Measure_SideMode_ReturnsValidMeasurement()
    {
        // Arrange
        var markup = new Markup("Content");
        var renderable = new ScrollableRenderable(
            [markup],
            totalItems: 10,
            offset: 0,
            pageSize: 5,
            enableEmbeddedScrollbar: true,
            new ScrollableLayoutCoordinator(),
            new ScrollbarSettings());

        var options = CreateTestRenderOptions();

        // Act
        var measurement = renderable.Measure(options, 80);

        // Assert
        measurement.Min.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Render_CropLinesTrue_ReportsMaxOffsetToCoordinator()
    {
        // Arrange
        var text = new Text("Line1\nLine2\nLine3\nLine4\nLine5");
        var panel = new Panel(text);
        var coordinator = new ScrollableLayoutCoordinator();
        var scrollId = "test-id-1";

        var renderable = new ScrollableRenderable(
            [panel],
            totalItems: 0,
            offset: 0,
            pageSize: 2,
            enableEmbeddedScrollbar: true,
            scrollableLayoutCoordinator: coordinator,
            scrollbarSettings: null,
            cropLines: true,
            scrollId: scrollId);

        var options = CreateTestRenderOptions();

        // Act
        _ = renderable.Render(options, 80).ToList();

        // Assert
        var maxOffset = coordinator.GetMaxOffset(scrollId);
        // - 2 because of panel border
        maxOffset.ShouldBe(text.Lines - 2);
    }

    [Fact]
    public void Render_CropLinesTrue_WithOffset_CropsContentCorrectly()
    {
        // Arrange
        var text = new Text("Line1\nLine2\nLine3\nLine4\nLine5");
        var panel = new Panel(text) { Border = BoxBorder.Ascii };
        var coordinator = new ScrollableLayoutCoordinator();

        var renderable = new ScrollableRenderable(
            [panel],
            totalItems: 0,
            offset: 2, // Пропускаємо Line1 і Line2
            pageSize: 2, // Беремо Line3 і Line4
            enableEmbeddedScrollbar: true,
            scrollableLayoutCoordinator: coordinator,
            scrollbarSettings: null,
            cropLines: true,
            scrollId: "test-id-2");

        // Act
        var output = RenderToString(renderable);

        // Assert
        output.ShouldContain("Line3");
        output.ShouldContain("Line4");

        output.ShouldNotContain("Line1");
        output.ShouldNotContain("Line2");
        output.ShouldNotContain("Line5");

        output.ShouldContain("-"); //  Ascii border should not be erased
    }

    [Fact]
    public void Render_PageSizeGreaterThanContent_DoesNotDuplicateBorders()
    {
        // Arrange
        var text = new Text("Only1Line");
        var panel = new Panel(text);
        var coordinator = new ScrollableLayoutCoordinator();

        var renderable = new ScrollableRenderable(
            [panel],
            totalItems: 0,
            offset: 0,
            pageSize: 10, // More than content lines
            enableEmbeddedScrollbar: true,
            scrollableLayoutCoordinator: coordinator,
            scrollbarSettings: null,
            cropLines: true,
            scrollId: "test-id-3");

        // Act
        var output = RenderToString(renderable);
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Assert
        // BorderUp + 1 line + BorderDown
        lines.Length.ShouldBe(3);
    }

    [Fact]
    public void Render_WithoutScrollbarSettings_DoesNotRenderThumb()
    {
        // Arrange
        var text = new Text("Line1\nLine2\nLine3");
        var panel = new Panel(text);
        var coordinator = new ScrollableLayoutCoordinator();

        var renderable = new ScrollableRenderable(
            [panel],
            totalItems: 0,
            offset: 0,
            pageSize: 2,
            enableEmbeddedScrollbar: true,
            scrollableLayoutCoordinator: coordinator,
            scrollbarSettings: null,
            cropLines: true,
            scrollId: "test-id-4");

        // Act
        var output = RenderToString(renderable);

        // Assert
        output.ShouldNotContain("█");
    }

    [Fact]
    public void Render_WithScrollbarSettings_RendersScrollbarThumb()
    {
        // Arrange
        var text = new Text("Line1\nLine2\nLine3");
        var panel = new Panel(text);
        var coordinator = new ScrollableLayoutCoordinator();
        var settings = CreateScrollbarSettings();

        var renderable = new ScrollableRenderable(
            [panel],
            totalItems: 0,
            offset: 0,
            pageSize: 2,
            enableEmbeddedScrollbar: true,
            scrollableLayoutCoordinator: coordinator,
            scrollbarSettings: settings,
            cropLines: true,
            scrollId: "test-id-5");

        // Act
        var output = RenderToString(renderable);

        // Assert
        output.ShouldContain("█");
    }

    [Fact]
    public void Render_SideMode_ReturnsSegmentsWithScrollbar()
    {
        // Arrange
        var markup = new Markup("Content");
        var renderable = new ScrollableRenderable(
            [markup],
            totalItems: 10,
            offset: 0,
            pageSize: 5,
            enableEmbeddedScrollbar: false,
            new ScrollableLayoutCoordinator(),
            CreateScrollbarSettings());

        var options = CreateTestRenderOptions();

        // Act
        var segments = renderable.Render(options, 80).ToList();

        // Assert
        segments.ShouldNotBeEmpty();
    }

    [Fact]
    public void Render_WithEmptyItems_StillRendersContainer()
    {
        // Arrange
        var panel = new Panel("Empty");
        var renderable = new ScrollableRenderable(
            [panel],
            totalItems: 0,
            offset: 0,
            pageSize: 5,
            enableEmbeddedScrollbar: true,
            new ScrollableLayoutCoordinator(),
            CreateScrollbarSettings());

        var options = CreateTestRenderOptions();

        // Act
        var segments = renderable.Render(options, 80).ToList();

        // Assert
        segments.ShouldNotBeEmpty();
    }
}
