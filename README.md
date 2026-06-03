# QuickLook.Plugin.GitFolderViewer

[![Build and Release](https://github.com/Chendaqian/QuickLook.Plugin.GitFolderViewer/actions/workflows/release.yml/badge.svg)](https://github.com/Chendaqian/QuickLook.Plugin.GitFolderViewer/actions/workflows/release.yml)
[![GitHub Release](https://img.shields.io/github/v/release/Chendaqian/QuickLook.Plugin.GitFolderViewer)](https://github.com/Chendaqian/QuickLook.Plugin.GitFolderViewer/releases/latest)

A [QuickLook](https://github.com/QL-Win/QuickLook) plugin that lets you preview `.git` folders with a single press of the spacebar.

## Features

Press spacebar on any `.git` folder in Windows Explorer to see:

- **Current branch** (or detached HEAD state)
- **Tracking remote branch** (e.g., `origin/master`)
- **Remote URL** (e.g., `https://github.com/...`)
- **HEAD commit hash**
- **Branch count** (local + remote)
- **Last commit message**
- **Recent activity** (last 5 reflog entries)

All values are copyable — just click and select.

### Localization

- English (default)
- Chinese (zh-CN) — automatically applied when system language is Chinese

## Installation

### From Releases (Recommended)

1. Go to [Releases](https://github.com/Chendaqian/QuickLook.Plugin.GitFolderViewer/releases/latest)
2. Download the `.qlplugin` file
3. Double-click the file — QuickLook will install it automatically

### Manual

1. Download the `.qlplugin` file from [Releases](https://github.com/Chendaqian/QuickLook.Plugin.GitFolderViewer/releases/latest)
2. Rename `.qlplugin` to `.zip`
3. Extract to `%LocalAppData%\QuickLook\QuickLook.Plugin\QuickLook.Plugin.GitFolderViewer\`
4. Restart QuickLook

### Build from Source

```bash
git clone https://github.com/Chendaqian/QuickLook.Plugin.GitFolderViewer.git
cd QuickLook.Plugin.GitFolderViewer

# Requires QuickLook source at ../QuickLook (sibling directory)
dotnet build src/QuickLook.Plugin.GitFolderViewer.csproj -c Release -p:LangVersion=preview
```

Output: `build/Release/QuickLook.Plugin.GitFolderViewer.dll`

Copy the DLL and `Translations.config` to the QuickLook plugin directory.

## How It Works

This plugin reads git data directly from `.git` folder files — no `git.exe` required:

| File | Data |
|------|------|
| `HEAD` | Current branch or detached commit |
| `config` | Remote URLs, tracking branch config |
| `refs/heads/` | Branch count, HEAD commit hash |
| `refs/remotes/` | Remote branch count |
| `COMMIT_EDITMSG` | Last commit message |
| `logs/HEAD` | Recent reflog activity |
| `packed-refs` | Fallback for packed refs |

## Requirements

- [QuickLook](https://github.com/QL-Win/QuickLook) 3.7+
- .NET Framework 4.6.2

## License

[GPL-3.0](LICENSE)
