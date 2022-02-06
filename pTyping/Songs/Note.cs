using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using pTyping.Graphics.Player;

namespace pTyping.Songs;

public enum HitResult {
    Excellent,
    Good,
    Fair,
    Poor,
    NotHit
}

public enum NoteType {
    UTyping
}

[JsonObject(MemberSerialization.OptIn)]
public class Note {
    [JsonProperty]
    public Color Color = Color.Red;

    [JsonProperty]
    public NoteType Type = NoteType.UTyping;

    /// <summary>
    ///     Whether or not the note has been hit
    /// </summary>
    public HitResult HitResult = HitResult.NotHit;

    [JsonProperty]
    public string Text;
    [JsonProperty]
    public double Time;
    /// <summary>
    ///     The currently typed part of the note
    /// </summary>
    public string Typed = "";
    /// <summary>
    ///     The currently typed part of the current hiragana to type
    /// </summary>
    public string TypedRomaji = "";
    [JsonProperty]
    public float YOffset;
    public (string Hiragana, List<string> Romaji) TypableRomaji => this.GetTypableRomaji(this.Typed);

    [Pure]
    public (string Hiragana, List<string> Romaji) GetTypableRomaji(string typed = "") {
        if (this.IsHit)
            return (string.Empty, new() {
                           string.Empty
                       });

        string       textToCheck = string.Empty;
        List<string> possible    = null;

        if (this.Text.Length - typed.Length >= 3) {//Try to get the next 3 chars
            textToCheck = this.Text.Substring(typed.Length, 3);
            HiraganaConversion.CONVERSIONS.TryGetValue(textToCheck, out possible);
        }

        //Try to get the next 2 chars instead
        if (possible is null && this.Text.Length - typed.Length >= 2) {
            textToCheck = this.Text.Substring(typed.Length, 2);
            HiraganaConversion.CONVERSIONS.TryGetValue(textToCheck, out possible);
        }

        //Try to get the next char instead
        if (possible is null) {
            textToCheck = this.Text.Substring(typed.Length, 1);
            HiraganaConversion.CONVERSIONS.TryGetValue(textToCheck, out possible);
        }

        if (possible is null) throw new Exception("Unknown character! Did you put kanji? smh my head");

        possible.Sort((x, y) => x.Length - y.Length);

        return (textToCheck, possible);
    }

    public bool IsHit => this.Typed == this.Text;
}