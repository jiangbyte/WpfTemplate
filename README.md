# WpfTemplate

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/UI-WPF-0078D4?logo=windows&logoColor=white)
![WPF-UI](https://img.shields.io/badge/WPF--UI-4.3-5C2D91)
![MVVM](https://img.shields.io/badge/MVVM-CommunityToolkit-512BD4)
![License](https://img.shields.io/badge/License-MIT-yellow)
![Version](https://img.shields.io/badge/version-1.0.0-orange)

**WpfTemplate** 是一套开箱即用的 WPF 桌面脚手架：Fluent 壳层布局、侧边导航、登录 / 注册 / 找回密码，以及对齐 HEI Portal 的 HTTP 客户端基建，方便在此基础上快速扩展业务页面。

> 当前版本：`1.0.0` · 协议：[MIT License](LICENSE)

## 目录

- [功能特性](#功能特性)
- [技术栈](#技术栈)
- [工程结构](#工程结构)
- [快速开始](#快速开始)
- [配置说明](#配置说明)
- [默认账号](#默认账号)
- [License](#license)

## 功能特性

- Fluent 主窗口：TitleBar + NavigationView 壳层，首页 / 关于示例页
- 认证流程：登录、注册、找回与重置密码（图形验证码、密码 RSA 加密、OTP 可选）
- Portal 会话：本地持久化 Token / 用户信息，未授权自动回到登录
- HTTP 基建：`ApiClient`、统一响应解包、文件服务封装
- DI / Hosting：`Microsoft.Extensions.Hosting` + CommunityToolkit.Mvvm

## 技术栈

| 类别 | 选型 |
| --- | --- |
| 运行时 | .NET 10（`net10.0-windows`） |
| UI | WPF · [WPF-UI](https://github.com/lepoco/wpfui) 4.3 |
| MVVM | CommunityToolkit.Mvvm |
| 基础设施 | Microsoft.Extensions.Hosting / Http / Configuration |
| 后端契约 | HEI Portal（`/api/v1/portal`） |

## 工程结构

```text
WpfTemplate/
├── WpfTemplate.slnx
├── LICENSE / README.md
└── WpfTemplate/
    ├── Api/                 AuthApi、FileApi
    ├── Configuration/       appsettings.json
    ├── Models/              请求/响应与选项
    ├── Services/            Auth、Http、Navigation、Files
    ├── ViewModels/          Auth / Shell / Pages
    ├── Views/               登录窗、主壳、页面
    ├── Resources/           主题与样式
    └── Assets/              图标等资源
```

## 快速开始

### 环境要求

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- 可选：Visual Studio 2022 / Rider / Cursor

### 1. 启动后端（hei-boot）

本客户端默认对接 [hei-boot](https://github.com/jiangbyte/hei-boot) 的 Portal API（`http://127.0.0.1:8000`）。请先按 hei-boot 仓库 README 初始化数据库并启动：

```bash
# 在 hei-boot 仓库内
mvn -pl app/admin -am spring-boot:run
```

| 项 | 地址 |
| --- | --- |
| API | http://127.0.0.1:8000 |
| 接口文档 | http://127.0.0.1:8000/doc.html |

### 2. 克隆并运行本项目

```bash
git clone git@github.com:jiangbyte/WpfTemplate.git
cd WpfTemplate
dotnet restore WpfTemplate/WpfTemplate.csproj
dotnet run --project WpfTemplate/WpfTemplate.csproj
```

或用 Visual Studio / Rider 打开 `WpfTemplate.slnx` 后 F5。

## 配置说明

API 基址见 [`WpfTemplate/Configuration/appsettings.json`](WpfTemplate/Configuration/appsettings.json)：

```json
{
  "Api": {
    "BaseUrl": "http://localhost:8000"
  }
}
```

本地会话目录：`%LocalAppData%\WpfTemplate\`。

## 默认账号

与 hei-boot Portal 演示账号一致：

| 端 | 账号 | 密码 |
| --- | --- | --- |
| Portal | `user` | `123456` |

> 仅供本地演示。部署到非本机环境后请立即修改默认密码，并核对后端配置与密钥。

## License

本项目基于 [MIT License](LICENSE) 开源。完整条款见 [LICENSE](LICENSE)。
