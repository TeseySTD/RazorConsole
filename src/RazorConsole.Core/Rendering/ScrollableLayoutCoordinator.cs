// Copyright (c) RazorConsole. All rights reserved.

using System.Collections.Concurrent;

namespace RazorConsole.Core.Rendering;

public class ScrollableLayoutCoordinator
{
    private readonly ConcurrentDictionary<string, int> _maxOffsets = new();

    public void ReportMaxOffset(string scrollId, int maxOffset)
    {
        if (!string.IsNullOrEmpty(scrollId))
        {
            _maxOffsets[scrollId] = maxOffset;
        }
    }

    public int GetMaxOffset(string? scrollId) => _maxOffsets.GetValueOrDefault(scrollId ?? string.Empty, int.MaxValue);

    public void Remove(string scrollId)
    {
        if (!string.IsNullOrEmpty(scrollId))
        {
            _maxOffsets.TryRemove(scrollId, out _);
        }
    }
}
