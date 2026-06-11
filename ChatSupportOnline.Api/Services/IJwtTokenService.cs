using ChatSupportOnline.Api.Contracts.Auth;
using ChatSupportOnline.Api.Models.Entities;

namespace ChatSupportOnline.Api.Services;

/// <summary>
/// JWT 令牌服务接口。
/// 专门负责把用户信息转换成 JWT 及相关响应数据。
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// 为指定用户生成认证响应。
    /// </summary>
    AuthResponse CreateAuthResponse(User user);
}
