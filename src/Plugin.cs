using System.IO;
using System.Windows;

namespace QuickLook.Plugin.GitFolderViewer;

public sealed class Plugin : IViewer
{
    private GitInfoPanel _panel;

    public int Priority => -1;

    public void Init()
    {
    }

    public bool CanHandle(string path)
    {
        if (!Directory.Exists(path))
            return false;

        string dirName = Path.GetFileName(path);
        if (!string.Equals(dirName, ".git", System.StringComparison.OrdinalIgnoreCase))
            return false;

        return File.Exists(Path.Combine(path, "HEAD"));
    }

    public void Prepare(string path, ContextObject context)
    {
        context.PreferredSize = new Size { Width = 600, Height = 480 };
    }

    public void View(string path, ContextObject context)
    {
        _panel = new GitInfoPanel();

        GitInfo info = GitInfoReader.Read(path);
        _panel.DisplayGitInfo(info);

        context.ViewerContent = _panel;
        context.Title = $".git — {Path.GetDirectoryName(path)}";
        context.IsBusy = false;
    }

    public void Cleanup()
    {
        System.GC.SuppressFinalize(this);

        _panel = null;
    }
}