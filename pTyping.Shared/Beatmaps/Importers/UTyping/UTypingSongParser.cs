using System.Text;
using Furball.Engine.Engine.Helpers;
using Furball.Vixie.Backends.Shared;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.Events;

namespace pTyping.Shared.Beatmaps.Importers.UTyping;

public class UTypingSongParser {
    public static Beatmap? ParseUTypingBeatmap(FileInfo fileInfo) {
        Beatmap beatmap = new();

        string infoData = Encoding.GetEncoding(932).GetString(File.ReadAllBytes(fileInfo.FullName));

        infoData = infoData.Replace("\r", "");
        string[] info = infoData.Split("\n");

        beatmap.Info.Title          = new AsciiUnicodeTuple(null, info[0]);
        beatmap.Info.Artist         = new AsciiUnicodeTuple(null, info[1]);
        beatmap.Info.Mapper         = info[2];
        beatmap.Info.DifficultyName = new AsciiUnicodeTuple(null, info[3]);

        string fumenFilename = info[4];

        //info[5] is just the score filename, which we dont parse
        //all lines after info[5] are description

        string[] descSplit = info[6..];

        beatmap.Info.Description = string.Join("\n", descSplit);

        string mapData = Encoding.GetEncoding(932).GetString(File.ReadAllBytes(Path.Combine(fileInfo.DirectoryName!, fumenFilename)));

        //If the first char of the map is not the song filename, panic out
        //TODO: do all UTyping songs have the song filename first?
        if (mapData[0] != '@') return null;

        List<HitObject> hitObjects = new();
        List<Event>     events     = new();

        double?     firstBeatlineBar = null;
        TimingPoint timingPoint      = new(0, 0);

        using StringReader reader = new(mapData);

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
                        throw new Exception();

                    beatmap.FileCollection.AudioHash = new PathHashTuple(line, CryptoHelper.GetMd5(File.ReadAllBytes(fullAudioPath)));
                    break;
                }
                //Contains a note in the format of
                //TimeInSeconds CharacterToType
                //ex. +109.041176 だい
                case "+": {
                    string[] splitLine = line.Split(" ");

                    double time = double.Parse(splitLine[0]) * 1000;
                    string text = splitLine[1];

                    hitObjects.Add(
                    new LyricHitObject {
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

                    if (events.Any(x => x is LyricEvent)) {
                        LyricEvent ev = (LyricEvent)events.Last(x => x is LyricEvent);

                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        if (ev.End == double.PositiveInfinity)
                            ev.End = time;
                    }

                    events.Add(
                    new LyricEvent(text.Trim()) {
                        Start = time,
                        End   = double.PositiveInfinity
                    }
                    );

                    break;
                }
                //Prevents you from typing the previous note, and stops the currently lyric
                // /TimeInSeconds
                //ex. /17.982353
                case "/": {
                    double time = double.Parse(line) * 1000d;

                    events.Add(
                    new TypingCutoffEvent {
                        Start = time,
                        End   = time
                    }
                    );

                    if (events.Any(x => x is LyricEvent)) {
                        LyricEvent ev = (LyricEvent)events.Last(x => x is LyricEvent);

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
                    // song.Events.Add(
                    // new BeatLineBarEvent {
                    //     Time = double.Parse(line) * 1000d
                    // }
                    // );

                    if (firstBeatlineBar == null) {
                        firstBeatlineBar = double.Parse(line) * 1000d;
                        break;
                    }
                    timingPoint = new TimingPoint(firstBeatlineBar.Value, (double.Parse(line) * 1000d - firstBeatlineBar.Value) / 4d);

                    break;
                }
            }
        } while (line != null);

        beatmap.HitObjects = hitObjects;
        beatmap.Events     = events;

        beatmap.TimingPoints = new[] {
            timingPoint
        };

        return beatmap;
    }
}
