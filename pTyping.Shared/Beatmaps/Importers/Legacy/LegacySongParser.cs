#nullable enable

using Furball.Engine.Engine.Helpers;
using Newtonsoft.Json;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.Beatmaps.Importers.Legacy.Events;
using pTyping.Shared.Events;

namespace pTyping.Shared.Beatmaps.Importers.Legacy;

public static class LegacySongParser {
	public static Beatmap? ParseLegacySong(FileInfo fileInfo, out AsciiUnicodeTuple artist, out string source, out AsciiUnicodeTuple title) {
		string fileContents = File.ReadAllText(fileInfo.FullName);

		fileContents = fileContents.Replace("pTyping.Songs.Events.", "pTyping.Shared.Beatmaps.Importers.Legacy.Events.");
		fileContents = fileContents.Replace("Event, pTyping", "Event, pTyping.Shared");

		LegacySong? legacySong = JsonConvert.DeserializeObject<LegacySong>(
			fileContents,
			new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.Auto
			}
		);
		if (legacySong == null) {
			artist = null;
			source = string.Empty;
			title  = null;
			return null;
		}
		if (fileInfo.DirectoryName == null) {
			artist = null;
			source = string.Empty;
			title  = null;
			return null;
		}
		string audioPath = Path.Combine(fileInfo.DirectoryName, legacySong.AudioPath);

		artist = new AsciiUnicodeTuple(null, legacySong.Artist);
		source = "Unknown";
		title  = new AsciiUnicodeTuple(null, legacySong.Name);

		Beatmap map = new Beatmap {
			Difficulty = {
				Strictness = legacySong.Settings.Strictness
			},
			Id = CryptoHelper.GetMd5(File.ReadAllBytes(fileInfo.FullName)),
			Info = {
				Description    = legacySong.Description,
				Mapper = {
					Username = legacySong.Creator
				},
				DifficultyName = new AsciiUnicodeTuple(legacySong.Difficulty),
				PreviewTime    = legacySong.PreviewPoint
			},
			FileCollection = {
				Audio = new PathHashTuple(legacySong.AudioPath, CryptoHelper.GetMd5(File.ReadAllBytes(audioPath))),
				Background = legacySong.BackgroundPath == null ? null : new PathHashTuple(
					legacySong.BackgroundPath,
					CryptoHelper.GetMd5(File.ReadAllBytes(Path.Combine(fileInfo.DirectoryName, legacySong.BackgroundPath)))
				),
				BackgroundVideo = legacySong.VideoPath == null ? null : new PathHashTuple(
					legacySong.VideoPath,
					CryptoHelper.GetMd5(File.ReadAllBytes(Path.Combine(fileInfo.DirectoryName, legacySong.VideoPath)))
				)
			}
		};
		foreach (LegacyNote legacyNote in legacySong.Notes)
			map.HitObjects.Add(
				new HitObject {
					Color = legacyNote.Color,
					Text  = legacyNote.Text,
					Time  = legacyNote.Time
				}
			);
		foreach (LegacyEvent legacyEvent in legacySong.Events) {
			if (legacyEvent.Type is LegacyEventType.BeatLineBar or LegacyEventType.BeatLineBeat)
				continue;

			double  end  = legacyEvent is LyricEvent lyric ? lyric.EndTime : legacyEvent.Time;
			string? text = legacyEvent is LyricEvent lyric2 ? lyric2.Lyric : null;

			map.Events.Add(
				new Event {
					Type = legacyEvent.Type switch {
						LegacyEventType.Lyric        => EventType.Lyric,
						LegacyEventType.TypingCutoff => EventType.TypingCutoff,
						_                            => throw new ArgumentOutOfRangeException()
					},
					Start = legacyEvent.Time,
					End   = end,
					Text  = text
				}
			);
		}
		foreach (LegacyTimingPoint legacyTimingPoint in legacySong.TimingPoints)
			map.TimingPoints.Add(new TimingPoint(legacyTimingPoint.Time, legacyTimingPoint.Tempo));

		return map;
	}
}
