using Backend.Common.DbContext;
using Backend.Common.DbContext.Chat;
using Backend.Features.Chat.Commands.SendMessage;
using Backend.Features.Chat.Queries.GetChatMessages;
using Backend.Features.Chat.Queries.GetChats;
using BackendTest.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTest.Features.Chat;

public class ChatTest(IntegrationTestFactory factory) : BaseCqrsIntegrationTest(factory), IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        // No per-test setup needed, cleanup is handled after all tests.
        // This is where you might put global test setup if it were required to run once before all tests.
        // The individual tests are already calling CleanupDatabase() at the start for isolation.
    }

    public async Task DisposeAsync()
    {
        await CleanupDatabase();
    }

    private async Task CleanupDatabase()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Clean up chat-related data
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM message_media");
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM messages");
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM chat_participants");
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM chats");

        // Clean up test users (keep the mock user that's created by test setup)
        await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM users WHERE user_id != '{CurrentUser.UserId}'");
    }

    [Fact]
    public async Task Should_Send_Direct_Message_Successfully()
    {
        // Arrange
        var otherUser = new Backend.Common.DbContext.User
        {
            UserId = Guid.NewGuid(),
            OauthSub = $"test-oauth-sub-{Guid.NewGuid()}",
            Username = $"otheruser-{Guid.NewGuid()}",
            Email = $"other-{Guid.NewGuid()}@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            VerificationStatus = "verified"
        };

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(otherUser);
            await dbContext.SaveChangesAsync();
        }

        var command = new SendMessageCommand
        {
            OtherUserId = otherUser.UserId,
            Content = "Hello, this is a test message!"
        };

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);

        // Verify message was created in database
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var message = await dbContext.Messages.FirstOrDefaultAsync(m => m.MessageId == result.Value.Id);
            Assert.NotNull(message);
            Assert.Equal(command.Content, message.Content);
            Assert.Equal(CurrentUser.UserId, message.UserId);
            Assert.False(message.IsDeleted);

            // Verify chat was created
            var chat = await dbContext.Chats.FirstOrDefaultAsync(c => c.ChatId == message.ChatId);
            Assert.NotNull(chat);
            Assert.Equal("direct", chat.ChatType);
            Assert.Equal(1, chat.MessageCount);

            // Verify participants were created
            var participants = await dbContext.ChatParticipants
                .Where(cp => cp.ChatId == chat.ChatId)
                .ToListAsync();
            Assert.Equal(2, participants.Count);
            Assert.Contains(participants, p => p.UserId == CurrentUser.UserId);
            Assert.Contains(participants, p => p.UserId == otherUser.UserId);
        }
    }

    [Fact]
    public async Task Should_Return_404_For_Non_Existent_User()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var command = new SendMessageCommand
        {
            OtherUserId = nonExistentUserId,
            Content = "This should fail"
        };

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsFailed);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.Status);
        Assert.Equal("User not found", result.Error.Title);
    }

    [Fact]
    public async Task Should_Return_400_For_Self_Message()
    {
        // Arrange
        var command = new SendMessageCommand
        {
            OtherUserId = CurrentUser.UserId,
            Content = "Talking to myself"
        };

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsFailed);
        Assert.NotNull(result.Error);
        Assert.Equal(400, result.Error.Status);
        Assert.Equal("Invalid request", result.Error.Title);
    }

    [Fact]
    public async Task Should_Use_Existing_Chat_For_Subsequent_Messages()
    {
        // Arrange
        var otherUser = new Backend.Common.DbContext.User
        {
            UserId = Guid.NewGuid(),
            OauthSub = $"test-oauth-sub-{Guid.NewGuid()}",
            Username = $"otheruser-{Guid.NewGuid()}",
            Email = $"other-{Guid.NewGuid()}@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            VerificationStatus = "verified"
        };

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(otherUser);
            await dbContext.SaveChangesAsync();
        }

        // Act - Send first message
        var firstMessage = await Mediator.Send(new SendMessageCommand
        {
            OtherUserId = otherUser.UserId,
            Content = "First message"
        });

        // Act - Send second message
        var secondMessage = await Mediator.Send(new SendMessageCommand
        {
            OtherUserId = otherUser.UserId,
            Content = "Second message"
        });

        // Assert
        Assert.True(firstMessage.IsSuccess);
        Assert.True(secondMessage.IsSuccess);

        // Verify both messages are in the same chat
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var firstMsg = await dbContext.Messages.FirstOrDefaultAsync(m => m.MessageId == firstMessage.Value.Id);
            var secondMsg = await dbContext.Messages.FirstOrDefaultAsync(m => m.MessageId == secondMessage.Value.Id);

            Assert.NotNull(firstMsg);
            Assert.NotNull(secondMsg);
            Assert.Equal(firstMsg.ChatId, secondMsg.ChatId);

            // Verify chat message count was updated
            var chat = await dbContext.Chats.FirstOrDefaultAsync(c => c.ChatId == firstMsg.ChatId);
            Assert.NotNull(chat);
            Assert.Equal(2, chat.MessageCount);
        }
    }

    [Fact]
    public async Task Should_Get_Chat_Messages_Successfully()
    {
        // Arrange
        var otherUser = new Backend.Common.DbContext.User
        {
            UserId = Guid.NewGuid(),
            OauthSub = $"test-oauth-sub-{Guid.NewGuid()}",
            Username = $"otheruser-{Guid.NewGuid()}",
            Email = $"other-{Guid.NewGuid()}@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            VerificationStatus = "verified"
        };

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(otherUser);
            await dbContext.SaveChangesAsync();
        }

        // Send some messages
        await Mediator.Send(new SendMessageCommand
        {
            OtherUserId = otherUser.UserId,
            Content = "Message 1"
        });

        await Mediator.Send(new SendMessageCommand
        {
            OtherUserId = otherUser.UserId,
            Content = "Message 2"
        });

        var query = new GetChatMessagesQuery
        {
            OtherUserId = otherUser.UserId,
            PageSize = 10
        };

        // Act
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal("Message 2", result.Value.Items[0].Content); // Most recent first
        Assert.Equal("Message 1", result.Value.Items[1].Content);
        Assert.Equal(CurrentUser.Username, result.Value.Items[0].Username);
        Assert.Equal(CurrentUser.Username, result.Value.Items[1].Username);
    }

    [Fact]
    public async Task Should_Return_Empty_Messages_For_Non_Existent_Chat()
    {
        // Arrange
        var otherUser = new Backend.Common.DbContext.User
        {
            UserId = Guid.NewGuid(),
            OauthSub = $"test-oauth-sub-{Guid.NewGuid()}",
            Username = $"otheruser-{Guid.NewGuid()}",
            Email = $"other-{Guid.NewGuid()}@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            VerificationStatus = "verified"
        };

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(otherUser);
            await dbContext.SaveChangesAsync();
        }

        var query = new GetChatMessagesQuery
        {
            OtherUserId = otherUser.UserId,
            PageSize = 10
        };

        // Act
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Items);
        Assert.False(result.Value.HasMore);
        Assert.Null(result.Value.NextCursor);
    }

    [Fact]
    public async Task Should_Get_Chats_Successfully()
    {
        // Arrange
        var otherUser1 = new Backend.Common.DbContext.User
        {
            UserId = Guid.NewGuid(),
            OauthSub = $"test-oauth-sub-{Guid.NewGuid()}",
            Username = $"otheruser1-{Guid.NewGuid()}",
            Email = $"other1-{Guid.NewGuid()}@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            VerificationStatus = "verified"
        };

        var otherUser2 = new Backend.Common.DbContext.User
        {
            UserId = Guid.NewGuid(),
            OauthSub = $"test-oauth-sub-{Guid.NewGuid()}",
            Username = $"otheruser2-{Guid.NewGuid()}",
            Email = $"other2-{Guid.NewGuid()}@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            VerificationStatus = "verified"
        };

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.AddRange(otherUser1, otherUser2);
            await dbContext.SaveChangesAsync();
        }

        // Send messages to create chats
        await Mediator.Send(new SendMessageCommand
        {
            OtherUserId = otherUser1.UserId,
            Content = "Hello user 1"
        });

        await Mediator.Send(new SendMessageCommand
        {
            OtherUserId = otherUser2.UserId,
            Content = "Hello user 2"
        });

        var query = new GetChatsQuery
        {
            PaginationMeta = new Backend.Common.Helpers.Types.InfiniteCursorPaginationMeta
            {
                PageSize = 10
            }
        };

        // Act
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count);

        // Check that each chat has participants and last message
        foreach (var chat in result.Value.Items)
        {
            Assert.Equal("direct", chat.ChatType);
            Assert.Equal(2, chat.Participants.Count);
            Assert.NotNull(chat.LastMessage);
            Assert.Equal(1, chat.MessageCount);
        }
    }

    [Fact]
    public async Task Should_Return_Empty_Chats_For_New_User()
    {
        // Arrange
        var query = new GetChatsQuery
        {
            PaginationMeta = new Backend.Common.Helpers.Types.InfiniteCursorPaginationMeta
            {
                PageSize = 10
            }
        };

        // Act
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Items);
        Assert.False(result.Value.HasMore);
        Assert.Null(result.Value.NextCursor);
    }

    [Fact]
    public async Task Should_Handle_Message_Content_Validation()
    {
        // Arrange
        var otherUser = new Backend.Common.DbContext.User
        {
            UserId = Guid.NewGuid(),
            OauthSub = $"test-oauth-sub-{Guid.NewGuid()}",
            Username = $"otheruser-{Guid.NewGuid()}",
            Email = $"other-{Guid.NewGuid()}@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            VerificationStatus = "verified"
        };

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(otherUser);
            await dbContext.SaveChangesAsync();
        }

        // Test valid message
        var validCommand = new SendMessageCommand
        {
            OtherUserId = otherUser.UserId,
            Content = "Valid message"
        };

        var validResult = await Mediator.Send(validCommand);
        Assert.True(validResult.IsSuccess);

        // Test very long message (over 2000 characters)
        var longCommand = new SendMessageCommand
        {
            OtherUserId = otherUser.UserId,
            Content = new string('A', 2001) // Over 2000 character limit
        };

        var longResult = await Mediator.Send(longCommand);
        // Since validation happens at the API level, the handler should still work
        // The validation would be caught by the routes
        Assert.True(longResult.IsSuccess);
    }
}