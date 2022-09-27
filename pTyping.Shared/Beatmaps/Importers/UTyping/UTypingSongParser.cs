#nullable enable
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using Furball.Engine.Engine.Helpers;
using Furball.Vixie.Backends.Shared;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.Events;
using pTyping.Shared.Scores;

namespace pTyping.Shared.Beatmaps.Importers.UTyping;

public static class UTypingSongParser {
	public static Beatmap? ParseUTypingBeatmapAndScores(FileInfo fileInfo, out AsciiUnicodeTuple artist, out string source, out AsciiUnicodeTuple title, ScoreDatabase scoreDatabase) {
		Beatmap beatmap = new Beatmap();

		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		byte[] fullFile = File.ReadAllBytes(fileInfo.FullName);

		beatmap.Id = CryptoHelper.GetMd5(fullFile);

		string infoData = Encoding.GetEncoding(932).GetString(fullFile);

		infoData = infoData.Replace("\r", "");
		string[] info = infoData.Split("\n");

		title                       = new AsciiUnicodeTuple(null, info[0]);
		artist                      = new AsciiUnicodeTuple(null, info[1]);
		beatmap.Info.Mapper         = info[2];
		beatmap.Info.DifficultyName = new AsciiUnicodeTuple(null, info[3]);

		source = "UTyping";

		string fumenFilename = info[4];

		//Parse the score file
		ParseUTypingScores(Path.Combine(fileInfo.DirectoryName!, info[5]), scoreDatabase, beatmap);

		string[] descSplit = info[6..];

		beatmap.Info.Description = string.Join("\n", descSplit);

		string mapData = Encoding.GetEncoding(932).GetString(File.ReadAllBytes(Path.Combine(fileInfo.DirectoryName!, fumenFilename)));

		//If the first char of the map is not the song filename, panic out
		//TODO: do all UTyping songs have the song filename first?
		if (mapData[0] != '@') return null;

		double? firstBeatlineBar = null;

		TimingPoint? timingPoint = null;

		using StringReader reader = new StringReader(mapData);

		string? line;
		do {
			line = reader.ReadLine();

			//If we reached the end of the string or are on an empty line, skip to the next line...
			if (line == null || line.Trim() == string.Empty)
				continue;

			//Get the first char of the line
			string firstChar = line[0].ToString();

			//Get the rest of the line
			line = line[1..];

			switch (firstChar) {
				//Contains the relative path to the song file in the format of
				//@path
				//ex. @animariot.ogg
				case "@": {
					string fullAudioPath = Path.Combine(fileInfo.DirectoryName!, line);

					if (!File.Exists(fullAudioPath))
						return null;

					beatmap.FileCollection.Audio = new PathHashTuple(line, CryptoHelper.GetMd5(File.ReadAllBytes(fullAudioPath)));
					break;
				}
				//Contains a note in the format of
				//TimeInSeconds CharacterToType
				//ex. +109.041176 だい
				case "+": {
					string[] splitLine = line.Split(" ");

					double time = double.Parse(splitLine[0]) * 1000;
					string text = splitLine[1];

					beatmap.HitObjects.Add(
						new HitObject {
							Text  = text,
							Time  = time,
							Color = Color.Red
						}
					);

					break;
				}
				//Contains the next set of lyrics in the format of
				//*TimeInSeconds Lyrics
				//ex. *16.100000 だいてだいてだいてだいて　もっと
				case "*": {
					string[] splitLine = line.Split(" ");

					double time = double.Parse(splitLine[0]) * 1000d;
					string text = string.Join(' ', splitLine[1..]);

					if (beatmap.Events.Any(x => x.Type == EventType.Lyric)) {
						Event ev = beatmap.Events.Last(x => x.Type == EventType.Lyric);

						// ReSharper disable once CompareOfFloatsByEqualityOperator
						if (ev.End == double.PositiveInfinity)
							ev.End = time;
					}

					beatmap.Events.Add(
						new Event {
							Start = time,
							End   = double.PositiveInfinity,
							Text  = text.Trim(),
							Type  = EventType.Lyric
						}
					);

					break;
				}
				//Prevents you from typing the previous note, and stops the currently lyric
				// /TimeInSeconds
				//ex. /17.982353
				case "/": {
					double time = double.Parse(line) * 1000d;

					beatmap.Events.Add(
						new Event {
							Start = time,
							End   = time,
							Type  = EventType.TypingCutoff
						}
					);

					if (beatmap.Events.Any(x => x.Type == EventType.Lyric)) {
						Event ev = beatmap.Events.Last(x => x.Type == EventType.Lyric);

						// ReSharper disable once CompareOfFloatsByEqualityOperator
						if (ev.End == double.PositiveInfinity)
							ev.End = time;
					}

					break;
				}
				//A beatline beat (happens every 1/4th beat except for full beats)
				//-TimeInSeconds
				//ex. -17.747059
				case "-": {
					// song.Events.Add(
					// new BeatLineBeatEvent {
					//     Time = double.Parse(line) * 1000d
					// }
					// );

					//We ignore these events for now...
					break;
				}
				//A beatline bar (happens every full beat)
				//=TimeInSeconds
				//ex. =4.544444
				case "=": {
					if (timingPoint != null)
						break;

					//This is null if we are at the first beatline bar
					if (firstBeatlineBar == null) {
						//We found the first beatline bar, so save the time
						firstBeatlineBar = double.Parse(line) * 1000d;
						break;
					}

					double thisBeatLineBarTime = double.Parse(line) * 1000d;

					//The tempo of the song is the 
					double tempo = thisBeatLineBarTime - firstBeatlineBar.Value;

					timingPoint = new TimingPoint(firstBeatlineBar.Value, tempo / 4d);

					break;
				}
			}
		} while (line != null);

		if (timingPoint != null)
			beatmap.TimingPoints.Add(timingPoint);

		return beatmap;
	}

	private const int SCORE_FILE_VERSION = 5;
	private const int RANKING_LEN        = 20;

	private const int NAME_LEN     = 16;
	private const int OLD_NAME_LEN = 8;

	private enum ScoreAchievement {
		NoData     = 0x0000, /* フルコンボか以外はスコアから読み取れないため */
		Failed     = 0x0100,
		RedZone    = 0x0200,
		YellowZone = 0x0240,
		BlueZone   = 0x0280,
		Clear      = 0x0300,
		FullCombo  = 0x0500,
		FullGood   = 0x0600,
		Perfect    = 0x0700
	}

	private enum ScoreChallengeType {
		Hidden,        // hide
		Sudden,        // suddenly appear 
		Stealth,       // cant see 
		LyricsStealth, // cant see the lyrics
		Sin,
		Cos,
		Tan,
		Num
	}

	private class ScoreChallenge {
		public BitVector32 Type;
		public bool        Valid;
		[Description("The speed of the challenge, which in this case is 10x the actual speed, due to this being an int, to get back the float speed, divide by 10f")]
		public int Speed; //Speed is 10x the actual speed, due to this being an int
		public int Key;

		public static ScoreChallenge Read(BinaryReader reader) {
			ScoreChallenge challenge = new ScoreChallenge();

			//Read all the types
			for (int i = 0; i < (int)ScoreChallengeType.Num; i++)
				challenge.Type[i] = reader.ReadByte() != 0;
			//The types are padded to 16 bytes, so read the rest of them
			for (int i = (int)ScoreChallengeType.Num; i < 16; i++)
				reader.ReadByte();

			challenge.Speed = reader.ReadInt32();
			challenge.Key   = reader.ReadInt32();

			challenge.Valid = challenge.Speed != 0;

			return challenge;
		}
	}

	private class UTypingScore {
		public string Name;
		public int    Score;
		public int    ScoreAccuracy;
		public int    ScoreTyping;
		public int    CountExcellent;
		public int    CountGood;
		public int    CountFair;
		public int    CountPoor;
		public int    CountPass;
		public int    CountAll;
		public int    ComboMax;
		public int    Date;

		public DateTimeOffset RealDate {
			get {
				int day   = this.Date & 0x000000FF;
				int month = (this.Date & 0x0000FF00) >> 8;
				int year  = (int)((this.Date & 0xFFFF0000) >> 16);

				return new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
			}
		}

		public ScoreChallenge Challenge;

		public ScoreAchievement GetLevel() {
			if (this.ComboMax  != -1) return ScoreAchievement.NoData;
			if (this.CountFair != 0) return ScoreAchievement.FullCombo;
			if (this.CountGood != 0) return ScoreAchievement.FullGood;
			return ScoreAchievement.Perfect;
		}

		public static UTypingScore Read(BinaryReader reader, int version) {
			UTypingScore score = new UTypingScore();

			//In version 0, names were stored in 8 bytes, after, they were stored in 16
			byte[] nameBytes = reader.ReadBytes(version >= 1 ? NAME_LEN : OLD_NAME_LEN);

			//Should this be in ShiftJIS? Not sure, using ASCII for now
			score.Name = Encoding.ASCII.GetString(nameBytes);

			score.Score         = reader.ReadInt32();
			score.ScoreAccuracy = reader.ReadInt32();
			score.ScoreTyping   = reader.ReadInt32();

			score.CountExcellent = reader.ReadInt32();
			score.CountGood      = reader.ReadInt32();
			score.CountFair      = reader.ReadInt32();
			score.CountPoor      = reader.ReadInt32();
			score.CountPass      = reader.ReadInt32();

			score.CountAll = reader.ReadInt32();
			score.ComboMax = reader.ReadInt32();

			//Challenge info was only added in version 1
			if (version >= 1)
				score.Challenge = ScoreChallenge.Read(reader);

			//Date was only added in version 2
			if (version >= 2)
				score.Date = reader.ReadInt32();

			return score;
		}
	}

	private class Ranking {
		public          bool             Changed;
		public readonly UTypingScore?[]  Score       = new UTypingScore?[RANKING_LEN];
		public          ScoreAchievement Achievement = ScoreAchievement.NoData;
		public          int              PlayCount;
		public          int              PlayTime;

		public static Ranking Read(FileStream stream) {
			using BinaryReader reader = new BinaryReader(stream);

			Ranking ranking = new Ranking();

			//If the score database is an empty file, then just ignore it
			if (stream.Length == 0)
				return ranking;

			int version = reader.ReadInt32();

			if ((version & 0xFF) > ' ') { //Hack for old formats(?)
				version         = 0;
				stream.Position = 0;
			}

			//If version is too new or invalid, then skip the score file
			if (version is > SCORE_FILE_VERSION or < 0)
				return ranking; //TODO: notify the user something went wrong reading the score file

			//Score achievement was only added in version 4
			if (version >= 4)
				ranking.Achievement = (ScoreAchievement)reader.ReadInt32();

			//Play count and time was only added in version 3
			if (version >= 3) {
				ranking.PlayCount = reader.ReadInt32();
				ranking.PlayTime  = reader.ReadInt32();
			}

			int rankingSize = RANKING_LEN;

			//variable ranking size was only added in version 5
			if (version >= 5)
				rankingSize = reader.ReadInt32();

			for (int i = 0; i < rankingSize; i++)
				ranking.Score[i] = UTypingScore.Read(reader, version);

			if (version < 4)
				for (int i = 0; i < rankingSize; i++) {
					ScoreAchievement l = ranking.Score[0]!.GetLevel();
					if (l > ranking.Achievement)
						ranking.Achievement = (ScoreAchievement)1;
				}

			return ranking;
		}
	}

	private static void ParseUTypingScores(string filePath, ScoreDatabase scoreDatabase, Beatmap beatmap) {
		//If the score file does not exist, ignore
		if (!File.Exists(filePath))
			return;

		using FileStream stream = File.OpenRead(filePath);

		Ranking ranking = Ranking.Read(stream);

		scoreDatabase.Realm.Write(() => {
			foreach (UTypingScore? uTypingScore in ranking.Score) {
				if (uTypingScore == null)
					continue;

				Score score = new Score {
					User = {
						Username = uTypingScore.Name
					},
					AchievedScore = uTypingScore.Score,
					BeatmapId     = beatmap.Id,
					MaxCombo      = uTypingScore.ComboMax,
					ExcellentHits = uTypingScore.CountExcellent,
					FairHits      = uTypingScore.CountFair,
					GoodHits      = uTypingScore.CountGood,
					PoorHits      = uTypingScore.CountPoor,
					OnlineScore   = false,
					Accuracy      = -1f,
					Time          = uTypingScore.RealDate
				};

				scoreDatabase.Realm.Add(score);
			}
		});
	}
}
