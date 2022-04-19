using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Furball.Vixie.Backends.Shared;
using Kettu;
using pTyping.Engine;
using pTyping.Songs.Events;

namespace pTyping.Songs.SongLoaders;

public class UTypingSongHandler : ISongHandler {
    public SongType Type => SongType.UTyping;

    public Song LoadSong(FileInfo fileInfo) {
        Song song = new() {
            Name       = "",
            Artist     = "",
            Creator    = "",
            Difficulty = "",
            FileInfo   = fileInfo
        };

        string infoData = Encoding.GetEncoding(932).GetString(File.ReadAllBytes(fileInfo.FullName));

        infoData = infoData.Replace("\r", "");
        string[] info = infoData.Split("\n");

        song.Name       = info[0];
        song.Artist     = info[1];
        song.Creator    = info[2];
        song.Difficulty = info[3];
        song.Type       = this.Type;

        string dataFilename = info[4];

        string mapData = Encoding.GetEncoding(932).GetString(File.ReadAllBytes(Path.Combine(fileInfo.DirectoryName!, dataFilename)));

        if (mapData[0] != '@') return null;

        using StringReader reader = new(mapData);
        string             line;
        do {
            line = reader.ReadLine();

            if (line == null || line.Trim() == string.Empty)
                continue;

            string firstChar = line[0].ToString();

            line = line[1..];

            switch (firstChar) {
                //Contains the relative path to the song file in the format of
                //@path
                //ex. @animariot.ogg
                case "@": {
                    song.AudioPath = line;
                    break;
                }
                //Contains a note in the format of
                //TimeInSeconds CharacterToType
                //ex. +109.041176 だい
                case "+": {
                    string[] splitLine = line.Split(" ");

                    double time = double.Parse(splitLine[0]) * 1000;
                    string text = splitLine[1];

                    song.Notes.Add(
                    new() {
                        Time  = time,
                        Text  = text.Trim(),
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
                    string text = splitLine[1];

                    song.Events.Add(
                    new LyricEvent {
                        Lyric = text.Trim(),
                        Time  = time
                    }
                    );

                    break;
                }
                //Prevents you from typing the previous note in the format of
                // /TimeInSeconds
                //ex. /17.982353
                case "/": {
                    song.Events.Add(
                    new TypingCutoffEvent {
                        Time = double.Parse(line) * 1000d
                    }
                    );

                    break;
                }
                //A beatline beat (happens every 1/4th beat except for full beats)
                //-TimeInSeconds
                //ex. -17.747059
                case "-": {
                    song.Events.Add(
                    new BeatLineBeatEvent {
                        Time = double.Parse(line) * 1000d
                    }
                    );

                    break;
                }
                //A beatline bar (happens every full beat)
                //=TimeInSeconds
                //ex. =4.544444
                case "=": {
                    song.Events.Add(
                    new BeatLineBarEvent {
                        Time = double.Parse(line) * 1000d
                    }
                    );

                    break;
                }
            }

        } while (line != null);

        List<BeatLineBarEvent> beatLineBarEvents = song.Events.Where(x => x is BeatLineBarEvent).Cast<BeatLineBarEvent>().ToList();

        double tempo = beatLineBarEvents[1].Time - beatLineBarEvents[0].Time;

        song.TimingPoints.Add(
        new TimingPoint {
            Time  = song.Events.First(x => x is BeatLineBarEvent).Time,
            Tempo = tempo / 4d
        }
        );

        Logger.Log(
        $"UTyping song loaded! notecount:{song.Notes.Count} eventcount:{song.Events.Count} {song.Artist}-{song.Name} diff:{song.Difficulty} creator:{song.Creator}",
        LoggerLevelSongInfo.Instance
        );

        return song;
    }

    public void SaveSong(Song song) {
        throw new NotImplementedException();
    }
}