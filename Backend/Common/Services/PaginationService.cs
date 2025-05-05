using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers;
using Backend.Common.Helpers.Types;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Backend.Common.Services;

public static class InfinitePaginationService
{
    // Updated this overload to use Guid explicitly as the key type
    public static Task<InfiniteCursorPage<TResult>> PaginateAsync<T, TResult>(
        IQueryable<T> source,
        Expression<Func<T, Guid>> keySelector,
        Func<T, TResult> mapper,
        string? after = null,
        int limit = 10,
        bool ascending = true)
    {
        // Now explicitly passing Guid as the key type
        return PaginateInternalAsync(source, keySelector, mapper, after, limit, ascending);
    }

    // This overload already uses Guid as the key type
    public static Task<InfiniteCursorPage<T>> PaginateAsync<T>(
        IQueryable<T> source,
        Expression<Func<T, Guid>> keySelector,
        string? after = null,
        int limit = 10,
        bool ascending = true)
    {
        return PaginateInternalAsync<T, Guid, T>(
            source,
            keySelector,
            mapper: x => x,
            after: after,
            limit: limit,
            ascending: ascending
        );
    }

    // Renamed the main implementation method to clarify it's the internal implementation
    // and to avoid conflict with the public overloads
    private static async Task<InfiniteCursorPage<TResult>> PaginateInternalAsync<T, TKey, TResult>(
        IQueryable<T> source,
        Expression<Func<T, TKey>> keySelector,
        Func<T, TResult> mapper,
        string? after = null,
        int limit = 10,
        bool ascending = true)
        where TKey : struct, IEquatable<TKey> // Constraint for value types like Guid
    {
        // Get the total count of items in the source
        int totalCount = await source.CountAsync();

        if (totalCount == 0)
        {
            // Return an empty page if there are no items
            return new InfiniteCursorPage<TResult>(
                items: new List<TResult>(),
                nextCursor: null,
                hasMore: false
            );
        }

        TKey? cursorValue = null;
        if (after != null)
        {
            // Decode the cursor to get the key value
            try
            {
                string decodedCursor = CursorEncoder.Decode(after);

                // TODO: looks complicated, plz refactor this
                if (typeof(TKey) != typeof(Guid))
                {
                    throw new ValidationException("Invalid cursor type.");
                }
                cursorValue = (TKey)(object)Guid.Parse(decodedCursor);
            }
            catch (FormatException)
            {
                throw new ValidationException("Invalid cursor format.");
            }
        }

        // Create a copy of the original source to use for circular pagination
        var originalSource = source;

        if (cursorValue is not null)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Invoke(keySelector, param);
            var constant = Expression.Constant(cursorValue, typeof(TKey));
            Expression comparison = ascending
                ? Expression.GreaterThan(propertyAccess, constant)
                : Expression.LessThan(propertyAccess, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(comparison, param);
            source = source.Where(lambda);
        }

        source = ascending
            ? source.OrderBy(keySelector)
            : source.OrderByDescending(keySelector);

        // Get the initial batch of items after the cursor
        var items = await source.Take(limit).ToListAsync();

        // If we didn't get enough items, we need to loop around
        if (items.Count < limit && totalCount > 0 && items.Any())
        {
            int remaining = limit - items.Count;

            // We need to get items from the beginning, up to the point of the first item in the current batch
            var wrappedSource = ascending
                ? originalSource.OrderBy(keySelector)
                : originalSource.OrderByDescending(keySelector);

            // Get the key of the first item in the current batch to exclude it from the wrapped items
            var firstItemKey = GetKeyValue(items.First(), keySelector);

            // Add a Where clause to the wrapped source to exclude items already fetched
            var param = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Invoke(keySelector, param);
            var constant = Expression.Constant(firstItemKey, typeof(TKey));
            Expression comparison = ascending
                ? Expression.LessThan(propertyAccess, constant) // For ascending, get items less than the first key
                : Expression.GreaterThan(propertyAccess,
                    constant); // For descending, get items greater than the first key
            var lambda = Expression.Lambda<Func<T, bool>>(comparison, param);
            // Apply Where and then re-apply the ordering to maintain IOrderedQueryable
            IQueryable<T> filteredSource = wrappedSource.Where(lambda);
            wrappedSource = ascending
                ? filteredSource.OrderBy(keySelector)
                : filteredSource.OrderByDescending(keySelector);

            // Take the remaining items needed from the start
            var wrappedItems = await wrappedSource.Take(remaining).ToListAsync();

            // Combine the initial and wrapped items
            items.AddRange(wrappedItems);

            // Re-sort the combined list to ensure correct order, especially for wrapped items
            items = ascending
                ? items.OrderBy(item => GetKeyValue(item, keySelector)).ToList()
                : items.OrderByDescending(item => GetKeyValue(item, keySelector)).ToList();
        }

        // Determine if there are more items
        bool hasMore;
        if (items.Count == 0)
        {
            hasMore = false; // No items, so no more items
        }
        else if (items.Count < limit && totalCount <= limit)
        {
            hasMore = false; // Returned less than the limit and total is within the limit
        }
        else
        {
            // Check if there's at least one item after the last item returned based on the key
            var lastItemKey = GetKeyValue(items.Last(), keySelector);
            var param = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Invoke(keySelector, param);
            var constant = Expression.Constant(lastItemKey, typeof(TKey));
            Expression comparison = ascending
                ? Expression.GreaterThan(propertyAccess, constant)
                : Expression.LessThan(propertyAccess, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(comparison, param);
            hasMore = await originalSource.AnyAsync(lambda);
        }

        // Get the next cursor value for the last item we're returning
        string? nextCursor = items.Count > 0
            ? CursorEncoder.Encode(GetKeyValue(items.Last(), keySelector).ToString()!)
            : null;

        // Map the items to the result type
        var mappedItems = items.Select(mapper).ToList();

        return new InfiniteCursorPage<TResult>(
            items: mappedItems,
            nextCursor: nextCursor,
            hasMore: hasMore
        );
    }

    // Updated helper method to work with the generic TKey
    private static TKey GetKeyValue<T, TKey>(T item, Expression<Func<T, TKey>> keySelector)
    {
        return keySelector.Compile().Invoke(item);
    }
}