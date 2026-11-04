# 本地打包脚本 - 构建 Release 并生成可安装的 .qlplugin 文件
# 版本来源: src/QuickLook.Plugin.GitFolderViewer.csproj 的 <Version>
# QuickLook 显示的是 QuickLook.Plugin.Metadata.config 的 <Version>，两者必须一致

param(
    [string]$OutputRoot = (Join-Path $PSScriptRoot '..\publish')
)

$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$projectPath = Join-Path $repoRoot 'src\QuickLook.Plugin.GitFolderViewer.csproj'
$metadataPath = Join-Path $repoRoot 'QuickLook.Plugin.Metadata.config'
$resolvedOutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)

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

Write-Host "构建版本: $version" -ForegroundColor Cyan
dotnet build $projectPath -c Release -p:LangVersion=preview -p:PreBuildEvent=""
if ($LASTEXITCODE -ne 0) {
    throw '构建失败。'
}

$dllPath = Join-Path $repoRoot 'build\Release\QuickLook.Plugin.GitFolderViewer.dll'
if (-not (Test-Path -LiteralPath $dllPath -PathType Leaf)) {
    throw "未找到构建产物: $dllPath"
}

$staging = Join-Path $resolvedOutputRoot 'staging'
if (Test-Path -LiteralPath $staging) {
    Remove-Item -LiteralPath $staging -Recurse -Force
}
New-Item -ItemType Directory -Path $staging -Force | Out-Null

Copy-Item -LiteralPath $dllPath -Destination $staging
Copy-Item -LiteralPath (Join-Path $repoRoot 'src\Translations.config') -Destination $staging
Copy-Item -LiteralPath $metadataPath -Destination $staging

$pluginPath = Join-Path $resolvedOutputRoot "QuickLook.Plugin.GitFolderViewer-$version.qlplugin"
if (Test-Path -LiteralPath $pluginPath) {
    Remove-Item -LiteralPath $pluginPath -Force
}

$tempZip = "$pluginPath.zip"
Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $tempZip -CompressionLevel Optimal
Move-Item -LiteralPath $tempZip -Destination $pluginPath
Remove-Item -LiteralPath $staging -Recurse -Force

$sizeKb = [math]::Round((Get-Item -LiteralPath $pluginPath).Length / 1KB, 1)
Write-Host "打包完成: $pluginPath ($sizeKb KB)" -ForegroundColor Green