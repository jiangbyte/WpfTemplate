using System.Net.Http;
using WpfTemplate.Models;

namespace WpfTemplate.Services.Abstractions;

public interface IApiClient
{
    Task<T> GetAsync<T>(string path, bool isPublic = false, CancellationToken cancellationToken = default);

    Task<T> PostAsync<T>(string path, object? body = null, bool isPublic = false, CancellationToken cancellationToken = default);

    Task<T> PostMultipartAsync<T>(string path, MultipartFormDataContent content, CancellationToken cancellationToken = default);

    Task<(byte[] Content, string? FileName)> DownloadAsync(string path, CancellationToken cancellationToken = default);
}

public interface ISessionStore
{
    string? Token { get; }

    UserInfo? User { get; }

    void SaveToken(string token, bool persist);

    void SaveUser(UserInfo user);

    void Clear(bool notify = true);

    event EventHandler? SessionCleared;
}

public interface IAuthService
{
    UserInfo? CurrentUser { get; }

    bool IsAuthenticated { get; }

    Task<bool> TryRestoreSessionAsync(CancellationToken cancellationToken = default);

    Task LoginAsync(
        string account,
        string password,
        string captchaId,
        string captchaValue,
        string identityType = "ACCOUNT",
        string loginMode = "PASSWORD",
        string? otpCode = null,
        bool rememberMe = true,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    event EventHandler? AuthenticationLost;
}

public interface INavigationService
{
    IReadOnlyList<MenuItemModel> Menus { get; }

    MenuItemModel? SelectedMenu { get; }

    object? CurrentPage { get; }

    void Navigate(string key);

    event EventHandler? Navigated;
}

public interface IFileService
{
    Task<SysFileItem> UploadAsync(string filePath, string? storageProvider = null, CancellationToken cancellationToken = default);

    Task<string> DownloadToFileAsync(string fileId, string destinationPath, CancellationToken cancellationToken = default);

    Task SaveBytesAsync(byte[] content, string destinationPath, CancellationToken cancellationToken = default);
}

public interface IAppNavigator
{
    void ShowLogin(string? statusMessage = null);

    void ShowRegister();

    void ShowForgotPassword(string? resetToken = null);

    void ShowMain();
}

public interface IUnauthorizedHandler
{
    void HandleUnauthorized();
}
