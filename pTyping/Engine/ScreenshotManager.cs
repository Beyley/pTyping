using System.IO;
using Furball.Engine;
using SixLabors.ImageSharp;

namespace pTyping.Engine;

public static class ScreenshotManager {
    public const string SCREENSHOT_DIR = "screenshots";

    public static string ResolvedScreenshotPath => Path.Combine(FurballGame.AssemblyPath, SCREENSHOT_DIR);

    public static void Initialize() {
        if (!Directory.Exists(ResolvedScreenshotPath))
            Directory.CreateDirectory(ResolvedScreenshotPath);
    }

    public static string SaveScreenshot(Image img) {
        DirectoryInfo info = new(ResolvedScreenshotPath);

        FileInfo[] files = info.GetFiles();

        string finalPath = Path.Combine(ResolvedScreenshotPath, $"screenshot-{files.Length}.png");

        img.SaveAsPng(finalPath);

        return finalPath;
    }
}
