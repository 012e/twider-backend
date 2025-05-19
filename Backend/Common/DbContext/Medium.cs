namespace Backend.Common.DbContext;

public abstract class Medium
{
    public Guid MediaId { get; set; }

    public string MediaType { get; set; } = null!;

    public string MediaUrl { get; set; } = null!;

    public string? ThumbnailUrl { get; set; }

    public DateTime UploadedAt { get; set; }
}
