namespace Backend.Common.DbContext;

public partial class MessageMedium
{
    public Guid MessageMediaId { get; set; }

    public Guid MessageId { get; set; }

    public string MediaType { get; set; } = null!;

    public string MediaUrl { get; set; } = null!;

    public string? FileName { get; set; }

    public long? FileSizeBytes { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Message Message { get; set; } = null!;
}
