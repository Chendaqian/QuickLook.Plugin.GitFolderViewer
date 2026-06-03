using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuickLook.Plugin.GitFolderViewer;

public class GitInfo
{
    public string Branch { get; set; }
    public bool IsDetached { get; set; }
    public string HeadCommitHash { get; set; }
    public string TrackingRemote { get; set; }
    public string TrackingBranch { get; set; }
    public List<string> RemoteUrls { get; set; } = new List<string>();
    public string LastCommitMessage { get; set; }
    public int LocalBranchCount { get; set; }
    public int RemoteBranchCount { get; set; }
    public List<string> RecentLog { get; set; } = new List<string>();
}

internal static class GitInfoReader
{
    public static GitInfo Read(string gitPath)
    {
        GitInfo info = new GitInfo();
        ReadHead(gitPath, info);
        ReadConfig(gitPath, info);
        ResolveHeadCommit(gitPath, info);
        info.LastCommitMessage = ReadLastCommitMessage(gitPath);
        info.LocalBranchCount = CountBranches(Path.Combine(gitPath, "refs", "heads"));
        info.RemoteBranchCount = CountRemoteBranches(Path.Combine(gitPath, "refs", "remotes"));
        info.RecentLog = ReadRecentLog(gitPath, 5);
        return info;
    }

    private static void ReadHead(string gitPath, GitInfo info)
    {
        string headFile = Path.Combine(gitPath, "HEAD");
        if (!File.Exists(headFile))
            return;

        string content = File.ReadAllText(headFile).Trim();

        if (content.StartsWith("ref: "))
        {
            string refPath = content.Substring(5).Trim();
            if (refPath.StartsWith("refs/heads/"))
                info.Branch = refPath.Substring(11);
            else
                info.Branch = refPath;

            info.IsDetached = false;
        }
        else
        {
            info.HeadCommitHash = content;
            info.IsDetached = true;
            info.Branch = content.Length > 8
                ? content.Substring(0, 8)
                : content;
        }
    }

    private static void ResolveHeadCommit(string gitPath, GitInfo info)
    {
        if (info.IsDetached)
            return;

        string headFile = Path.Combine(gitPath, "HEAD");
        if (!File.Exists(headFile))
            return;

        string content = File.ReadAllText(headFile).Trim();
        if (!content.StartsWith("ref: "))
            return;

        string refPath = content.Substring(5).Trim();
        string refFile = Path.Combine(gitPath, refPath.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(refFile))
        {
            info.HeadCommitHash = File.ReadAllText(refFile).Trim();
        }
        else
        {
            string packedRefs = Path.Combine(gitPath, "packed-refs");
            if (File.Exists(packedRefs))
            {
                foreach (string line in File.ReadAllLines(packedRefs))
                {
                    if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] parts = line.Split(new[] { ' ' }, 2);
                    if (parts.Length == 2 && parts[1] == refPath)
                    {
                        info.HeadCommitHash = parts[0];
                        break;
                    }
                }
            }
        }
    }

    private static void ReadConfig(string gitPath, GitInfo info)
    {
        string configFile = Path.Combine(gitPath, "config");
        if (!File.Exists(configFile))
            return;

        string[] lines = File.ReadAllLines(configFile);
        string currentSection = "";
        string currentRemoteName = null;
        string currentBranchName = null;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                currentSection = line.Substring(1, line.Length - 2).ToLower();
                currentRemoteName = null;
                currentBranchName = null;

                if (currentSection.StartsWith("remote "))
                    currentRemoteName = ExtractQuotedValue(currentSection, "remote");
                else if (currentSection.StartsWith("branch "))
                    currentBranchName = ExtractQuotedValue(currentSection, "branch");

                continue;
            }

            if (line.StartsWith("url = ") && currentRemoteName != null)
            {
                string url = line.Substring(6).Trim();
                info.RemoteUrls.Add(url);
            }

            if (currentBranchName != null && info.Branch != null &&
                currentBranchName == info.Branch)
            {
                if (line.StartsWith("remote = "))
                    info.TrackingRemote = line.Substring(9).Trim();
                else if (line.StartsWith("merge = "))
                    info.TrackingBranch = line.Substring(8).Trim();
            }
        }
    }

    private static string ExtractQuotedValue(string section, string key)
    {
        int start = section.IndexOf('"');
        int end = section.LastIndexOf('"');
        if (start >= 0 && end > start)
            return section.Substring(start + 1, end - start - 1);

        return null;
    }

    private static string ReadLastCommitMessage(string gitPath)
    {
        string msgFile = Path.Combine(gitPath, "COMMIT_EDITMSG");
        if (!File.Exists(msgFile))
            return null;

        try
        {
            string content = File.ReadAllText(msgFile).Trim();
            string firstLine = content.Split(new[] { '\r', '\n' }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return firstLine;
        }
        catch
        {
            return null;
        }
    }

    private static int CountBranches(string headsPath)
    {
        if (!Directory.Exists(headsPath))
            return 0;

        int count = 0;
        try
        {
            foreach (string file in Directory.GetFiles(headsPath, "*", SearchOption.AllDirectories))
            {
                if (new FileInfo(file).Length > 0)
                    count++;
            }
        }
        catch { }

        return count;
    }

    private static int CountRemoteBranches(string remotesPath)
    {
        if (!Directory.Exists(remotesPath))
            return 0;

        int count = 0;
        try
        {
            foreach (string dir in Directory.GetDirectories(remotesPath))
            {
                foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    string name = Path.GetFileName(file);
                    if (name != "HEAD")
                        count++;
                }
            }
        }
        catch { }

        return count;
    }

    private static List<string> ReadRecentLog(string gitPath, int maxEntries)
    {
        List<string> entries = new List<string>();
        string logFile = Path.Combine(gitPath, "logs", "HEAD");
        if (!File.Exists(logFile))
            return entries;

        try
        {
            string[] lines = File.ReadAllLines(logFile);
            int start = Math.Max(0, lines.Length - maxEntries);

            for (int i = lines.Length - 1; i >= start; i--)
            {
                string line = lines[i];
                int tabIdx = line.IndexOf('\t');
                if (tabIdx < 0)
                    continue;

                string message = line.Substring(tabIdx + 1).Trim();
                string commitInfo = line.Substring(0, tabIdx);

                int authorEnd = commitInfo.IndexOf(">");
                if (authorEnd > 0)
                {
                    int authorStart = commitInfo.LastIndexOf("<");
                    if (authorStart > 0)
                    {
                        int spaceBefore = commitInfo.LastIndexOf(' ', authorStart);
                        string author = spaceBefore >= 0
                            ? commitInfo.Substring(spaceBefore + 1, authorEnd - spaceBefore)
                            : commitInfo.Substring(authorStart, authorEnd - authorStart + 1);
                        entries.Add($"{message} — {author}");
                        continue;
                    }
                }

                entries.Add(message);
            }
        }
        catch { }

        return entries;
    }
}
