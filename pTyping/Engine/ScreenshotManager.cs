using System.IO;
using Furball.Engine;
using SixLabors.ImageSharp;

namespace pTyping.Engine;

public static class ScreenshotManager {
    public const string SCREENSHOT_DIR        = "screenshots";
    public const string ONLINE_SCREENSHOT_DIR = "online-screenshots";

    public static string ResolvedScreenshotPath       => Path.Combine(FurballGame.AssemblyPath, SCREENSHOT_DIR);
    public static string ResolvedOnlineScreenshotPath => Path.Combine(FurballGame.AssemblyPath, ONLINE_SCREENSHOT_DIR);

    public static void Initialize() {
        if (!Directory.Exists(ResolvedScreenshotPath))
            Directory.CreateDirectory(ResolvedScreenshotPath);
        if (!Directory.Exists(ResolvedOnlineScreenshotPath))
            Directory.CreateDirectory(ResolvedOnlineScreenshotPath);
    }

    public static string SaveScreenshot(Image img, bool online, string id = null) {
        DirectoryInfo info = new(ResolvedScreenshotPath);

        FileInfo[] files = info.GetFiles();

        string path = online ? Path.Combine(ResolvedOnlineScreenshotPath, $"{id}.png") : Path.Combine(ResolvedScreenshotPath, $"screenshot-{files.Length}.png");

        img.SaveAsPng(path);

        return path;
    }
}
