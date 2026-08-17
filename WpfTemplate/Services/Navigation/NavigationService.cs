using Microsoft.Extensions.DependencyInjection;
using WpfTemplate.Models;
using WpfTemplate.Services.Abstractions;
using WpfTemplate.ViewModels.Pages;

namespace WpfTemplate.Services.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private readonly List<MenuItemModel> _menus;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
        _menus =
        [
            new MenuItemModel
            {
                Key = "home",
                Title = "首页",
                ViewModelType = typeof(HomeViewModel),
            },
            new MenuItemModel
            {
                Key = "about",
                Title = "关于",
                ViewModelType = typeof(AboutViewModel),
            },
        ];
    }

    public IReadOnlyList<MenuItemModel> Menus => _menus;

    public MenuItemModel? SelectedMenu { get; private set; }

    public object? CurrentPage { get; private set; }

    public event EventHandler? Navigated;

    public void Navigate(string key)
    {
        var menu = _menus.FirstOrDefault(m => m.Key == key)
            ?? throw new InvalidOperationException($"未知菜单: {key}");
        SelectedMenu = menu;
        CurrentPage = _services.GetRequiredService(menu.ViewModelType);
        Navigated?.Invoke(this, EventArgs.Empty);
    }
}
