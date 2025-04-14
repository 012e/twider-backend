namespace Backend.Common.DbContext;

public partial class UserSetting
{
    public int UserId { get; set; }

    public bool NotificationsEnabled { get; set; }

    public bool PrivateAccount { get; set; }

    public string? ThemePreference { get; set; }

    public string? LanguagePreference { get; set; }

    public virtual User User { get; set; } = null!;
}
