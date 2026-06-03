using QuickLook.Common.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace QuickLook.Plugin.GitFolderViewer;

public partial class GitInfoPanel : UserControl, INotifyPropertyChanged
{
    private string _branch;
    private string _trackingBranch;
    private string _remoteUrl;
    private string _headCommit;
    private string _branchInfo;
    private string _lastCommitMessage;
    private List<string> _recentLog;

    // Labels (translated)
    private string _labelTrackingBranch;
    private string _labelRemoteUrl;
    private string _labelBranches;
    private string _labelLastCommit;
    private string _labelRecentActivity;

    public string Branch
    {
        get => _branch;
        set { _branch = value; OnPropertyChanged(nameof(Branch)); }
    }

    public string TrackingBranch
    {
        get => _trackingBranch;
        set { _trackingBranch = value; OnPropertyChanged(nameof(TrackingBranch)); }
    }

    public string RemoteUrl
    {
        get => _remoteUrl;
        set { _remoteUrl = value; OnPropertyChanged(nameof(RemoteUrl)); }
    }

    public string HeadCommit
    {
        get => _headCommit;
        set { _headCommit = value; OnPropertyChanged(nameof(HeadCommit)); }
    }

    public string BranchInfo
    {
        get => _branchInfo;
        set { _branchInfo = value; OnPropertyChanged(nameof(BranchInfo)); }
    }

    public string LastCommitMessage
    {
        get => _lastCommitMessage;
        set { _lastCommitMessage = value; OnPropertyChanged(nameof(LastCommitMessage)); }
    }

    public List<string> RecentLog
    {
        get => _recentLog;
        set { _recentLog = value; OnPropertyChanged(nameof(RecentLog)); }
    }

    public string LabelTrackingBranch
    {
        get => _labelTrackingBranch;
        set { _labelTrackingBranch = value; OnPropertyChanged(nameof(LabelTrackingBranch)); }
    }

    public string LabelRemoteUrl
    {
        get => _labelRemoteUrl;
        set { _labelRemoteUrl = value; OnPropertyChanged(nameof(LabelRemoteUrl)); }
    }

    public string LabelBranches
    {
        get => _labelBranches;
        set { _labelBranches = value; OnPropertyChanged(nameof(LabelBranches)); }
    }

    public string LabelLastCommit
    {
        get => _labelLastCommit;
        set { _labelLastCommit = value; OnPropertyChanged(nameof(LabelLastCommit)); }
    }

    public string LabelRecentActivity
    {
        get => _labelRecentActivity;
        set { _labelRecentActivity = value; OnPropertyChanged(nameof(LabelRecentActivity)); }
    }

    private static string Domain => Assembly.GetExecutingAssembly().GetName().Name;

    public GitInfoPanel()
    {
        InitializeComponent();
        DataContext = this;

        LabelTrackingBranch = TranslationHelper.Get("GitTrackingBranch", domain: Domain);
        LabelRemoteUrl = TranslationHelper.Get("GitRemoteUrl", domain: Domain);
        LabelBranches = TranslationHelper.Get("GitBranches", domain: Domain);
        LabelLastCommit = TranslationHelper.Get("GitLastCommit", domain: Domain);
        LabelRecentActivity = TranslationHelper.Get("GitRecentActivity", domain: Domain);
    }

    public void DisplayGitInfo(GitInfo info)
    {
        string noTracking = TranslationHelper.Get("GitNoTracking", domain: Domain);
        string noRemote = TranslationHelper.Get("GitNoRemote", domain: Domain);

        Branch = info.IsDetached
            ? $"DETACHED HEAD @ {info.Branch}"
            : info.Branch ?? "";

        if (!string.IsNullOrEmpty(info.TrackingRemote) && !string.IsNullOrEmpty(info.TrackingBranch))
        {
            string branchShort = info.TrackingBranch;
            if (branchShort.StartsWith("refs/heads/"))
                branchShort = branchShort.Substring(11);
            TrackingBranch = $"{info.TrackingRemote}/{branchShort}";
        }
        else
        {
            TrackingBranch = noTracking;
        }

        RemoteUrl = info.RemoteUrls.FirstOrDefault() ?? noRemote;
        HeadCommit = info.HeadCommitHash ?? "";
        BranchInfo = $"{info.LocalBranchCount} local, {info.RemoteBranchCount} remote";
        LastCommitMessage = info.LastCommitMessage ?? "";
        RecentLog = info.RecentLog;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
