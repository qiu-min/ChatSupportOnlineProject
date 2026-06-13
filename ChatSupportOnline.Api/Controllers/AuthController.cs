using ChatSupportOnline.Api.Contracts.Auth;
using ChatSupportOnline.Api.Contracts.Common;
using ChatSupportOnline.Api.Data;
using ChatSupportOnline.Api.Models.Entities;
using ChatSupportOnline.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatSupportOnline.Api.Controllers;

/// <summary>
/// 认证控制器。
/// 负责注册、登录以及读取当前登录用户信息，是 JWT 学习阶段的核心入口。
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        AppDbContext dbContext,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// 注册新用户。
    /// 注册成功后会直接返回 JWT，方便前端完成“注册即登录”的体验。
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var userName = request.UserName.Trim();
        var displayName = request.DisplayName.Trim();
        var password = request.Password.Trim();

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(displayName) ||
            string.IsNullOrWhiteSpace(password))
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("用户名、显示名和密码不能为空。"));
        }

        var exists = await _dbContext.Users.AnyAsync(user => user.UserName == userName);
        if (exists)
        {
            return Conflict(ApiResponse<AuthResponse>.Fail("用户名已存在。"));
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

        var authResponse = _jwtTokenService.CreateAuthResponse(user);
        return Ok(ApiResponse<AuthResponse>.Ok(authResponse, "注册成功。"));
    }

    /// <summary>
    /// 用户登录。
    /// 先校验用户名是否存在，再校验密码哈希，全部通过后返回 JWT。
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var userName = request.UserName.Trim();
        var password = request.Password.Trim();

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("用户名和密码不能为空。"));
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.UserName == userName);
        if (user is null || !_passwordService.VerifyPassword(password, user.PasswordHash))
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("用户名或密码错误。"));
        }

        if (!user.IsActive)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("用户已被禁用。"));
        }

        var authResponse = _jwtTokenService.CreateAuthResponse(user);
        return Ok(ApiResponse<AuthResponse>.Ok(authResponse, "登录成功。"));
    }

    /// <summary>
    /// 获取当前登录用户信息。
    /// 这个接口可以帮助你观察 JWT 里的 Claim 最终是如何映射回业务用户的。
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<CurrentUserResponse>>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<CurrentUserResponse>.Fail("无法识别当前登录用户。"));
        }

        var user = await _dbContext.Users.FindAsync(userId);
        if (user is null)
        {
            return NotFound(ApiResponse<CurrentUserResponse>.Fail("当前用户不存在。"));
        }

        var response = new CurrentUserResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Role = user.Role.ToString()
        };

        return Ok(ApiResponse<CurrentUserResponse>.Ok(response));
    }
}
