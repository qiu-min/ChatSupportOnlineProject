using ChatSupportOnline.Api.Contracts.Common;
using ChatSupportOnline.Api.Contracts.Users;
using ChatSupportOnline.Api.Data;
using ChatSupportOnline.Api.Models.Entities;
using ChatSupportOnline.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatSupportOnline.Api.Controllers;

/// <summary>
/// 用户管理控制器。
/// 当前阶段用于帮助我们快速创建和查看测试用户。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordService _passwordService;

    public UsersController(AppDbContext dbContext, IPasswordService passwordService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
    }

    /// <summary>
    /// 获取所有用户。
    /// 这里加上授权特性，用来演示 JWT 生效后如何保护接口。
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<User>>>> GetUsers()
    {
        var users = await _dbContext.Users
            .OrderBy(user => user.CreatedAtUtc)
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<User>>.Ok(users));
    }

    /// <summary>
    /// 根据 Id 获取单个用户。
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<User>>> GetUser(Guid id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound(ApiResponse<User>.Fail("未找到对应用户。"));
        }

        return Ok(ApiResponse<User>.Ok(user));
    }

    /// <summary>
    /// 创建用户。
    /// 当前保留这个接口用于学习阶段造数，但内部已经使用密码哈希服务。
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<User>>> CreateUser([FromBody] CreateUserRequest request)
    {
        var userName = request.UserName.Trim();
        var displayName = request.DisplayName.Trim();
        var password = request.Password.Trim();

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(displayName) ||
            string.IsNullOrWhiteSpace(password))
        {
            return BadRequest(ApiResponse<User>.Fail("用户名、显示名和密码不能为空。"));
        }

        var exists = await _dbContext.Users.AnyAsync(user => user.UserName == userName);
        if (exists)
        {
            return Conflict(ApiResponse<User>.Fail("用户名已存在。"));
        }

        var user = new User
        {
            UserName = userName,
            DisplayName = displayName,
            PasswordHash = _passwordService.HashPassword(password),
            Role = request.Role
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id },
            ApiResponse<User>.Ok(user, "用户创建成功。"));
    }
}
