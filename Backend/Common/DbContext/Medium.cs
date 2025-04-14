namespace Backend.Common.DbContext;

public partial class Medium
{
    public int MediaId { get; set; }

    public int PostId { get; set; }

    public string MediaType { get; set; } = null!;

    public string MediaUrl { get; set; } = null!;

    public string? ThumbnailUrl { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
