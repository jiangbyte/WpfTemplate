using System.IO;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WpfTemplate.Api;
using WpfTemplate.Models;
using WpfTemplate.Services;
using WpfTemplate.Services.Abstractions;
using WpfTemplate.Services.Auth;
using WpfTemplate.Services.Files;
using WpfTemplate.Services.Http;
using WpfTemplate.Services.Navigation;
using WpfTemplate.ViewModels.Auth;
using WpfTemplate.ViewModels.Pages;
using WpfTemplate.ViewModels.Shell;
using WpfTemplate.Views.Auth;

namespace WpfTemplate;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile(Path.Combine("Configuration", "appsettings.json"), optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<ApiOptions>(context.Configuration.GetSection(ApiOptions.SectionName));

                services.AddSingleton<ISessionStore, SessionStore>();
                services.AddSingleton<IUnauthorizedHandler, UnauthorizedHandler>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IAppNavigator, AppNavigator>();
                services.AddSingleton<IAuthService, AuthService>();
                services.AddSingleton<IFileService, FileService>();

                services.AddHttpClient("api", (sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
                    var baseUrl = options.BaseUrl.TrimEnd('/') + "/";
                    client.BaseAddress = new Uri(baseUrl);
                    client.Timeout = TimeSpan.FromSeconds(60);
                });

                services.AddSingleton<IApiClient>(sp =>
                {
                    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("api");
                    return ActivatorUtilities.CreateInstance<ApiClient>(sp, http);
                });

                services.AddSingleton<AuthApi>();
                services.AddSingleton<FileApi>();

                services.AddTransient<LoginViewModel>();
                services.AddTransient<RegisterViewModel>();
                services.AddTransient<ForgotPasswordViewModel>();
                services.AddTransient<ShellViewModel>();
                services.AddTransient<HomeViewModel>();
                services.AddTransient<AboutViewModel>();

                services.AddTransient<LoginWindow>();
                services.AddTransient<RegisterWindow>();
                services.AddTransient<ForgotPasswordWindow>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        var auth = _host.Services.GetRequiredService<IAuthService>();
        var navigator = _host.Services.GetRequiredService<IAppNavigator>();

        var restored = await auth.TryRestoreSessionAsync();
        if (restored)
        {
            navigator.ShowMain();
        }
        else
        {
            navigator.ShowLogin();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(2));
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
