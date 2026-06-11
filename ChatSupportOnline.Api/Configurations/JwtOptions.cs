namespace ChatSupportOnline.Api.Configurations;

/// <summary>
/// JWT 配置模型。
/// 它负责承接 appsettings.json 中的 Jwt 节点，避免把配置字符串散落在代码各处。
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// 配置节名称。
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Token 颁发者。
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token 面向的接收方。
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// 对称签名密钥。
    /// 实际生产环境应该放到安全配置中心或环境变量，而不是直接写进仓库。
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Token 有效期，单位为分钟。
    /// </summary>
    public int ExpiresMinutes { get; set; } = 120;
}
