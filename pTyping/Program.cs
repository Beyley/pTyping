using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using EeveeTools.Helpers;
using Furball.Engine.Engine.Platform;
using Newtonsoft.Json;

namespace pTyping;

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

	private static string ReadManifestResource(string name) {
		using Stream stream = Assembly.GetAssembly(typeof(Program))?.GetManifestResourceStream(name);

		using StreamReader reader = new StreamReader(stream!);

		return reader.ReadToEnd().Trim();
	}

	public static List<GitLogEntry> GetGitLog() {
		string gitlog = ReadManifestResource("pTyping.gitlog.json");

		//evil hack to get around evil commit messages
		gitlog = gitlog.Replace("\"", "\\\"");
		gitlog = gitlog.Replace("@^^ABBA^^@", "\"");

		return JsonConvert.DeserializeObject<List<GitLogEntry>>(gitlog);
	}

	[STAThread]
	private static void Main() {
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		
		SetReleaseStream();
		GitVersion = ReadManifestResource("pTyping.gitversion.txt");
		
		using pTypingGame game = new pTypingGame();
		
		if (RuntimeInfo.IsDebug())
			game.Run();
		else
			try {
				game.Run();
			}
			catch (Exception ex) {
				using FileStream   stream = File.Create($"crashlog-{UnixTime.Now()}");
				using StreamWriter writer = new StreamWriter(stream);
		
				writer.Write(ex.ToString());
				writer.Flush();

				writer.Close();
		
				game.WindowManager.Close();
			}
	}
}
