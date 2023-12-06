using System.IO;
using System.Threading;
using UnityEditor;
 
[InitializeOnLoad]
public class FileWatcher
{
    public static string ScriptPath = "./";
    public static bool SetRefresh;
 
    static FileWatcher()
    {
        ThreadPool.QueueUserWorkItem(MonitorDirectory, ScriptPath);
        EditorApplication.update += OnUpdate;
    }
 
    private static void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        SetRefresh = true;
    }
 
    private static void MonitorDirectory(object obj)
    {
        string path = (string)obj;
 
        FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
        fileSystemWatcher.Path = path;
        fileSystemWatcher.IncludeSubdirectories = true;
        fileSystemWatcher.Changed += FileSystemWatcher_Changed;
        fileSystemWatcher.Created += FileSystemWatcher_Changed;
        fileSystemWatcher.Renamed += FileSystemWatcher_Changed;
        fileSystemWatcher.Deleted += FileSystemWatcher_Changed;
        fileSystemWatcher.EnableRaisingEvents = true;
    }
 
    private static void OnUpdate()
    {
        if (!SetRefresh) return;
 
        if (EditorApplication.isCompiling) return;
        if (EditorApplication.isUpdating) return;
 
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport & ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(ScriptPath, ImportAssetOptions.ForceSynchronousImport & ImportAssetOptions.ForceUpdate);
        SetRefresh = false;
    }
}
 