using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using EeveeTools.Helpers;
using Furball.Engine.Engine.Platform;
using Newtonsoft.Json;
using Silk.NET.Windowing;

namespace pTyping;

internal class Program {
    public static List<GitLogEntry> GitLog;

    public static string GitVersion    = "unknown";
    public static string ReleaseStream = "other";
    public static string BuildVersion => $"{GitVersion}-{ReleaseStream}";

    private static void SetReleaseStream() {
        StreamDebug();
        StreamRelease();
    }

    [Conditional("DEBUG")]
    private static void StreamDebug() {
        ReleaseStream = "debug";
    }

    [Conditional("RELEASE")]
    private static void StreamRelease() {
        ReleaseStream = "release";
    }

    [STAThread]
    private static void Main(string[] args) {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        SetReleaseStream();
        using (Stream stream = Assembly.GetAssembly(typeof(Program))?.GetManifestResourceStream("pTyping.gitversion.txt")) {
            using (StreamReader reader = new(stream ?? throw new Exception("Somehow the code we are executing is not in an assembly?"))) {
                GitVersion = reader.ReadToEnd().Trim();
            }
        }

        using (Stream stream = Assembly.GetAssembly(typeof(Program))?.GetManifestResourceStream("pTyping.gitlog.json")) {
            using (StreamReader reader = new(stream ?? throw new Exception("Somehow the code we are executing is not in an assembly?"))) {
                string gitlog = reader.ReadToEnd().Trim();

                //evil hack to get around evil commit messages
                gitlog = gitlog.Replace("\"",         "\\\"");
                gitlog = gitlog.Replace("@^^ABBA^^@", "\"");

                GitLog = JsonConvert.DeserializeObject<List<GitLogEntry>>(gitlog);
            }
        }

        using pTypingGame game = new();

        WindowOptions options = WindowOptions.Default with {
            VSync = false,
            WindowBorder = WindowBorder.Fixed
        };
        if (RuntimeInfo.IsDebug())
            game.Run(options);
        else
            try {
                game.Run(options);
            }
            catch (Exception ex) {
                using FileStream   stream = File.Create($"crashlog-{UnixTime.Now()}");
                using StreamWriter writer = new(stream);

                writer.Write(ex.ToString());

                game.WindowManager.Close();
            }
    }
}
