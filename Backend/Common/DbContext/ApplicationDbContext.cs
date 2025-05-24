using Backend.Common.DbContext.Chat;
using Backend.Common.DbContext.Configurations;
using Backend.Common.DbContext.Post;
using Backend.Common.DbContext.Reaction;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Backend.Common.DbContext;

public partial class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chat.Chat> Chats { get; set; }

    public virtual DbSet<ChatParticipant> ChatParticipants { get; set; }
    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Follow> Follows { get; set; }

    public virtual DbSet<Hashtag> Hashtags { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<Medium> Media { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<MessageMedium> MessageMedia { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Post.Post> Posts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSetting> UserSettings { get; set; }

    public virtual DbSet<UnknownMedium> UnknownMedia { get; set; }
    public virtual DbSet<PostMedium> PostMedia { get; set; }
    public virtual DbSet<CommentMedium> CommentMedia { get; set; }

    public virtual DbSet<PostReaction> PostReactions { get; set; }

    public virtual DbSet<CommentReaction> CommentReactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}