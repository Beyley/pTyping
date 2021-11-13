using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace pTyping {
    internal class Program {
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

        private static void Main(string[] args) {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            SetReleaseStream();
            using (Stream stream = Assembly.GetAssembly(typeof(Program))?.GetManifestResourceStream("pTyping.gitversion.txt")) {
                using (StreamReader reader = new(stream ?? throw new Exception("Somehow the code we are executing is not in an assembly?"))) {
                    GitVersion = reader.ReadToEnd().Trim();
                }
            }

            using pTypingGame game = new();
            game.Run();
        }
    }
}
