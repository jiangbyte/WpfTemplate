using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using WpfTemplate.Api;
using WpfTemplate.Models;
using WpfTemplate.Services.Abstractions;

namespace WpfTemplate.ViewModels.Shell;

public partial class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IAuthService _auth;
    private readonly IAppNavigator _navigator;
    private readonly AuthApi _authApi;
    private readonly ISessionStore _session;
    private readonly ApiOptions _apiOptions;
    private readonly HttpClient _http;
    private int _avatarLoadVersion;

    public ShellViewModel(
        INavigationService navigation,
        IAuthService auth,
        IAppNavigator navigator,
        AuthApi authApi,
        ISessionStore session,
        IOptions<ApiOptions> apiOptions,
        IHttpClientFactory httpClientFactory)
    {
        _navigation = navigation;
        _auth = auth;
        _navigator = navigator;
        _authApi = authApi;
        _session = session;
        _apiOptions = apiOptions.Value;
        _http = httpClientFactory.CreateClient();
        _http.Timeout = TimeSpan.FromSeconds(30);

        _navigation.Navigated += (_, _) => RefreshNavigation();
        _navigation.Navigate("home");
        RefreshNavigation();
        ApplyUser(_auth.CurrentUser);
        _ = RefreshUserAsync();
    }

    public IReadOnlyList<MenuItemModel> Menus => _navigation.Menus;

    [ObservableProperty]
    private object? currentPage;

    [ObservableProperty]
    private string pageTitle = "首页";

    [ObservableProperty]
    private string userDisplayName = "用户";

    [ObservableProperty]
    private string userSubtitle = string.Empty;

    [ObservableProperty]
    private string avatarInitials = "用";

    [ObservableProperty]
    private BitmapImage? avatarImage;

    [ObservableProperty]
    private bool isPaneOpen = true;

    [ObservableProperty]
    private string? selectedMenuKey = "home";

    public Visibility ExpandedUserInfoVisibility => IsPaneOpen ? Visibility.Visible : Visibility.Collapsed;

    public Visibility CompactUserInfoVisibility => IsPaneOpen ? Visibility.Collapsed : Visibility.Visible;

    public HorizontalAlignment UserButtonAlignment => IsPaneOpen ? HorizontalAlignment.Stretch : HorizontalAlignment.Center;

    public Thickness UserButtonMargin => IsPaneOpen ? new Thickness(8, 4, 8, 12) : new Thickness(0, 8, 0, 12);

    public Thickness UserButtonPadding => IsPaneOpen ? new Thickness(8) : new Thickness(0);

    public HorizontalAlignment UserButtonContentAlignment =>
        IsPaneOpen ? HorizontalAlignment.Left : HorizontalAlignment.Center;

    partial void OnIsPaneOpenChanged(bool value)
    {
        OnPropertyChanged(nameof(ExpandedUserInfoVisibility));
        OnPropertyChanged(nameof(CompactUserInfoVisibility));
        OnPropertyChanged(nameof(UserButtonAlignment));
        OnPropertyChanged(nameof(UserButtonMargin));
        OnPropertyChanged(nameof(UserButtonPadding));
        OnPropertyChanged(nameof(UserButtonContentAlignment));
    }

    partial void OnSelectedMenuKeyChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value != _navigation.SelectedMenu?.Key)
        {
            _navigation.Navigate(value);
            RefreshNavigation();
        }
    }

    [RelayCommand]
    private void GoHome()
    {
        SelectedMenuKey = "home";
    }

    [RelayCommand]
    private void GoAbout()
    {
        SelectedMenuKey = "about";
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirm = MessageBox.Show(
            "确定退出当前账号？",
            "退出登录",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        await _auth.LogoutAsync().ConfigureAwait(true);
        _navigator.ShowLogin();
    }

    private async Task RefreshUserAsync()
    {
        try
        {
            var me = await _authApi.MeAsync().ConfigureAwait(true);
            _session.SaveUser(me);
            ApplyUser(me);
        }
        catch
        {
            // Keep session cache when refresh fails.
        }
    }

    private void ApplyUser(UserInfo? user)
    {
        UserDisplayName = user?.DisplayName ?? "用户";
        UserSubtitle = !string.IsNullOrWhiteSpace(user?.Email)
            ? user!.Email!.Trim()
            : (user?.Account?.Trim() ?? string.Empty);
        AvatarInitials = BuildInitials(UserDisplayName);
        _ = LoadAvatarAsync(user?.Avatar);
    }

    private async Task LoadAvatarAsync(string? avatar)
    {
        var version = Interlocked.Increment(ref _avatarLoadVersion);
        AvatarImage = null;

        var url = ResolveAvatarUrl(avatar);
        if (url is null)
        {
            return;
        }

        try
        {
            using var response = await _http.GetAsync(url).ConfigureAwait(true);
            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(true);
            if (version != _avatarLoadVersion || bytes.Length == 0)
            {
                return;
            }

            using var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = 96;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            AvatarImage = image;
        }
        catch
        {
            if (version == _avatarLoadVersion)
            {
                AvatarImage = null;
            }
        }
    }

    private string? ResolveAvatarUrl(string? avatar)
    {
        if (string.IsNullOrWhiteSpace(avatar))
        {
            return null;
        }

        var value = avatar.Trim();
        if (Uri.TryCreate(value, UriKind.Absolute, out _))
        {
            return value;
        }

        if (value.StartsWith('/'))
        {
            return _apiOptions.BaseUrl.TrimEnd('/') + value;
        }

        if (File.Exists(value))
        {
            return new Uri(Path.GetFullPath(value)).AbsoluteUri;
        }

        return null;
    }

    private void RefreshNavigation()
    {
        CurrentPage = _navigation.CurrentPage;
        PageTitle = _navigation.SelectedMenu?.Title ?? string.Empty;
        SelectedMenuKey = _navigation.SelectedMenu?.Key;
    }

    private static string BuildInitials(string displayName)
    {
        var text = displayName.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return "用";
        }

        if (text.Any(c => c > 127))
        {
            return text[^1].ToString().ToUpperInvariant();
        }

        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";
        }

        return text.Length >= 2
            ? text[..2].ToUpperInvariant()
            : text.ToUpperInvariant();
    }
}
