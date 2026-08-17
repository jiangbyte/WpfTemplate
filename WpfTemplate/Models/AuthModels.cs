using System.Text.Json.Serialization;

namespace WpfTemplate.Models;

public sealed class CaptchaResponse
{
    [JsonPropertyName("captcha_id")]
    public string CaptchaId { get; set; } = string.Empty;

    [JsonPropertyName("image_base64")]
    public string ImageBase64 { get; set; } = string.Empty;

    [JsonPropertyName("image_type")]
    public string? ImageType { get; set; }
}

public sealed class PasswordKeyResponse
{
    [JsonPropertyName("key_id")]
    public string KeyId { get; set; } = string.Empty;

    [JsonPropertyName("public_key")]
    public string PublicKey { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    [JsonPropertyName("account")]
    public string Account { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("identity_type")]
    public string IdentityType { get; set; } = "ACCOUNT";

    [JsonPropertyName("remember_me")]
    public bool RememberMe { get; set; } = true;

    [JsonPropertyName("password_key_id")]
    public string? PasswordKeyId { get; set; }

    [JsonPropertyName("captcha_id")]
    public string? CaptchaId { get; set; }

    [JsonPropertyName("captcha_value")]
    public string? CaptchaValue { get; set; }

    [JsonPropertyName("login_mode")]
    public string LoginMode { get; set; } = "PASSWORD";

    [JsonPropertyName("otp_code")]
    public string? OtpCode { get; set; }
}

public sealed class RegisterRequest
{
    [JsonPropertyName("register_channel")]
    public string RegisterChannel { get; set; } = "ACCOUNT";

    [JsonPropertyName("account")]
    public string? Account { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("otp_code")]
    public string? OtpCode { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("password_key_id")]
    public string PasswordKeyId { get; set; } = string.Empty;

    [JsonPropertyName("captcha_id")]
    public string CaptchaId { get; set; } = string.Empty;

    [JsonPropertyName("captcha_value")]
    public string CaptchaValue { get; set; } = string.Empty;
}

public sealed class ForgotPasswordRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("captcha_id")]
    public string CaptchaId { get; set; } = string.Empty;

    [JsonPropertyName("captcha_value")]
    public string CaptchaValue { get; set; } = string.Empty;
}

public sealed class ResetPasswordRequest
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("password_key_id")]
    public string PasswordKeyId { get; set; } = string.Empty;

    [JsonPropertyName("captcha_id")]
    public string CaptchaId { get; set; } = string.Empty;

    [JsonPropertyName("captcha_value")]
    public string CaptchaValue { get; set; } = string.Empty;
}

public sealed class AuthOptionsResponse
{
    [JsonPropertyName("allow_account")]
    public object? AllowAccount { get; set; }

    [JsonPropertyName("allow_email")]
    public object? AllowEmail { get; set; }

    [JsonPropertyName("allow_phone")]
    public object? AllowPhone { get; set; }

    [JsonPropertyName("allow_otp")]
    public object? AllowOtp { get; set; }

    [JsonPropertyName("register_enabled")]
    public object? RegisterEnabled { get; set; }

    [JsonPropertyName("register_allow_account")]
    public object? RegisterAllowAccount { get; set; }

    [JsonPropertyName("register_allow_email")]
    public object? RegisterAllowEmail { get; set; }

    [JsonPropertyName("register_allow_phone")]
    public object? RegisterAllowPhone { get; set; }

    [JsonPropertyName("register_require_email")]
    public object? RegisterRequireEmail { get; set; }

    [JsonPropertyName("register_require_phone")]
    public object? RegisterRequirePhone { get; set; }

    [JsonPropertyName("copyright_text")]
    public string? CopyrightText { get; set; }
}

public sealed class SendLoginCodeRequest
{
    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = "EMAIL";

    [JsonPropertyName("captcha_id")]
    public string CaptchaId { get; set; } = string.Empty;

    [JsonPropertyName("captcha_value")]
    public string CaptchaValue { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("password_expired")]
    public object? PasswordExpired { get; set; }

    [JsonPropertyName("force_bind_email")]
    public object? ForceBindEmail { get; set; }

    [JsonPropertyName("force_bind_phone")]
    public object? ForceBindPhone { get; set; }
}

public sealed class UserInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("account")]
    public string? Account { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    public string DisplayName =>
        !string.IsNullOrWhiteSpace(Nickname) ? Nickname! :
        !string.IsNullOrWhiteSpace(Account) ? Account! :
        "用户";
}
