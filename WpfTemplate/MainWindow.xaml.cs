using System.Windows;
using System.Windows.Controls;
using WpfTemplate.ViewModels.Shell;
using Wpf.Ui.Controls;
using NavigationView = Wpf.Ui.Controls.NavigationView;

namespace WpfTemplate;

public partial class MainWindow
{
    private readonly ShellViewModel _viewModel;

    public MainWindow(ShellViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
        RootNavigation.PaneOpened += (_, _) => _viewModel.IsPaneOpen = true;
        RootNavigation.PaneClosed += (_, _) => _viewModel.IsPaneOpen = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ShellViewModel.CurrentPage))
            {
                ReplacePageContent();
            }
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.IsPaneOpen = RootNavigation.IsPaneOpen;
        ReplacePageContent();
    }

    private void TogglePaneButton_OnClick(object sender, RoutedEventArgs e)
    {
        RootNavigation.IsPaneOpen = !RootNavigation.IsPaneOpen;
        _viewModel.IsPaneOpen = RootNavigation.IsPaneOpen;
    }

    private void UserMenuButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (UserMenuButton.ContextMenu is null)
        {
            return;
        }

        UserMenuButton.ContextMenu.PlacementTarget = UserMenuButton;
        UserMenuButton.ContextMenu.Placement = _viewModel.IsPaneOpen
            ? System.Windows.Controls.Primitives.PlacementMode.Top
            : System.Windows.Controls.Primitives.PlacementMode.Right;
        UserMenuButton.ContextMenu.DataContext = _viewModel;
        UserMenuButton.ContextMenu.IsOpen = true;
    }

    private void RootNavigation_OnSelectionChanged(NavigationView sender, RoutedEventArgs args)
    {
        if (sender.SelectedItem is NavigationViewItem item && item.Tag is string key)
        {
            _viewModel.SelectedMenuKey = key;
        }
    }

    private void ReplacePageContent()
    {
        var host = new ContentControl
        {
            Content = _viewModel.CurrentPage,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
        };
        RootNavigation.ReplaceContent(host);
    }
}
