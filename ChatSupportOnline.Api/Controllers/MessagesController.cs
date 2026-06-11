using ChatSupportOnline.Api.Contracts.Common;
using ChatSupportOnline.Api.Contracts.Messages;
using ChatSupportOnline.Api.Data;
using ChatSupportOnline.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatSupportOnline.Api.Controllers;

/// <summary>
/// 消息控制器。
/// 当前通过 HTTP 接口完成消息入库和历史查询，后续会由 SignalR 承担实时推送。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public MessagesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 根据会话 Id 获取历史消息。
    /// </summary>
    [HttpGet("conversation/{conversationId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Message>>>> GetConversationMessages(Guid conversationId)
    {
        var conversationExists = await _dbContext.Conversations.AnyAsync(conversation => conversation.Id == conversationId);
        if (!conversationExists)
        {
            return NotFound(ApiResponse<IEnumerable<Message>>.Fail("会话不存在。"));
        }

        var messages = await _dbContext.Messages
            .AsNoTracking()
            .Include(message => message.Sender)
            .Where(message => message.ConversationId == conversationId)
            .OrderBy(message => message.SentAtUtc)
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<Message>>.Ok(messages));
    }

    /// <summary>
    /// 创建一条新消息，并同步更新会话的最近活跃时间。
    /// 这个更新时间对于后续会话列表排序非常有用。
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Message>>> CreateMessage([FromBody] CreateMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(ApiResponse<Message>.Fail("消息内容不能为空。"));
        }

        var conversation = await _dbContext.Conversations.FindAsync(request.ConversationId);
        if (conversation is null)
        {
            return BadRequest(ApiResponse<Message>.Fail("会话不存在。"));
        }

        var sender = await _dbContext.Users.FindAsync(request.SenderUserId);
        if (sender is null)
        {
            return BadRequest(ApiResponse<Message>.Fail("发送者不存在。"));
        }

        var message = new Message
        {
            ConversationId = request.ConversationId,
            SenderUserId = request.SenderUserId,
            Content = request.Content.Trim()
        };

        conversation.UpdatedAtUtc = DateTime.UtcNow;

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();

        return Created(
            $"/api/messages/{message.Id}",
            ApiResponse<Message>.Ok(message, "消息发送成功。"));
    }
}
