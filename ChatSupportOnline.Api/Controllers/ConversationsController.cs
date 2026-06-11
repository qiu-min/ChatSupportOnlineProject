using ChatSupportOnline.Api.Contracts.Common;
using ChatSupportOnline.Api.Contracts.Conversations;
using ChatSupportOnline.Api.Data;
using ChatSupportOnline.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatSupportOnline.Api.Controllers;

/// <summary>
/// 会话管理控制器。
/// 负责创建会话、查询会话列表，是客服系统中的核心业务入口之一。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ConversationsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 获取会话列表，并带出客户、客服和消息数量等基础信息。
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetConversations()
    {
        var conversations = await _dbContext.Conversations
            .AsNoTracking()
            .Include(conversation => conversation.Customer)
            .Include(conversation => conversation.Agent)
            .Include(conversation => conversation.Messages)
            .OrderByDescending(conversation => conversation.UpdatedAtUtc)
            .Select(conversation => new
            {
                conversation.Id,
                conversation.Subject,
                conversation.CustomerId,
                CustomerName = conversation.Customer != null ? conversation.Customer.DisplayName : null,
                conversation.AgentId,
                AgentName = conversation.Agent != null ? conversation.Agent.DisplayName : null,
                conversation.CreatedAtUtc,
                conversation.UpdatedAtUtc,
                MessageCount = conversation.Messages.Count
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<object>>.Ok(conversations));
    }

    /// <summary>
    /// 获取单个会话详情，包含完整消息列表。
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<Conversation>>> GetConversation(Guid id)
    {
        var conversation = await _dbContext.Conversations
            .Include(item => item.Customer)
            .Include(item => item.Agent)
            .Include(item => item.Messages.OrderBy(message => message.SentAtUtc))
            .ThenInclude(message => message.Sender)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (conversation is null)
        {
            return NotFound(ApiResponse<Conversation>.Fail("未找到对应会话。"));
        }

        return Ok(ApiResponse<Conversation>.Ok(conversation));
    }

    /// <summary>
    /// 创建会话。
    /// 创建前会校验客户和客服是否真实存在，避免脏数据进入数据库。
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Conversation>>> CreateConversation([FromBody] CreateConversationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            return BadRequest(ApiResponse<Conversation>.Fail("会话主题不能为空。"));
        }

        var customer = await _dbContext.Users.FindAsync(request.CustomerId);
        if (customer is null)
        {
            return BadRequest(ApiResponse<Conversation>.Fail("客户不存在。"));
        }

        if (request.AgentId.HasValue)
        {
            var agentExists = await _dbContext.Users.AnyAsync(user => user.Id == request.AgentId.Value);
            if (!agentExists)
            {
                return BadRequest(ApiResponse<Conversation>.Fail("客服不存在。"));
            }
        }

        var conversation = new Conversation
        {
            Subject = request.Subject.Trim(),
            CustomerId = request.CustomerId,
            AgentId = request.AgentId
        };

        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetConversation),
            new { id = conversation.Id },
            ApiResponse<Conversation>.Ok(conversation, "会话创建成功。"));
    }
}
