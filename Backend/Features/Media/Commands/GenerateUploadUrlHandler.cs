using Backend.Common.DbContext;
using Backend.Common.DbContext.Post;
using MediatR;
using Minio;
using Minio.DataModel.Args;

namespace Backend.Features.Media.Commands;

public class
    GenerateUploadUrlHandler : IRequestHandler<GenerateUploadUrlCommand, ApiResult<GenerateUploadUrlResponse>>
{
    private readonly IMinioClient _minioClient;
    private readonly ApplicationDbContext _db;

    public GenerateUploadUrlHandler(IMinioClient minioClient, ApplicationDbContext db)
    {
        _minioClient = minioClient;
        _db = db;
    }

    public async Task<ApiResult<GenerateUploadUrlResponse>> Handle(GenerateUploadUrlCommand request,
        CancellationToken cancellationToken)
    {
        var mediaPath = Guid.NewGuid();
        var mediaUrl = await _minioClient.PresignedPutObjectAsync(new PresignedPutObjectArgs()
            .WithBucket("media")
            .WithExpiry(1000)
            .WithObject(mediaPath.ToString())
        );

        var medium = new UnknownMedium()
        {
            MediaPath = mediaPath.ToString(),
        };
        await _db.UnknownMedia.AddAsync(medium, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return ApiResult.Ok(new GenerateUploadUrlResponse
        {
            Url = mediaUrl,
            MediumId = medium.MediaId,
        });
    }
}