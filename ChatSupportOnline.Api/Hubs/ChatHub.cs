using ChatSupportOnline.Api.Data;
using ChatSupportOnline.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatSupportOnline.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _dbContext;

    public ChatHub(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private Guid GetCurrentUserId()
    {
        var userIdText = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdText, out var userId))
        {
            throw new HubException("无法识别当前登录用户。");
        }

        return userId;
    }

    private static string ConversationGroup(Guid conversationId) => $"conversation:{conversationId}";

    public async Task JoinConversation(Guid conversationId)
    {
        var exists = await _dbContext.Conversations
            .AnyAsync(item => item.Id == conversationId);

        if (!exists)
        {
            throw new HubException("会话不存在。");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        var senderUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new HubException("消息内容不能为空。");
        }

        var conversation = await _dbContext.Conversations.FindAsync(conversationId);
        if (conversation is null)
        {
            throw new HubException("会话不存在。");
        }

        var message = new Message
        {
            ConversationId = conversationId,
            SenderUserId = senderUserId,
            Content = content.Trim()
        };

        conversation.UpdatedAtUtc = DateTime.UtcNow;

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();

        await Clients.Group(ConversationGroup(conversationId)).SendAsync("MessageReceived", new
        {
            message.Id,
            message.ConversationId,
            message.SenderUserId,
            message.Content,
            message.SentAtUtc
        });
    }
}