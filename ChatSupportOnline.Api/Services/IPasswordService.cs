namespace ChatSupportOnline.Api.Services;

/// <summary>
/// 密码服务接口。
/// 把密码哈希与验证逻辑抽出来，便于控制器保持简洁，也方便以后替换实现。
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// 将明文密码转换为可安全存储的哈希字符串。
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// 校验明文密码是否与已保存的哈希匹配。
    /// </summary>
    bool VerifyPassword(string password, string hashedPassword);
}
