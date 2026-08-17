using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfTemplate.ViewModels.Pages;

public partial class HomeViewModel : ObservableObject
{
    public string Title => "首页";

    public string Description =>
        "欢迎使用 WpfTemplate。这是一套 WPF 桌面脚手架：壳层布局、导航、登录与 HTTP 基建已就绪，可在此基础上扩展业务页面。";
}
