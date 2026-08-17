using WpfTemplate.Api;
using WpfTemplate.Models;
using WpfTemplate.Services.Abstractions;

namespace WpfTemplate.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AuthApi _authApi;
    private readonly ISessionStore _session;

    public AuthService(AuthApi authApi, ISessionStore session)
    {
        _authApi = authApi;
        _session = session;
        _session.SessionCleared += (_, _) => AuthenticationLost?.Invoke(this, EventArgs.Empty);
    }

    public UserInfo? CurrentUser => _session.User;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_session.Token) && _session.User is not null;

    public event EventHandler? AuthenticationLost;

    public async Task<bool> TryRestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_session.Token))
        {
            return false;
        }

        try
        {
            var me = await _authApi.MeAsync(cancellationToken).ConfigureAwait(false);
            _session.SaveUser(me);
            return true;
        }
        catch
        {
            _session.Clear(notify: false);
            return false;
        }
    }

    public async Task LoginAsync(
        string account,
        string password,
        string captchaId,
        string captchaValue,
        string identityType = "ACCOUNT",
        string loginMode = "PASSWORD",
        string? otpCode = null,
        bool rememberMe = true,
        CancellationToken cancellationToken = default)
    {
        string? encryptedPassword = null;
        string? passwordKeyId = null;
        if (string.Equals(loginMode, "PASSWORD", StringComparison.OrdinalIgnoreCase))
        {
            var key = await _authApi.GetPasswordKeyAsync(cancellationToken).ConfigureAwait(false);
            encryptedPassword = PasswordCrypto.EncryptRsaOaepSha256(key.PublicKey, password);
            passwordKeyId = key.KeyId;
        }

        var response = await _authApi.LoginAsync(new LoginRequest
        {
            Account = account.Trim(),
            Password = encryptedPassword,
            IdentityType = identityType,
            RememberMe = rememberMe,
            PasswordKeyId = passwordKeyId,
            CaptchaId = captchaId,
            CaptchaValue = captchaValue.Trim(),
            LoginMode = loginMode,
            OtpCode = string.IsNullOrWhiteSpace(otpCode) ? null : otpCode.Trim(),
        }, cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(response.Token))
        {
            _session.SaveToken(response.Token!, rememberMe);
        }

        var me = await _authApi.MeAsync(cancellationToken).ConfigureAwait(false);
        _session.SaveUser(me);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_session.Token))
            {
                await _authApi.LogoutAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
            // Best-effort logout; always clear local session.
        }
        finally
        {
            _session.Clear(notify: false);
        }
    }
}
