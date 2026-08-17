using System.IO;
using WpfTemplate.Api;
using WpfTemplate.Models;
using WpfTemplate.Services.Abstractions;

namespace WpfTemplate.Services.Files;

public sealed class FileService : IFileService
{
    private readonly FileApi _fileApi;

    public FileService(FileApi fileApi)
    {
        _fileApi = fileApi;
    }

    public async Task<SysFileItem> UploadAsync(string filePath, string? storageProvider = null, CancellationToken cancellationToken = default)
    {
        await using var stream = System.IO.File.OpenRead(filePath);
        var fileName = Path.GetFileName(filePath);
        return await _fileApi.UploadAsync(stream, fileName, storageProvider, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> DownloadToFileAsync(string fileId, string destinationPath, CancellationToken cancellationToken = default)
    {
        var (content, remoteName) = await _fileApi.DownloadAsync(fileId, cancellationToken).ConfigureAwait(false);
        var path = destinationPath;
        if (Directory.Exists(path) || path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar))
        {
            var name = string.IsNullOrWhiteSpace(remoteName) ? fileId : remoteName;
            path = Path.Combine(path, name);
        }

        await SaveBytesAsync(content, path, cancellationToken).ConfigureAwait(false);
        return path;
    }

    public async Task SaveBytesAsync(byte[] content, string destinationPath, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await System.IO.File.WriteAllBytesAsync(destinationPath, content, cancellationToken).ConfigureAwait(false);
    }
}
