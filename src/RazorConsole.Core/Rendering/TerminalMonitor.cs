// Copyright (c) RazorConsole. All rights reserved.

using System.Runtime.InteropServices;
using Spectre.Console;

namespace RazorConsole.Core.Rendering;

internal sealed class TerminalMonitor : IDisposable
{
    public static readonly TimeSpan CheckInterval = TimeSpan.FromMilliseconds(250);
    private int _width = AnsiConsole.Console.Profile.Width;
    private int _height = AnsiConsole.Console.Profile.Height;
    private IDisposable? _posixRegistration;
    private CancellationTokenSource? _cts;
    private bool _isStarted = false;
#if NET9_0_OR_GREATER
    private readonly Lock _sync = new();
#else
    private readonly object _sync = new();
#endif

    public event Action? OnResized;

    public void Start(CancellationToken cancellationToken)
    {
#if NET9_0_OR_GREATER
        using (_sync.EnterScope())
#else
        lock (_sync)
#endif
        {
            if (_isStarted)
            {
                return;
            }
        }
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _posixRegistration = PosixSignalRegistration.Create(PosixSignal.SIGWINCH, _ => OnResized?.Invoke());
            _isStarted = true;
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = PollResizeAsync(_cts.Token);
        _isStarted = true;
    }

    private async Task PollResizeAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(CheckInterval);
        while (await timer.WaitForNextTickAsync(token))
        {
            if (AnsiConsole.Console.Profile.Width != _width || AnsiConsole.Console.Profile.Height != _height)
            {
                _width = AnsiConsole.Console.Profile.Width;
                _height = AnsiConsole.Console.Profile.Height;
                OnResized?.Invoke();
            }
        }
    }

    public void Dispose()
    {
        _posixRegistration?.Dispose();
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
