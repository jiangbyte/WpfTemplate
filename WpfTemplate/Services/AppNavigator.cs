using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WpfTemplate.Services.Abstractions;
using WpfTemplate.ViewModels.Auth;
using WpfTemplate.Views.Auth;

namespace WpfTemplate.Services;

public sealed class AppNavigator : IAppNavigator
{
    private readonly IServiceProvider _services;
    private readonly IAuthService _auth;
    private Window? _current;

    public AppNavigator(IServiceProvider services, IAuthService auth)
    {
        _services = services;
        _auth = auth;
        _auth.AuthenticationLost += (_, _) =>
        {
            Application.Current.Dispatcher.Invoke(() => ShowLogin());
        };
    }

    public void ShowLogin(string? statusMessage = null)
    {
        var login = _services.GetRequiredService<LoginWindow>();
        if (login.DataContext is LoginViewModel vm)
        {
            vm.StatusMessage = statusMessage;
            vm.ErrorMessage = null;
        }

        SwitchTo(login);
    }

    public void ShowRegister()
    {
        var window = _services.GetRequiredService<RegisterWindow>();
        SwitchTo(window);
    }

    public void ShowForgotPassword(string? resetToken = null)
    {
        var window = _services.GetRequiredService<ForgotPasswordWindow>();
        if (window.DataContext is ForgotPasswordViewModel vm)
        {
            vm.ApplyResetToken(resetToken);
        }

        SwitchTo(window);
    }

    public void ShowMain()
    {
        var main = _services.GetRequiredService<MainWindow>();
        SwitchTo(main);
    }

    private void SwitchTo(Window next)
    {
        var previous = _current;

        // 认证窗之间切换时继承位置与尺寸，避免闪跳割裂
        if (previous is not null
            && IsAuthWindow(previous)
            && IsAuthWindow(next)
            && previous.WindowState == WindowState.Normal)
        {
            next.WindowStartupLocation = WindowStartupLocation.Manual;
            next.Left = previous.Left;
            next.Top = previous.Top;
            next.Width = previous.Width;
            next.Height = previous.Height;
        }

        _current = next;
        next.Show();
        Application.Current.MainWindow = next;

        if (previous is not null && !ReferenceEquals(previous, next))
        {
            previous.Close();
        }
    }

    private static bool IsAuthWindow(Window window) =>
        window is LoginWindow or RegisterWindow or ForgotPasswordWindow;
}
