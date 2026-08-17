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
            else if (e.PropertyName == nameof(ShellViewModel.SelectedMenuKey))
            {
                SyncMenuSelection();
            }
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.IsPaneOpen = RootNavigation.IsPaneOpen;
        SyncMenuSelection();
        ReplacePageContent();
    }

    private void TogglePaneButton_OnClick(object sender, RoutedEventArgs e)
    {
        RootNavigation.SetCurrentValue(NavigationView.IsPaneOpenProperty, !RootNavigation.IsPaneOpen);
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

    private void SyncMenuSelection()
    {
        var key = _viewModel.SelectedMenuKey;
        foreach (var item in RootNavigation.MenuItems.OfType<NavigationViewItem>())
        {
            var active = item.Tag is string tag && tag == key;
            item.SetCurrentValue(NavigationViewItem.IsActiveProperty, active);
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
