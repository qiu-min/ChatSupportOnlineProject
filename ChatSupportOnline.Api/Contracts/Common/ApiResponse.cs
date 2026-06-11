namespace ChatSupportOnline.Api.Contracts.Common;

/// <summary>
/// 统一接口返回结构。
/// 这样前端接数据时只需要围绕一个固定格式做处理。
/// </summary>
/// <typeparam name="T">业务数据类型。</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功。
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// 返回给调用方的说明信息。
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 业务数据载荷。
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// 创建成功响应。
    /// </summary>
    public static ApiResponse<T> Ok(T data, string message = "OK") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    /// <summary>
    /// 创建失败响应。
    /// </summary>
    public static ApiResponse<T> Fail(string message) =>
        new()
        {
            Success = false,
            Message = message
        };
}
