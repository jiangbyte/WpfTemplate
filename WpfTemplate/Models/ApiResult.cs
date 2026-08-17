using System.Text.Json.Serialization;

namespace WpfTemplate.Models;

public sealed class ApiEnvelope<T>
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public sealed class ApiResponseException : Exception
{
    public ApiResponseException(string code, string? message, object? data = null)
        : base(message ?? $"请求失败，错误码 {code}")
    {
        ApiCode = code;
        ApiData = data;
    }

    public string ApiCode { get; }

    public object? ApiData { get; }
}
