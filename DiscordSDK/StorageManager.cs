namespace DiscordSDK;

public partial class StorageManager {
    public IEnumerable<FileStat> Files() {
        int            fileCount = this.Count();
        List<FileStat> files     = new();
        for (int i = 0; i < fileCount; i++)
            files.Add(this.StatAt(i));
        return files;
    }
}
