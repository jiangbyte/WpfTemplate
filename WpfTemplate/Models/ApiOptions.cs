namespace WpfTemplate.Models;

public sealed class ApiOptions
{
    public const string SectionName = "Api";

    /// <summary>后端根地址，对应 web/portal 的 VITE_API_URL（为空时 web 走同源代理；桌面端需显式配置）。</summary>
    public string BaseUrl { get; set; } = "http://localhost:8000";
}
