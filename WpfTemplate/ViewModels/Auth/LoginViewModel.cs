using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfTemplate.Api;
using WpfTemplate.Helpers;
using WpfTemplate.Models;
using WpfTemplate.Services.Abstractions;

namespace WpfTemplate.ViewModels.Auth;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly IAppNavigator _navigator;
    private readonly AuthApi _authApi;

    public LoginViewModel(IAuthService auth, IAppNavigator navigator, AuthApi authApi)
    {
        _auth = auth;
        _navigator = navigator;
        _authApi = authApi;
        _ = InitializeAsync();
    }

    [ObservableProperty]
    private string identityType = "ACCOUNT";

    [ObservableProperty]
    private string loginMode = "PASSWORD";

    [ObservableProperty]
    private string identityValue = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string otpCode = string.Empty;

    [ObservableProperty]
    private string captchaId = string.Empty;

    [ObservableProperty]
    private string captchaValue = string.Empty;

    [ObservableProperty]
    private BitmapImage? captchaImage;

    [ObservableProperty]
    private bool isCaptchaLoading;

    [ObservableProperty]
    private bool rememberMe = true;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isSendingCode;

    [ObservableProperty]
    private int otpCooldown;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool allowAccount = true;

    [ObservableProperty]
    private bool allowEmail = true;

    [ObservableProperty]
    private bool allowPhone = true;

    [ObservableProperty]
    private bool allowOtp = true;

    [ObservableProperty]
    private bool registerEnabled;

    public string IdentityPlaceholder => IdentityType switch
    {
        "EMAIL" => "请输入登录邮箱",
        "PHONE" => "请输入登录手机号",
        _ => "请输入账号",
    };

    public string IdentityLabel => IdentityType switch
    {
        "EMAIL" => "邮箱",
        "PHONE" => "手机号",
        _ => "账号",
    };

    public bool IsPasswordMode => string.Equals(LoginMode, "PASSWORD", StringComparison.OrdinalIgnoreCase);

    public bool IsOtpMode => !IsPasswordMode;

    public bool OtpAvailable =>
        AllowOtp && (IdentityType is "EMAIL" or "PHONE");

    public string ModeSwitchLabel => IsPasswordMode ? "验证码登录" : "密码登录";

    public string SendCodeLabel => OtpCooldown > 0 ? $"{OtpCooldown}s 后重发" : "发送验证码";

    public bool CanSendCode => !IsSendingCode && OtpCooldown <= 0;

    public Visibility AccountTabVisibility => AllowAccount ? Visibility.Visible : Visibility.Collapsed;

    public Visibility EmailTabVisibility => AllowEmail ? Visibility.Visible : Visibility.Collapsed;

    public Visibility PhoneTabVisibility => AllowPhone ? Visibility.Visible : Visibility.Collapsed;

    public Visibility OtpSwitchVisibility => OtpAvailable ? Visibility.Visible : Visibility.Collapsed;

    public Visibility PasswordFieldsVisibility => IsPasswordMode ? Visibility.Visible : Visibility.Collapsed;

    public Visibility OtpFieldsVisibility => IsOtpMode ? Visibility.Visible : Visibility.Collapsed;

    public Visibility RegisterLinkVisibility => RegisterEnabled ? Visibility.Visible : Visibility.Collapsed;

    partial void OnIdentityTypeChanged(string value)
    {
        if (!OtpAvailable)
        {
            LoginMode = "PASSWORD";
        }

        NotifyUiState();
    }

    partial void OnLoginModeChanged(string value) => NotifyUiState();

    partial void OnAllowAccountChanged(bool value) => OnPropertyChanged(nameof(AccountTabVisibility));

    partial void OnAllowEmailChanged(bool value) => OnPropertyChanged(nameof(EmailTabVisibility));

    partial void OnAllowPhoneChanged(bool value) => OnPropertyChanged(nameof(PhoneTabVisibility));

    partial void OnAllowOtpChanged(bool value) => NotifyUiState();

    partial void OnRegisterEnabledChanged(bool value) => OnPropertyChanged(nameof(RegisterLinkVisibility));

    partial void OnOtpCooldownChanged(int value)
    {
        OnPropertyChanged(nameof(SendCodeLabel));
        OnPropertyChanged(nameof(CanSendCode));
    }

    partial void OnIsSendingCodeChanged(bool value) => OnPropertyChanged(nameof(CanSendCode));

    [RelayCommand]
    private void SelectIdentity(string? type)
    {
        if (!string.IsNullOrWhiteSpace(type))
        {
            IdentityType = type;
        }
    }

    [RelayCommand]
    private void ToggleLoginMode()
    {
        if (!OtpAvailable)
        {
            return;
        }

        LoginMode = IsPasswordMode ? "OTP" : "PASSWORD";
    }

    [RelayCommand]
    private void GoRegister()
    {
        if (RegisterEnabled)
        {
            _navigator.ShowRegister();
        }
    }

    [RelayCommand]
    private void GoForgotPassword() => _navigator.ShowForgotPassword();

    [RelayCommand]
    private async Task RefreshCaptchaAsync()
    {
        IsCaptchaLoading = true;
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
        finally
        {
            IsCaptchaLoading = false;
        }
    }

    [RelayCommand]
    private async Task SendLoginCodeAsync()
    {
        if (OtpCooldown > 0 || IsSendingCode)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(IdentityValue))
        {
            ErrorMessage = $"请输入{IdentityLabel}";
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
            await _authApi.SendLoginCodeAsync(new SendLoginCodeRequest
            {
                Target = IdentityValue.Trim(),
                Channel = IdentityType == "EMAIL" ? "EMAIL" : "PHONE",
                CaptchaId = CaptchaId,
                CaptchaValue = CaptchaValue.Trim(),
            }).ConfigureAwait(true);

            StartOtpCooldown();
            await RefreshCaptchaAsync().ConfigureAwait(true);
        }
        catch (ApiResponseException ex)
        {
            ErrorMessage = ex.Message;
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
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        StatusMessage = null;
        if (string.IsNullOrWhiteSpace(IdentityValue))
        {
            ErrorMessage = $"请输入{IdentityLabel}";
            return;
        }

        if (IsPasswordMode && string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "请输入密码";
            return;
        }

        if (IsOtpMode && string.IsNullOrWhiteSpace(OtpCode))
        {
            ErrorMessage = "请输入登录验证码";
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
            await _auth.LoginAsync(
                IdentityValue,
                Password,
                CaptchaId,
                CaptchaValue,
                IdentityType,
                LoginMode,
                OtpCode,
                RememberMe).ConfigureAwait(true);
            _navigator.ShowMain();
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
            AllowAccount = WireJson.ReadBool(options.AllowAccount, true);
            AllowEmail = WireJson.ReadBool(options.AllowEmail, true);
            AllowPhone = WireJson.ReadBool(options.AllowPhone, true);
            AllowOtp = WireJson.ReadBool(options.AllowOtp, true);
            RegisterEnabled = WireJson.ReadBool(options.RegisterEnabled, false);

            if (AllowAccount)
            {
                IdentityType = "ACCOUNT";
            }
            else if (AllowEmail)
            {
                IdentityType = "EMAIL";
            }
            else if (AllowPhone)
            {
                IdentityType = "PHONE";
            }
        }
        catch
        {
            // Keep defaults when options endpoint is unavailable.
        }

        await RefreshCaptchaAsync().ConfigureAwait(true);
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
        OnPropertyChanged(nameof(IdentityPlaceholder));
        OnPropertyChanged(nameof(IdentityLabel));
        OnPropertyChanged(nameof(IsPasswordMode));
        OnPropertyChanged(nameof(IsOtpMode));
        OnPropertyChanged(nameof(OtpAvailable));
        OnPropertyChanged(nameof(ModeSwitchLabel));
        OnPropertyChanged(nameof(OtpSwitchVisibility));
        OnPropertyChanged(nameof(PasswordFieldsVisibility));
        OnPropertyChanged(nameof(OtpFieldsVisibility));
    }
}
