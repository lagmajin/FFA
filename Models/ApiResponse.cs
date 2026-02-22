namespace FFA.Models;

/// <summary>
/// API統一レスポンスラッパー
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public static ApiResponse<T> Ok(T data) => new()
    {
        Success = true,
        Data = data
    };
    
    public static ApiResponse<T> Fail(string error, int code = 400) => new()
    {
        Success = false,
        ErrorMessage = error,
        ErrorCode = code
    };
}

/// <summary>
/// ページネーションレスポンス
/// </summary>
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => PageIndex < TotalPages;
    public bool HasPreviousPage => PageIndex > 0;
}

/// <summary>
/// 一般的なリスト結果
/// </summary>
public class ListResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Count => Items.Count;
}

/// <summary>
/// エラーコード
/// </summary>
public static class ApiErrorCodes
{
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int Conflict = 409;
    public const int InternalError = 500;
    public const int ServiceUnavailable = 503;
}
