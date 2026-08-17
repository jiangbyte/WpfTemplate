using System.Windows;
using WpfTemplate.ViewModels.Auth;
using Wpf.Ui.Controls;
using PasswordBox = Wpf.Ui.Controls.PasswordBox;

namespace WpfTemplate.Views.Auth;

public partial class ForgotPasswordWindow
{
    public ForgotPasswordWindow(ForgotPasswordViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ForgotPasswordViewModel vm && sender is PasswordBox box)
        {
            vm.Password = box.Password;
        }
    }

    private void ConfirmPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ForgotPasswordViewModel vm && sender is PasswordBox box)
        {
            vm.ConfirmPassword = box.Password;
        }
    }

    private void CaptchaImage_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is ForgotPasswordViewModel vm && vm.RefreshCaptchaCommand.CanExecute(null))
        {
            vm.RefreshCaptchaCommand.Execute(null);
        }
    }
}
