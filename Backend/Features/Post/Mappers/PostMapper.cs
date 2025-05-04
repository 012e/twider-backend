using Backend.Features.Post.Queries;
using Riok.Mapperly.Abstractions;

namespace Backend.Features.Post.Mappers;

[Mapper]
public static partial class PostMapper
{
    public static partial GetPostByIdResponse ToResponse(this Common.DbContext.Post post);
}