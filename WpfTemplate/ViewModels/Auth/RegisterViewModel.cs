using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfTemplate.Api;
using WpfTemplate.Helpers;
using WpfTemplate.Models;
using WpfTemplate.Services.Abstractions;
using WpfTemplate.Services.Auth;

namespace WpfTemplate.ViewModels.Auth;

public partial class RegisterViewModel : ObservableObject
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PhoneRegex = new(@"^1\d{10}$", RegexOptions.Compiled);

    private readonly AuthApi _authApi;
    private readonly IAppNavigator _navigator;

    public RegisterViewModel(AuthApi authApi, IAppNavigator navigator)
    {
        _authApi = authApi;
        _navigator = navigator;
        _ = InitializeAsync();
    }

    /// <summary>PASSWORD = 用户名注册；OTP = 邮箱/手机验证码注册。</summary>
    [ObservableProperty]
    private string registerMode = "PASSWORD";

    [ObservableProperty]
    private string registerChannel = "ACCOUNT";

    [ObservableProperty]
    private string account = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private string otpCode = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private string captchaId = string.Empty;

    [ObservableProperty]
    private string captchaValue = string.Empty;

    [ObservableProperty]
    private BitmapImage? captchaImage;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isSendingCode;

    [ObservableProperty]
    private int otpCooldown;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool registerEnabled = true;

    [ObservableProperty]
    private bool allowAccount = true;

    [ObservableProperty]
    private bool allowEmail = true;

    [ObservableProperty]
    private bool allowPhone;

    [ObservableProperty]
    private bool requireEmail;

    [ObservableProperty]
    private bool requirePhone;

    public bool IsClosed => !RegisterEnabled || (!AllowAccount && !AllowEmail && !AllowPhone);

    public bool IsPasswordMode => string.Equals(RegisterMode, "PASSWORD", StringComparison.OrdinalIgnoreCase);

    public bool IsOtpMode => !IsPasswordMode;

    public bool OtpAvailable => AllowEmail || AllowPhone;

    public bool ModeSwitchAvailable => AllowAccount && OtpAvailable;

    public bool IsAccountChannel => RegisterChannel == "ACCOUNT";

    public bool IsEmailChannel => RegisterChannel == "EMAIL";

    public bool IsPhoneChannel => RegisterChannel == "PHONE";

    public bool NeedsOtp => IsOtpMode;

    public string ModeSwitchLabel => IsPasswordMode ? "验证码注册" : "密码注册";

    public Visibility ClosedVisibility => IsClosed ? Visibility.Visible : Visibility.Collapsed;

    public Visibility FormVisibility => IsClosed ? Visibility.Collapsed : Visibility.Visible;

    public Visibility ModeSwitchVisibility => ModeSwitchAvailable ? Visibility.Visible : Visibility.Collapsed;

    public Visibility PasswordTabsVisibility => IsPasswordMode && AllowAccount ? Visibility.Visible : Visibility.Collapsed;

    public Visibility OtpTabsVisibility => IsOtpMode ? Visibility.Visible : Visibility.Collapsed;

    public Visibility AccountTabVisibility => AllowAccount && IsPasswordMode ? Visibility.Visible : Visibility.Collapsed;

    public Visibility EmailTabVisibility => AllowEmail && IsOtpMode ? Visibility.Visible : Visibility.Collapsed;

    public Visibility PhoneTabVisibility => AllowPhone && IsOtpMode ? Visibility.Visible : Visibility.Collapsed;

    public Visibility AccountFieldsVisibility => IsPasswordMode ? Visibility.Visible : Visibility.Collapsed;

    public Visibility EmailFieldsVisibility => IsOtpMode && IsEmailChannel ? Visibility.Visible : Visibility.Collapsed;

    public Visibility PhoneFieldsVisibility => IsOtpMode && IsPhoneChannel ? Visibility.Visible : Visibility.Collapsed;

    public Visibility RequireEmailVisibility => IsPasswordMode && RequireEmail ? Visibility.Visible : Visibility.Collapsed;

    public Visibility RequirePhoneVisibility => IsPasswordMode && RequirePhone ? Visibility.Visible : Visibility.Collapsed;

    public Visibility OtpFieldsVisibility => NeedsOtp ? Visibility.Visible : Visibility.Collapsed;

    public string SendCodeLabel => OtpCooldown > 0 ? $"{OtpCooldown}s 后重发" : "发送验证码";

    public bool CanSendCode => !IsSendingCode && OtpCooldown <= 0;

    public string IdentityLabel => RegisterChannel switch
    {
        "EMAIL" => "邮箱",
        "PHONE" => "手机号",
        _ => "用户名",
    };

    partial void OnRegisterModeChanged(string value)
    {
        if (IsPasswordMode)
        {
            if (AllowAccount)
            {
                RegisterChannel = "ACCOUNT";
            }
        }
        else if (AllowEmail)
        {
            RegisterChannel = "EMAIL";
        }
        else if (AllowPhone)
        {
            RegisterChannel = "PHONE";
        }

        NotifyUiState();
    }

    partial void OnRegisterChannelChanged(string value) => NotifyUiState();

    partial void OnRegisterEnabledChanged(bool value) => NotifyUiState();

    partial void OnAllowAccountChanged(bool value) => NotifyUiState();

    partial void OnAllowEmailChanged(bool value) => NotifyUiState();

    partial void OnAllowPhoneChanged(bool value) => NotifyUiState();

    partial void OnRequireEmailChanged(bool value) => NotifyUiState();

    partial void OnRequirePhoneChanged(bool value) => NotifyUiState();

    partial void OnOtpCooldownChanged(int value)
    {
        OnPropertyChanged(nameof(SendCodeLabel));
        OnPropertyChanged(nameof(CanSendCode));
    }

    partial void OnIsSendingCodeChanged(bool value) => OnPropertyChanged(nameof(CanSendCode));

    [RelayCommand]
    private void GoLogin() => _navigator.ShowLogin();

    [RelayCommand]
    private void ToggleRegisterMode()
    {
        if (!ModeSwitchAvailable)
        {
            return;
        }

        RegisterMode = IsPasswordMode ? "OTP" : "PASSWORD";
    }

    [RelayCommand]
    private async Task RefreshCaptchaAsync()
    {
        try
        {
            var captcha = await _authApi.GetCaptchaAsync().ConfigureAwait(true);
            CaptchaId = captcha.CaptchaId;
            CaptchaValue = string.Empty;
            CaptchaImage = CaptchaImageHelper.DecodePngBase64(captcha.ImageBase64);
            if (CaptchaImage is null)
            {
                ErrorMessage = "验证码图片解析失败";
            }
        }
        catch (Exception ex)
        {
            CaptchaImage = null;
            ErrorMessage = $"验证码加载失败：{ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SendRegisterCodeAsync()
    {
        if (!NeedsOtp || OtpCooldown > 0 || IsSendingCode)
        {
            return;
        }

        var target = IsEmailChannel ? Email.Trim() : Phone.Trim();
        if (string.IsNullOrWhiteSpace(target))
        {
            ErrorMessage = $"请输入{IdentityLabel}";
            return;
        }

        if (IsEmailChannel && !EmailRegex.IsMatch(target))
        {
            ErrorMessage = "请输入有效邮箱";
            return;
        }

        if (IsPhoneChannel && !PhoneRegex.IsMatch(target))
        {
            ErrorMessage = "请输入有效手机号";
            return;
        }

        if (string.IsNullOrWhiteSpace(CaptchaId) || string.IsNullOrWhiteSpace(CaptchaValue))
        {
            ErrorMessage = "请输入图形验证码";
            return;
        }

        IsSendingCode = true;
        ErrorMessage = null;
        try
        {
            await _authApi.SendRegisterCodeAsync(new SendLoginCodeRequest
            {
                Target = target,
                Channel = RegisterChannel,
                CaptchaId = CaptchaId,
                CaptchaValue = CaptchaValue.Trim(),
            }).ConfigureAwait(true);

            StartOtpCooldown();
            await RefreshCaptchaAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await RefreshCaptchaAsync().ConfigureAwait(true);
        }
        finally
        {
            IsSendingCode = false;
        }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = null;
        if (IsClosed)
        {
            return;
        }

        var request = new RegisterRequest
        {
            RegisterChannel = RegisterChannel,
            CaptchaId = CaptchaId,
            CaptchaValue = CaptchaValue.Trim(),
        };

        if (IsPasswordMode)
        {
            var name = Account.Trim();
            if (name.Length is < 3 or > 64)
            {
                ErrorMessage = "用户名需 3-64 个字符";
                return;
            }

            request.RegisterChannel = "ACCOUNT";
            request.Account = name;

            if (RequireEmail)
            {
                var mail = Email.Trim();
                if (!EmailRegex.IsMatch(mail) || mail.Length > 128)
                {
                    ErrorMessage = "邮箱格式不正确";
                    return;
                }

                request.Email = mail;
            }

            if (RequirePhone)
            {
                var mobile = Phone.Trim();
                if (!PhoneRegex.IsMatch(mobile))
                {
                    ErrorMessage = "请输入有效手机号";
                    return;
                }

                request.Phone = mobile;
            }
        }
        else if (IsEmailChannel)
        {
            var mail = Email.Trim();
            if (!EmailRegex.IsMatch(mail) || mail.Length > 128)
            {
                ErrorMessage = "邮箱格式不正确";
                return;
            }

            if (string.IsNullOrWhiteSpace(OtpCode))
            {
                ErrorMessage = "请输入邮箱验证码";
                return;
            }

            request.Email = mail;
            request.OtpCode = OtpCode.Trim();
        }
        else
        {
            var mobile = Phone.Trim();
            if (!PhoneRegex.IsMatch(mobile))
            {
                ErrorMessage = "请输入有效手机号";
                return;
            }

            if (string.IsNullOrWhiteSpace(OtpCode))
            {
                ErrorMessage = "请输入手机验证码";
                return;
            }

            request.Phone = mobile;
            request.OtpCode = OtpCode.Trim();
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "请输入密码";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "两次密码输入不一致";
            return;
        }

        if (string.IsNullOrWhiteSpace(CaptchaId) || string.IsNullOrWhiteSpace(CaptchaValue))
        {
            ErrorMessage = "请输入图形验证码";
            return;
        }

        IsBusy = true;
        try
        {
            var key = await _authApi.GetPasswordKeyAsync().ConfigureAwait(true);
            request.Password = PasswordCrypto.EncryptRsaOaepSha256(key.PublicKey, Password);
            request.PasswordKeyId = key.KeyId;

            await _authApi.RegisterAsync(request).ConfigureAwait(true);
            _navigator.ShowLogin("注册成功，请登录");
        }
        catch (ApiResponseException ex)
        {
            ErrorMessage = ex.Message;
            await RefreshCaptchaAsync().ConfigureAwait(true);
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"无法连接服务器：{ex.Message}";
            await RefreshCaptchaAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await RefreshCaptchaAsync().ConfigureAwait(true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            var options = await _authApi.GetAuthOptionsAsync().ConfigureAwait(true);
            RegisterEnabled = WireJson.ReadBool(options.RegisterEnabled, false);
            AllowAccount = WireJson.ReadBool(options.RegisterAllowAccount, true);
            AllowEmail = WireJson.ReadBool(options.RegisterAllowEmail, true);
            AllowPhone = WireJson.ReadBool(options.RegisterAllowPhone, false);
            RequireEmail = WireJson.ReadBool(options.RegisterRequireEmail, false);
            RequirePhone = WireJson.ReadBool(options.RegisterRequirePhone, false);
        }
        catch
        {
            // Keep defaults when options endpoint is unavailable.
        }

        ApplyDefaultModeAndChannel();

        if (!IsClosed)
        {
            await RefreshCaptchaAsync().ConfigureAwait(true);
        }
    }

    private void ApplyDefaultModeAndChannel()
    {
        if (AllowAccount)
        {
            RegisterMode = "PASSWORD";
            RegisterChannel = "ACCOUNT";
        }
        else if (AllowEmail)
        {
            RegisterMode = "OTP";
            RegisterChannel = "EMAIL";
        }
        else if (AllowPhone)
        {
            RegisterMode = "OTP";
            RegisterChannel = "PHONE";
        }

        NotifyUiState();
    }

    private void StartOtpCooldown()
    {
        OtpCooldown = 60;
        _ = Task.Run(async () =>
        {
            while (OtpCooldown > 0)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                var next = OtpCooldown - 1;
                await Application.Current.Dispatcher.InvokeAsync(() => OtpCooldown = next);
            }
        });
    }

    private void NotifyUiState()
    {
        OnPropertyChanged(nameof(IsClosed));
        OnPropertyChanged(nameof(IsPasswordMode));
        OnPropertyChanged(nameof(IsOtpMode));
        OnPropertyChanged(nameof(OtpAvailable));
        OnPropertyChanged(nameof(ModeSwitchAvailable));
        OnPropertyChanged(nameof(IsAccountChannel));
        OnPropertyChanged(nameof(IsEmailChannel));
        OnPropertyChanged(nameof(IsPhoneChannel));
        OnPropertyChanged(nameof(NeedsOtp));
        OnPropertyChanged(nameof(ModeSwitchLabel));
        OnPropertyChanged(nameof(ClosedVisibility));
        OnPropertyChanged(nameof(FormVisibility));
        OnPropertyChanged(nameof(ModeSwitchVisibility));
        OnPropertyChanged(nameof(PasswordTabsVisibility));
        OnPropertyChanged(nameof(OtpTabsVisibility));
        OnPropertyChanged(nameof(AccountTabVisibility));
        OnPropertyChanged(nameof(EmailTabVisibility));
        OnPropertyChanged(nameof(PhoneTabVisibility));
        OnPropertyChanged(nameof(AccountFieldsVisibility));
        OnPropertyChanged(nameof(EmailFieldsVisibility));
        OnPropertyChanged(nameof(PhoneFieldsVisibility));
        OnPropertyChanged(nameof(RequireEmailVisibility));
        OnPropertyChanged(nameof(RequirePhoneVisibility));
        OnPropertyChanged(nameof(OtpFieldsVisibility));
        OnPropertyChanged(nameof(IdentityLabel));
    }
}
