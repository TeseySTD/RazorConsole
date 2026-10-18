// Copyright (c) RazorConsole. All rights reserved.

using System.Collections.ObjectModel;
using System.Text;
using RazorConsole.Core.Layout;
using RazorConsole.Core.Vdom;

namespace RazorConsole.Testing;

/// <summary>
/// An immutable capture of one rendered terminal frame: the cell grid, focus, and layouts.
/// </summary>
public sealed class TerminalFrame
{
    private readonly TerminalCell[,] _cells;

    internal TerminalFrame(
        long frameNumber,
        DateTimeOffset observedAt,
        TerminalCanvas canvas,
        int terminalWidth,
        int terminalHeight,
        string? focusKey,
        IReadOnlyDictionary<string, VNodeLayoutInfo> layouts)
    {
        FrameNumber = frameNumber;
        ObservedAt = observedAt;
        Width = terminalWidth;
        Height = terminalHeight;
        FocusKey = focusKey;
        _cells = new TerminalCell[Height, Width];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                _cells[y, x] = x < canvas.Width && y < canvas.Height
                    ? canvas[x, y]
                    : TerminalCell.Empty;
            }
        }

        Layouts = new ReadOnlyDictionary<string, VNodeLayoutInfo>(
            new Dictionary<string, VNodeLayoutInfo>(layouts, StringComparer.Ordinal));
    }

    /// <summary>
    /// Gets the sequence number of the frame within its terminal.
    /// </summary>
    public long FrameNumber { get; }

    /// <summary>
    /// Gets the time at which the frame was captured.
    /// </summary>
    public DateTimeOffset ObservedAt { get; }

    /// <summary>
    /// Gets the terminal width in columns when the frame was captured.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the terminal height in rows when the frame was captured.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the focus key of the focused element when the frame was captured, if any.
    /// </summary>
    public string? FocusKey { get; }

    /// <summary>
    /// Gets the layouts of the elements that have a <c>data-vnode-hook</c> attribute, keyed by hook.
    /// </summary>
    public IReadOnlyDictionary<string, VNodeLayoutInfo> Layouts { get; }

    /// <summary>
    /// Gets the cell at the zero-based column <paramref name="x"/> and row <paramref name="y"/>.
    /// </summary>
    /// <param name="x">The zero-based column.</param>
    /// <param name="y">The zero-based row.</param>
    public TerminalCell this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            return _cells[y, x];
        }
    }

    /// <summary>
    /// Gets the plain text of the screen, one line per row separated by <c>\n</c>.
    /// </summary>
    public string ScreenText
    {
        get
        {
            var builder = new StringBuilder();
            for (var y = 0; y < Height; y++)
            {
                if (y > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(GetLine(y));
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// Returns whether the screen text contains <paramref name="text"/> (ordinal comparison).
    /// </summary>
    /// <param name="text">The text to look for.</param>
    public bool ContainsText(string text)
        => ScreenText.Contains(text, StringComparison.Ordinal);

    /// <summary>
    /// Returns whether this frame has the same size and cells as <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The frame to compare with.</param>
    public bool HasSameCells(TerminalFrame other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Width != other.Width || Height != other.Height)
        {
            return false;
        }
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (this[x, y] != other[x, y])
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Gets the plain text of one row.
    /// </summary>
    /// <param name="row">The zero-based row.</param>
    public string GetLine(int row)
    {
        if (row < 0 || row >= Height)
        {
            throw new ArgumentOutOfRangeException(nameof(row));
        }

        var builder = new StringBuilder();
        for (var x = 0; x < Width; x++)
        {
            builder.Append(_cells[row, x].Text);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the layout captured for the element with the given <c>data-vnode-hook</c>.
    /// </summary>
    /// <param name="hookKey">The hook key.</param>
    /// <exception cref="KeyNotFoundException">No layout was captured for <paramref name="hookKey"/>.</exception>
    public VNodeLayoutInfo GetLayout(string hookKey)
        => Layouts.TryGetValue(hookKey, out var layout)
            ? layout
            : throw new KeyNotFoundException($"No layout was captured for hook '{hookKey}'.");

    /// <summary>
    /// Tries to get the layout captured for the element with the given <c>data-vnode-hook</c>.
    /// </summary>
    /// <param name="hookKey">The hook key.</param>
    /// <param name="layout">The layout, when found.</param>
    public bool TryGetLayout(string hookKey, out VNodeLayoutInfo layout)
        => Layouts.TryGetValue(hookKey, out layout);

    /// <summary>
    /// Formats the frame for failure messages: header, screen lines, and layouts.
    /// </summary>
    public string ToDiagnosticString()
    {
        var builder = new StringBuilder();
        builder.Append("Frame ").Append(FrameNumber)
            .Append(" (").Append(Width).Append('x').Append(Height).Append(')')
            .Append(", focus: ").AppendLine(FocusKey ?? "<none>");

        for (var y = 0; y < Height; y++)
        {
            builder.Append(GetLine(y).TrimEnd()).AppendLine();
        }

        if (Layouts.Count > 0)
        {
            builder.AppendLine("Layouts:");
            foreach (var pair in Layouts.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                var layout = pair.Value;
                builder.Append("  ").Append(pair.Key).Append(": ")
                    .Append("left=").Append(layout.Left)
                    .Append(", top=").Append(layout.Top)
                    .Append(", width=").Append(layout.Width)
                    .Append(", height=").Append(layout.Height)
                    .Append(", z=").Append(layout.ZIndex)
                    .AppendLine();
            }
        }

        return builder.ToString().TrimEnd();
    }

    /// <inheritdoc />
    public override string ToString() => ToDiagnosticString();
}
