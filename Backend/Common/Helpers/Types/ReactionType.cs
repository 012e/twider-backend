using System.ComponentModel.DataAnnotations;

namespace Backend.Common.Helpers.Types;

public enum ReactionType
{
    Like = 1,
    Love = 2,
    Haha = 3,
    Wow = 4,
    Sad = 5,
    Angry = 6,
    Care = 7
}

public static class ReactionTypeHelper
{
    public static void Validate(string value)
    {
        if (value == "like" ||
            value == "love" ||
            value == "haha" ||
            value == "wow" ||
            value == "sad" ||
            value == "angry" ||
            value == "care")
            return;
        throw new ValidationException("Invalid reaction type");
    }

    public static ReactionType ToReactionTypeEnum(this string value) => value switch
    {
        "like" => ReactionType.Like,
        "love" => ReactionType.Love,
        "haha" => ReactionType.Haha,
        "wow" => ReactionType.Wow,
        "sad" => ReactionType.Sad,
        "angry" => ReactionType.Angry,
        "care" => ReactionType.Care,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
}