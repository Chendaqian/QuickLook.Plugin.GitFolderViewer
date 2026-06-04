# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

QuickLook 插件，在 Windows 资源管理器中按空格键预览 `.git` 文件夹信息。直接读取 `.git` 内部文件获取数据，不依赖 `git.exe`。

## Build Commands

```bash
# 构建 Release
dotnet build src/QuickLook.Plugin.GitFolderViewer.csproj -c Release -p:LangVersion=preview

# 构建 Debug
dotnet build src/QuickLook.Plugin.GitFolderViewer.csproj -c Debug -p:LangVersion=preview
```

输出路径：`build/Release/` 或 `build/Debug/`

CI 构建会额外加 `-p:PreBuildEvent=""` 跳过预构建事件。

## Architecture

本项目是 QuickLook 插件，实现 `IViewer` 接口（来自 QuickLook.Common 子模块）。

### 核心文件

| 文件 | 职责 |
|------|------|
| `src/Plugin.cs` | 插件入口，实现 `IViewer` 接口。`CanHandle` 判断是否为 `.git` 目录，`View` 创建 UI 面板 |
| `src/GitInfoReader.cs` | 读取 `.git` 内部文件解析仓库信息（HEAD、config、refs、packed-refs、reflog） |
| `src/GitInfoPanel.xaml` + `.xaml.cs` | WPF 用户控件，通过数据绑定展示 GitInfo |
| `src/Translations.config` | 多语言翻译（en、zh-CN），通过 `TranslationHelper.Get()` 加载 |
| `QuickLook.Plugin.Metadata.config` | 插件元数据（命名空间、版本、描述） |

### 数据流

1. `Plugin.CanHandle()` 检查路径是否为含 HEAD 文件的 `.git` 目录
2. `Plugin.View()` 调用 `GitInfoReader.Read()` 解析 git 文件
3. `GitInfoReader` 按顺序读取：HEAD → config → refs → COMMIT_EDITMSG → logs/HEAD
4. 返回 `GitInfo` 数据对象，传递给 `GitInfoPanel.DisplayGitInfo()` 展示

### 子模块

`QuickLook.Common` 是 git 子模块，提供插件接口（`IViewer`）、翻译工具（`TranslationHelper`）和 WPF 样式。构建时需要 `--recurse-submodules`。

## Dependencies

- .NET Framework 4.6.2
- QuickLook.Common（子模块，以 `ProjectReference` 引用，`Private=False` 表示不复制到输出）
- WPF（`UseWPF=true`）

## Release

通过 GitHub Actions 自动发布。推送 `v*` 标签触发构建，打包为 `.qlplugin` 文件（本质是 zip）并创建 GitHub Release。
