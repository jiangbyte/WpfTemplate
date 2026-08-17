using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WpfTemplate.ViewModels.Auth;
using Wpf.Ui.Controls;
using PasswordBox = Wpf.Ui.Controls.PasswordBox;

namespace WpfTemplate.Views.Auth;

public partial class RegisterWindow
{
    private RegisterViewModel? _viewModel;
    private bool _syncingTabs;

    public RegisterWindow(RegisterViewModel viewModel)
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
        if (e.PropertyName is nameof(RegisterViewModel.RegisterChannel)
            or nameof(RegisterViewModel.RegisterMode)
            or nameof(RegisterViewModel.AllowAccount)
            or nameof(RegisterViewModel.AllowEmail)
            or nameof(RegisterViewModel.AllowPhone)
            or nameof(RegisterViewModel.FormVisibility)
            or nameof(RegisterViewModel.OtpTabsVisibility))
        {
            SyncSelectedTab();
        }
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm && sender is PasswordBox box)
        {
            vm.Password = box.Password;
        }
    }

    private void ConfirmPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm && sender is PasswordBox box)
        {
            vm.ConfirmPassword = box.Password;
        }
    }

    private void CaptchaImage_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm && vm.RefreshCaptchaCommand.CanExecute(null))
        {
            vm.RefreshCaptchaCommand.Execute(null);
        }
    }

    private void RegisterTabs_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingTabs || DataContext is not RegisterViewModel vm)
        {
            return;
        }

        if (RegisterTabs.SelectedItem is TabItem { Tag: string type })
        {
            vm.RegisterChannel = type;
        }
    }

    private void SyncSelectedTab()
    {
        if (_viewModel is null || _viewModel.IsClosed || !_viewModel.IsOtpMode)
        {
            return;
        }

        _syncingTabs = true;
        try
        {
            foreach (var item in RegisterTabs.Items)
            {
                if (item is TabItem tab
                    && tab.Visibility == Visibility.Visible
                    && tab.Tag is string type
                    && type == _viewModel.RegisterChannel)
                {
                    RegisterTabs.SelectedItem = tab;
                    return;
                }
            }

            foreach (var item in RegisterTabs.Items)
            {
                if (item is TabItem { Visibility: Visibility.Visible } tab)
                {
                    RegisterTabs.SelectedItem = tab;
                    if (tab.Tag is string type)
                    {
                        _viewModel.RegisterChannel = type;
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
