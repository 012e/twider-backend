namespace Backend.Common.Helpers.Types;

using System;
using System.Collections.Generic;

public class InfiniteCursorPage<T>(
    IReadOnlyList<T> items,
    string? nextCursor,
    bool hasMore)
{
    public IReadOnlyList<T> Items { get; init; } = items;
    public string? NextCursor { get; init; } = nextCursor;
    public bool HasMore { get; init; } = hasMore;
}
