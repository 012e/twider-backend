using Backend.Common.Helpers.Types;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Comment.Queries;

public record GetCommentsByPostQuery(Guid PostId, Guid? CommentId, InfiniteCursorPaginationMeta Meta) : IRequest<ApiResult<InfiniteCursorPage<CommentDto>>>;