// Copyright (c) RazorConsole. All rights reserved.

namespace RazorConsole.Testing;

/// <summary>
/// The exception thrown when a <see cref="HeadlessTerminal"/> wait does not reach the requested state in time.
/// </summary>
public sealed class HeadlessTerminalTimeoutException : TimeoutException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HeadlessTerminalTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the timeout.</param>
    /// <param name="diagnostics">The terminal diagnostics captured when the wait timed out.</param>
    public HeadlessTerminalTimeoutException(string message, string diagnostics)
        : base(message)
    {
        Diagnostics = diagnostics;
    }

    /// <summary>
    /// Gets the terminal diagnostics captured when the wait timed out,
    /// in the same format as <see cref="HeadlessTerminal.DumpDiagnostics(int)"/>.
    /// </summary>
    public string Diagnostics { get; }
}
