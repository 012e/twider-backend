using MediatR;

namespace Backend.Features.Media.Commands;

public class GenerateUploadUrlCommand : IRequest<ApiResult<GenerateUploadUrlResponse>>;