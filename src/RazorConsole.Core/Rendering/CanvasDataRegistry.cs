using System.Collections.Concurrent;
using Spectre.Console;

namespace RazorConsole.Core.Rendering;

public static class CanvasDataRegistry
{
    private static readonly ConcurrentDictionary<Guid, (int, int, Color)[]> Data = new();
    private static readonly ConcurrentDictionary<Guid, Action<int, (int x, int y, Color color)[]>> Delegates = new();

    public static void Register(Guid id, (int, int, Color)[] data) => Data.AddOrUpdate(id, data, (_, _) => data);
    public static void RegisterDelegate (Guid id, Action<int, (int x, int y, Color color)[]> setFrameFunction) => Delegates.AddOrUpdate(id, setFrameFunction, (_, _) => setFrameFunction);

    public static void Unregister(Guid id) => Data.TryRemove(id, out _);
    public static void UnregisterDelegate (Guid id) => Delegates.TryRemove(id, out _);

    public static bool TryGetData(Guid id, out (int, int, Color)[]? data) => Data.TryGetValue(id, out data);
    public static bool TryGetDelegate(Guid id, out Action<int, (int x, int y, Color color)[]>? setFrameFunction) => Delegates.TryGetValue(id, out setFrameFunction);
}
