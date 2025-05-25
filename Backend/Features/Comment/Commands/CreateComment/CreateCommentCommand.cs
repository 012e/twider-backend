using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Types;
using MediatR;

namespace Backend.Features.Comment.Commands.CreateComment;

public class CreateCommentCommand : IRequest<ApiResult<ItemId>>
{
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }

    public CommentContent Content { get; set; } = null!;
}

public class CommentContent
{
    [Required(AllowEmptyStrings = false)]
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string Content { get; set; } = null!;
}