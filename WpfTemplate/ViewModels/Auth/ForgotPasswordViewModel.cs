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

public partial class ForgotPasswordViewModel : ObservableObject
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly AuthApi _authApi;
    private readonly IAppNavigator _navigator;

    public ForgotPasswordViewModel(AuthApi authApi, IAppNavigator navigator)
    {
        _authApi = authApi;
        _navigator = navigator;
        _ = RefreshCaptchaAsync();
    }

    [ObservableProperty]
    private bool isResetMode;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string resetToken = string.Empty;

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
    private string? errorMessage;

    [ObservableProperty]
    private string? statusMessage;

    public string Title => IsResetMode ? "重置密码" : "找回密码";

    public string Description => IsResetMode
        ? "请设置新密码。重置链接在过期前仅可使用一次。"
        : "请输入已启用登录的邮箱，系统将发送密码重置链接。";

    public string BrandLead => IsResetMode
        ? "链接仅可使用一次，完成后请使用新密码登录。"
        : "通过邮箱验证身份后即可重置密码。";

    public string SubmitLabel => IsResetMode ? "重置密码" : "发送重置链接";

    public Visibility SendLinkVisibility => IsResetMode ? Visibility.Collapsed : Visibility.Visible;

    public Visibility ResetVisibility => IsResetMode ? Visibility.Visible : Visibility.Collapsed;

    public void ApplyResetToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            IsResetMode = false;
            ResetToken = string.Empty;
            return;
        }

        ResetToken = token.Trim();
        IsResetMode = true;
    }

    partial void OnIsResetModeChanged(bool value)
    {
        ErrorMessage = null;
        StatusMessage = null;
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(BrandLead));
        OnPropertyChanged(nameof(SubmitLabel));
        OnPropertyChanged(nameof(SendLinkVisibility));
        OnPropertyChanged(nameof(ResetVisibility));
    }

    [RelayCommand]
    private void GoLogin() => _navigator.ShowLogin();

    [RelayCommand]
    private void GoRegister() => _navigator.ShowRegister();

    [RelayCommand]
    private void SwitchToSendLink()
    {
        IsResetMode = false;
        ResetToken = string.Empty;
    }

    [RelayCommand]
    private void SwitchToReset()
    {
        IsResetMode = true;
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
    private async Task SubmitAsync()
    {
        if (IsResetMode)
        {
            await ResetPasswordAsync().ConfigureAwait(true);
        }
        else
        {
            await SendLinkAsync().ConfigureAwait(true);
        }
    }

    private async Task SendLinkAsync()
    {
        ErrorMessage = null;
        StatusMessage = null;

        var mail = Email.Trim();
        if (!EmailRegex.IsMatch(mail))
        {
            ErrorMessage = "请输入有效邮箱";
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
            await _authApi.ForgotPasswordAsync(new ForgotPasswordRequest
            {
                Email = mail,
                CaptchaId = CaptchaId,
                CaptchaValue = CaptchaValue.Trim(),
            }).ConfigureAwait(true);

            StatusMessage = "密码重置链接已发送，请查收邮箱";
            await RefreshCaptchaAsync().ConfigureAwait(true);
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

    private async Task ResetPasswordAsync()
    {
        ErrorMessage = null;
        StatusMessage = null;

        if (string.IsNullOrWhiteSpace(ResetToken))
        {
            ErrorMessage = "请输入重置令牌";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "请输入新密码";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "两次输入的密码不一致";
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
            await _authApi.ResetPasswordAsync(new ResetPasswordRequest
            {
                Token = ResetToken.Trim(),
                Password = PasswordCrypto.EncryptRsaOaepSha256(key.PublicKey, Password),
                PasswordKeyId = key.KeyId,
                CaptchaId = CaptchaId,
                CaptchaValue = CaptchaValue.Trim(),
            }).ConfigureAwait(true);

            _navigator.ShowLogin("密码已重置，请重新登录");
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
}
