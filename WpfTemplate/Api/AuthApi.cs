using WpfTemplate.Models;
using WpfTemplate.Services.Abstractions;

namespace WpfTemplate.Api;

/// <summary>
/// 对齐 web/portal <c>src/api/auth/index.ts</c> 与后端 PortalAuthController。
/// 路径写全量，不经通用 ApiPrefix 拼接。
/// </summary>
public sealed class AuthApi
{
    private readonly IApiClient _api;

    public AuthApi(IApiClient api)
    {
        _api = api;
    }

    /// <summary>GET /api/v1/portal/public/auth-options</summary>
    public Task<AuthOptionsResponse> GetAuthOptionsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<AuthOptionsResponse>("/api/v1/portal/public/auth-options", isPublic: true, cancellationToken);

    /// <summary>GET /api/v1/portal/captcha — WPF 使用 format=png（后端官方参数，默认为 svg）。</summary>
    public Task<CaptchaResponse> GetCaptchaAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<CaptchaResponse>("/api/v1/portal/captcha?format=png", isPublic: true, cancellationToken);

    /// <summary>GET /api/v1/portal/password-key</summary>
    public Task<PasswordKeyResponse> GetPasswordKeyAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<PasswordKeyResponse>("/api/v1/portal/password-key", isPublic: true, cancellationToken);

    /// <summary>POST /api/v1/portal/send-login-code</summary>
    public Task SendLoginCodeAsync(SendLoginCodeRequest request, CancellationToken cancellationToken = default)
        => _api.PostAsync<object?>("/api/v1/portal/send-login-code", request, isPublic: true, cancellationToken);

    /// <summary>POST /api/v1/portal/register/send-code</summary>
    public Task SendRegisterCodeAsync(SendLoginCodeRequest request, CancellationToken cancellationToken = default)
        => _api.PostAsync<object?>("/api/v1/portal/register/send-code", request, isPublic: true, cancellationToken);

    /// <summary>POST /api/v1/portal/register</summary>
    public Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        => _api.PostAsync<object?>("/api/v1/portal/register", request, isPublic: true, cancellationToken);

    /// <summary>POST /api/v1/portal/forgot-password</summary>
    public Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
        => _api.PostAsync<object?>("/api/v1/portal/forgot-password", request, isPublic: true, cancellationToken);

    /// <summary>POST /api/v1/portal/reset-password</summary>
    public Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        => _api.PostAsync<object?>("/api/v1/portal/reset-password", request, isPublic: true, cancellationToken);

    /// <summary>POST /api/v1/portal/login</summary>
    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        => _api.PostAsync<LoginResponse>("/api/v1/portal/login", request, isPublic: true, cancellationToken);

    /// <summary>POST /api/v1/portal/logout</summary>
    public Task LogoutAsync(CancellationToken cancellationToken = default)
        => _api.PostAsync<object?>("/api/v1/portal/logout", null, isPublic: true, cancellationToken);

    /// <summary>GET /api/v1/portal/me</summary>
    public Task<UserInfo> MeAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<UserInfo>("/api/v1/portal/me", cancellationToken: cancellationToken);
}
