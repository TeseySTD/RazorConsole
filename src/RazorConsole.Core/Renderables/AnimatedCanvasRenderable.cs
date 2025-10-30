using System;
using System.Collections.Generic;
using System.Reflection;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class AnimatedCanvasRenderable : IRenderable, IAnimatedConsoleRenderable
{
    private readonly int _width;
    private readonly int _height;
    private readonly int _pixelWidth;
    private readonly bool _scale;
    private readonly int? _maxWidth;
    private readonly Action<int, (int x, int y, Color color)[]> _setFrameFunction;
    private readonly TimeSpan _interval;
    private static readonly DateTime GlobalStartTime = DateTime.UtcNow;

    private (int x, int y, Color color)[] _pixels;

    public AnimatedCanvasRenderable(int width, int height, int pixelWidth, bool scale, int? maxWidth,
        Action<int, (int x, int y, Color color)[]> setFrameFunction, TimeSpan interval)
    {
        _width = width;
        _height = height;
        _pixelWidth = pixelWidth;
        _scale = scale;
        _maxWidth = maxWidth;
        _setFrameFunction = setFrameFunction;
        _interval = interval;
        _pixels = new (int x, int y, Color color)[width * height];
    }

    public TimeSpan RefreshInterval => _interval;

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        var canvas = BuildCanvas();
        var measureMethod = typeof(Canvas).GetMethod("Measure", BindingFlags.NonPublic | BindingFlags.Instance);
        return (Measurement)measureMethod?.Invoke(canvas, new object[] { options, maxWidth })!;
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var canvas = BuildCanvas();
        var renderMethod = typeof(Canvas).GetMethod("Render", BindingFlags.NonPublic | BindingFlags.Instance);
        return (IEnumerable<Segment>)renderMethod?.Invoke(canvas, new object[] { options, maxWidth })!;
    }

    private Canvas BuildCanvas()
    {
        _setFrameFunction(GetCurrentFrameIndex(), _pixels);
        var canvas = new Canvas(_width, _height);
        if (_maxWidth.HasValue) canvas.MaxWidth = _maxWidth.Value;
        canvas.PixelWidth = _pixelWidth;
        canvas.Scale = _scale;

        foreach (var p in _pixels)
        {
            canvas.SetPixel(p.x, p.y, p.color);
        }

        return canvas;
    }

    private int GetCurrentFrameIndex()
    {
        var elapsed = DateTime.UtcNow - GlobalStartTime;
        return (int)(elapsed.Ticks / _interval.Ticks);

    }
}
