using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using pTyping.Screens.Player;

namespace pTyping.Songs {
    public enum HitResult {
        Excellent,
        Good,
        Fair,
        Poor,
        NotHit
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Note {
        [JsonProperty]
        public Color Color = Color.Red;

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
        public (string Hiragana, List<string> Romaji) TypableRomaji {
            get {
                if (this.IsHit)
                    return (string.Empty, new() {
                                   string.Empty
                               });

                string       textToCheck = string.Empty;
                List<string> possible    = null;

                if (this.Text.Length - this.Typed.Length >= 3) {//Try to get the next 3 chars
                    textToCheck = this.Text.Substring(this.Typed.Length, 3);
                    HiraganaConversion.CONVERSIONS.TryGetValue(textToCheck, out possible);
                }

                //Try to get the next 2 chars instead
                if (possible is null && this.Text.Length - this.Typed.Length >= 2) {
                    textToCheck = this.Text.Substring(this.Typed.Length, 2);
                    HiraganaConversion.CONVERSIONS.TryGetValue(textToCheck, out possible);
                }

                //Try to get the next char instead
                if (possible is null) {
                    textToCheck = this.Text.Substring(this.Typed.Length, 1);
                    HiraganaConversion.CONVERSIONS.TryGetValue(textToCheck, out possible);
                }

                if (possible is null) throw new Exception("Unknown character! Did you put kanji? smh my head");

                possible.Sort((x, y) => x.Length - y.Length);

                return (textToCheck, possible);
            }
        }

        public bool IsHit => this.Typed == this.Text;
    }
}
