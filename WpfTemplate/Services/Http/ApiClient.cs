using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WpfTemplate.Helpers;
using WpfTemplate.Models;
using WpfTemplate.Services.Abstractions;

namespace WpfTemplate.Services.Http;

public sealed class ApiClient : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly ISessionStore _session;
    private readonly IUnauthorizedHandler _unauthorizedHandler;

    public ApiClient(
        HttpClient http,
        ISessionStore session,
        IUnauthorizedHandler unauthorizedHandler)
    {
        _http = http;
        _session = session;
        _unauthorizedHandler = unauthorizedHandler;
    }

    public Task<T> GetAsync<T>(string path, bool isPublic = false, CancellationToken cancellationToken = default)
        => SendAsync<T>(HttpMethod.Get, path, null, isPublic, cancellationToken);

    public Task<T> PostAsync<T>(string path, object? body = null, bool isPublic = false, CancellationToken cancellationToken = default)
        => SendAsync<T>(HttpMethod.Post, path, body, isPublic, cancellationToken);

    public async Task<T> PostMultipartAsync<T>(string path, MultipartFormDataContent content, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, path, isPublic: false);
        request.Content = content;
        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return await ReadEnvelopeAsync<T>(response, isPublic: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<(byte[] Content, string? FileName)> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, path, isPublic: false);
        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _unauthorizedHandler.HandleUnauthorized();
            throw new UnauthorizedAccessException("登录已过期，请重新登录");
        }

        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        var fileName = GetFileName(response.Content.Headers.ContentDisposition);
        return (bytes, fileName);
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, bool isPublic, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, path, isPublic);
        if (body is not null)
        {
            // 对齐 web/portal request-interceptors：JSON 标量 stringify 为字符串
            var json = WireJson.Serialize(body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return await ReadEnvelopeAsync<T>(response, isPublic, cancellationToken).ConfigureAwait(false);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, bool isPublic)
    {
        // path 由各 Api 类写全量（如 /api/v1/portal/login），相对 BaseUrl 发起请求。
        var request = new HttpRequestMessage(method, path);
        if (!isPublic && !string.IsNullOrWhiteSpace(_session.Token))
        {
            request.Headers.TryAddWithoutValidation("Authorization", _session.Token);
        }

        return request;
    }

    private async Task<T> ReadEnvelopeAsync<T>(HttpResponseMessage response, bool isPublic, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized && !isPublic)
        {
            _unauthorizedHandler.HandleUnauthorized();
            throw new UnauthorizedAccessException("登录已过期，请重新登录");
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {Truncate(payload)}");
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            return default!;
        }

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("code", out var codeElement))
        {
            var code = codeElement.GetString() ?? string.Empty;
            var message = root.TryGetProperty("message", out var messageElement)
                ? messageElement.GetString()
                : null;

            if (code == "401" && !isPublic)
            {
                _unauthorizedHandler.HandleUnauthorized();
                throw new UnauthorizedAccessException(message ?? "登录已过期，请重新登录");
            }

            if (code != "200")
            {
                object? data = null;
                if (root.TryGetProperty("data", out var dataElement))
                {
                    data = dataElement.Clone();
                }

                throw new ApiResponseException(code, message, data);
            }

            if (!root.TryGetProperty("data", out var successData) || successData.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return default!;
            }

            return successData.Deserialize<T>(JsonOptions)!;
        }

        return JsonSerializer.Deserialize<T>(payload, JsonOptions)!;
    }

    private static string Truncate(string text) =>
        text.Length <= 200 ? text : text[..200] + "...";

    private static string? GetFileName(ContentDispositionHeaderValue? disposition)
    {
        if (disposition is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(disposition.FileNameStar))
        {
            return disposition.FileNameStar.Trim('"');
        }

        if (!string.IsNullOrWhiteSpace(disposition.FileName))
        {
            return disposition.FileName.Trim('"');
        }

        var raw = disposition.ToString();
        var match = Regex.Match(raw, "filename\\*?=(?:UTF-8''|\")?([^\";]+)", RegexOptions.IgnoreCase);
        return match.Success ? Uri.UnescapeDataString(match.Groups[1].Value.Trim('"')) : null;
    }
}
