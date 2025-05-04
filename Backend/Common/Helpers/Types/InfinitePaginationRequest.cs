using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web;

namespace Backend.Common.Helpers.Types;

public class InfiniteCursorPaginationMeta
{
    public string? Cursor { get; set; } = string.Empty;

    [Required]
    [Range(1, 100)]
    public int PageSize { get; set; }
}

