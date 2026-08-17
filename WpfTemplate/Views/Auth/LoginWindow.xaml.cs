using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WpfTemplate.ViewModels.Auth;
using Wpf.Ui.Controls;
using PasswordBox = Wpf.Ui.Controls.PasswordBox;

namespace WpfTemplate.Views.Auth;

public partial class LoginWindow
{
    private LoginViewModel? _viewModel;
    private bool _syncingTabs;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        Loaded += OnLoaded;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        Closed += (_, _) =>
        {
            if (_viewModel is not null)
            {
                _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            }
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => SyncSelectedTab();

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(LoginViewModel.IdentityType)
            or nameof(LoginViewModel.AllowAccount)
            or nameof(LoginViewModel.AllowEmail)
            or nameof(LoginViewModel.AllowPhone))
        {
            SyncSelectedTab();
        }
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm && sender is PasswordBox box)
        {
            vm.Password = box.Password;
        }
    }

    private void CaptchaImage_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm && vm.RefreshCaptchaCommand.CanExecute(null))
        {
            vm.RefreshCaptchaCommand.Execute(null);
        }
    }

    private void LoginTabs_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingTabs || DataContext is not LoginViewModel vm)
        {
            return;
        }

        if (LoginTabs.SelectedItem is TabItem { Tag: string type })
        {
            vm.IdentityType = type;
        }
    }

    private void SyncSelectedTab()
    {
        if (_viewModel is null)
        {
            return;
        }

        _syncingTabs = true;
        try
        {
            foreach (var item in LoginTabs.Items)
            {
                if (item is TabItem tab
                    && tab.Visibility == Visibility.Visible
                    && tab.Tag is string type
                    && type == _viewModel.IdentityType)
                {
                    LoginTabs.SelectedItem = tab;
                    return;
                }
            }

            foreach (var item in LoginTabs.Items)
            {
                if (item is TabItem { Visibility: Visibility.Visible } tab)
                {
                    LoginTabs.SelectedItem = tab;
                    if (tab.Tag is string type)
                    {
                        _viewModel.IdentityType = type;
                    }

                    return;
                }
            }
        }
        finally
        {
            _syncingTabs = false;
        }
    }
}
