# 发布脚本 - 根据项目版本创建 tag，并推送触发 GitHub Action 自动发布
# 版本来源: src/QuickLook.Plugin.GitFolderViewer.csproj 的 <Version>
# QuickLook 显示的是 QuickLook.Plugin.Metadata.config 的 <Version>，两者必须一致

$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$projectPath = Join-Path $repoRoot 'src\QuickLook.Plugin.GitFolderViewer.csproj'
$metadataPath = Join-Path $repoRoot 'QuickLook.Plugin.Metadata.config'

Push-Location -LiteralPath $repoRoot
try {
    $version = (dotnet msbuild $projectPath -getProperty:Version -p:PreBuildEvent="" | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw '无法读取项目版本。'
    }

    if ($version -notmatch '^\d+\.\d+\.\d+$') {
        throw "项目版本 '$version' 不是有效的三段式版本号。"
    }

    [xml]$metadata = Get-Content -LiteralPath $metadataPath -Encoding UTF8 -Raw
    $metadataVersion = ([string]$metadata.Metadata.Version).Trim()
    if ($metadataVersion -ne $version) {
        throw "版本不一致: csproj = $version, Metadata.config = $metadataVersion。请先把 QuickLook.Plugin.Metadata.config 的 <Version> 改成 $version。"
    }

    $tag = "v$version"

    if (git tag -l $tag) {
        throw "标签 $tag 已存在。请先提升版本号再发布。"
    }

    Write-Host "准备发布版本: $tag" -ForegroundColor Cyan

    $status = git status --porcelain
    if ($status) {
        Write-Host '警告: 有未提交的更改，请先提交:' -ForegroundColor Yellow
        $status | ForEach-Object { Write-Host $_ }
        $confirm = Read-Host '继续发布? (y/N)'
        if ($confirm -ne 'y') {
            Write-Host '已取消' -ForegroundColor Red
            return
        }
    }

    Write-Host "创建 tag: $tag" -ForegroundColor Green
    git tag -a $tag -m $tag
    if ($LASTEXITCODE -ne 0) {
        throw "创建 tag $tag 失败。"
    }

    git push origin $tag
    if ($LASTEXITCODE -ne 0) {
        throw "推送 tag $tag 失败。本地 tag 已创建，可手动重试: git push origin $tag"
    }

    Write-Host "已推送 tag $tag，GitHub Action 将自动构建并发布。" -ForegroundColor Green

    $remoteUrl = git remote get-url origin
    if ($remoteUrl -match 'github\.com[:/](?<slug>[^/]+/[^/]+?)(?:\.git)?$') {
        Write-Host "查看发布进度: https://github.com/$($Matches['slug'])/actions" -ForegroundColor Cyan
    }
}
finally {
    Pop-Location
}