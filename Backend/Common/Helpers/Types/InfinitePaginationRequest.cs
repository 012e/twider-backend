using System.ComponentModel.DataAnnotations;

namespace Backend.Common.Helpers.Types;

public class InfiniteCursorPaginationMeta
{
    public string? Cursor { get; set; } = null;

    [Required] [Range(1, 100)] public int PageSize { get; set; }
}