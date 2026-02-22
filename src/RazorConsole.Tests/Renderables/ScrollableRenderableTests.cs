// Copyright (c) RazorConsole. All rights reserved.

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

    [Fact]
    public void Measure_EmbeddedMode_ReturnsValidMeasurement()
    {
        // Arrange: Single Panel + Enabled -> Embedded Mode
        var panel = new Panel("Content");
        var renderable = new ScrollableRenderable(
            [panel],
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
        measurement.Max.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Measure_SideMode_ReturnsValidMeasurement()
    {
        // Arrange: Markup (not Panel/Table) -> Side Mode
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
    public void Render_EmbeddedPanel_ReturnsSegments()
    {
        // Arrange
        var panel = new Panel("Content") { Border = BoxBorder.Rounded };
        var renderable = new ScrollableRenderable(
            [panel],
            totalItems: 10,
            offset: 0,
            pageSize: 5,
            enableEmbeddedScrollbar: true,
            new ScrollableLayoutCoordinator(),
            new ScrollbarSettings());

        var options = CreateTestRenderOptions();

        // Act
        var segments = renderable.Render(options, 80).ToList();

        // Assert
        segments.ShouldNotBeEmpty();
    }

    [Fact]
    public void Render_EmbeddedTable_ReturnsSegments()
    {
        // Arrange
        var table = new Table();
        table.AddColumn("Col");
        table.AddRow("Val");

        var renderable = new ScrollableRenderable(
            [table],
            totalItems: 10,
            offset: 0,
            pageSize: 5,
            enableEmbeddedScrollbar: true,
            new ScrollableLayoutCoordinator(),
            new ScrollbarSettings());

        var options = CreateTestRenderOptions();

        // Act
        var segments = renderable.Render(options, 80).ToList();

        // Assert
        segments.ShouldNotBeEmpty();
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
            new ScrollbarSettings()); // Explicit side mode

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
            new ScrollbarSettings());

        var options = CreateTestRenderOptions();

        // Act
        var segments = renderable.Render(options, 80).ToList();

        // Assert
        segments.ShouldNotBeEmpty();
    }
}
