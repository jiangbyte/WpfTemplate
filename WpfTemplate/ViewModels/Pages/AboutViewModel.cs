using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;

namespace WpfTemplate.ViewModels.Pages;

public partial class AboutViewModel : ObservableObject
{
    public string Title => "关于";

    public string ProductName => "WpfTemplate";

    public string Version =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

    public string Description =>
        "WPF 桌面脚手架基础布局项目。基于 WPF-UI，含 Fluent 壳层、侧边导航、登录/注册/找回密码与 Portal HTTP 客户端基建。";
}
