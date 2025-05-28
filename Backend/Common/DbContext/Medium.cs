namespace Backend.Common.DbContext;

public abstract class Medium
{
    public Guid MediaId { get; set; }

    public string? Type { get; set; } = null!;

    public virtual string? OwnerType { get; set; }

    public string Path { get; set; } = null!;

    public string? Url { get; set; } = null!;

    [Obsolete] public string? ThumbnailUrl { get; set; }

    public DateTime UploadedAt { get; set; }
}