using Backend.Common.DbContext;
using Backend.Common.DbContext.Reaction;
using Backend.Common.Helpers.Types;
using Backend.Features.Post.Queries;
using Backend.Features.Post.Queries.GetPostById;

namespace Backend.Features.Post.Mappers;

public static class ReactionMapper
{
    public static GetPostByIdResponse.ReactionDto ExtractReactionCount(this ICollection<PostReaction> reactions)
    {
        return new GetPostByIdResponse.ReactionDto
        {
            Like = reactions.Count(r => r.ReactionType == ReactionType.Like),
            Love = reactions.Count(r => r.ReactionType == ReactionType.Love),
            Haha = reactions.Count(r => r.ReactionType == ReactionType.Haha),
            Wow = reactions.Count(r => r.ReactionType == ReactionType.Wow),
            Sad = reactions.Count(r => r.ReactionType == ReactionType.Sad),
            Angry = reactions.Count(r => r.ReactionType == ReactionType.Angry),
            Care = reactions.Count(r => r.ReactionType == ReactionType.Care)
        };
    }

}