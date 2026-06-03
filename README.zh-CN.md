# QuickLook.Plugin.GitFolderViewer

[![Build and Release](https://github.com/Chendaqian/QuickLook.Plugin.GitFolderViewer/actions/workflows/release.yml/badge.svg)](https://github.com/Chendaqian/QuickLook.Plugin.GitFolderViewer/actions/workflows/release.yml)
[![GitHub Release](https://img.shields.io/github/v/release/Chendaqian/QuickLook.Plugin.GitFolderViewer)](https://github.com/Chendaqian/QuickLook.Plugin.GitFolderViewer/releases/latest)

一个 [QuickLook](https://github.com/QL-Win/QuickLook) 插件，在资源管理器中按空格键即可预览 `.git` 文件夹信息。

[English](README.md)

## 功能

在任意 `.git` 文件夹上按空格键，即可查看：

- **当前分支**（或游离 HEAD 状态）
- **跟踪远端分支**（如 `origin/master`）
- **远端地址**（如 `https://github.com/...`）
- **HEAD 提交哈希**
- **分支数**（本地 + 远端）
- **最近提交信息**
- **最近活动**（最近 5 条 reflog 记录）

所有值均可点击选中复制。

### 多语言

- 英文（默认）
- 中文（zh-CN）— 系统语言为中文时自动生效

## 安装

### 从 Release 下载（推荐）

1. 前往 [Releases](https://github.com/Chendaqian/QuickLook.Plugin.GitFolderViewer/releases/latest)
2. 下载 `.qlplugin` 文件
3. 双击该文件，QuickLook 会自动安装

### 快速安装

在资源管理器中选中 `.qlplugin` 文件，按空格键，QuickLook 会直接安装。

### 手动安装

1. 从 [Releases](https://github.com/Chendaqian/QuickLook.Plugin.GitFolderViewer/releases/latest) 下载 `.qlplugin` 文件
2. 将 `.qlplugin` 后缀改为 `.zip`
3. 解压到 `%LocalAppData%\QuickLook\QuickLook.Plugin`
4. 重启 QuickLook

### 从源码编译

```bash
git clone --recurse-submodules https://github.com/Chendaqian/QuickLook.Plugin.GitFolderViewer.git
cd QuickLook.Plugin.GitFolderViewer
dotnet build src/QuickLook.Plugin.GitFolderViewer.csproj -c Release -p:LangVersion=preview
```

输出路径：`build/Release/QuickLook.Plugin.GitFolderViewer.dll`

将 DLL 和 `Translations.config` 复制到 QuickLook 插件目录即可。

## 工作原理

本插件直接读取 `.git` 文件夹中的文件获取信息，无需调用 `git.exe`：

| 文件 | 数据 |
|------|------|
| `HEAD` | 当前分支或游离提交哈希 |
| `config` | 远端地址、跟踪分支配置 |
| `refs/heads/` | 分支数、HEAD 提交哈希 |
| `refs/remotes/` | 远端分支数 |
| `COMMIT_EDITMSG` | 最近提交信息 |
| `logs/HEAD` | 最近 reflog 活动 |
| `packed-refs` | 打包引用的回退读取 |

## 系统要求

- [QuickLook](https://github.com/QL-Win/QuickLook) 3.7+
- .NET Framework 4.6.2

## 许可证

[GPL-3.0](LICENSE)
