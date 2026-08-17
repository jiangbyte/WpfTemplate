using System.IO;
using System.Net.Http;
using WpfTemplate.Models;
using WpfTemplate.Services.Abstractions;

namespace WpfTemplate.Api;

/// <summary>
/// 对齐 web/portal <c>src/api/sys/file.ts</c> 与后端 PortalFileController。
/// 路径写全量，不经通用 ApiPrefix 拼接。
/// </summary>
public sealed class FileApi
{
    private readonly IApiClient _api;

    public FileApi(IApiClient api)
    {
        _api = api;
    }

    /// <summary>POST /api/v1/portal/sys/file/upload</summary>
    public async Task<SysFileItem> UploadAsync(Stream stream, string fileName, string? storageProvider = null, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(stream);
        content.Add(streamContent, "file", fileName);
        if (!string.IsNullOrWhiteSpace(storageProvider))
        {
            content.Add(new StringContent(storageProvider), "storage_provider");
        }

        return await _api.PostMultipartAsync<SysFileItem>("/api/v1/portal/sys/file/upload", content, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>GET /api/v1/portal/sys/file/download?id=</summary>
    public Task<(byte[] Content, string? FileName)> DownloadAsync(string id, CancellationToken cancellationToken = default)
        => _api.DownloadAsync($"/api/v1/portal/sys/file/download?id={Uri.EscapeDataString(id)}", cancellationToken);

    /// <summary>POST /api/v1/portal/sys/file/url</summary>
    public Task<FileUrlResponse> GetUrlAsync(string objectName, CancellationToken cancellationToken = default)
        => _api.PostAsync<FileUrlResponse>("/api/v1/portal/sys/file/url", new { object_name = objectName }, cancellationToken: cancellationToken);
}
