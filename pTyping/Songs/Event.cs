using Furball.Engine.Engine.Graphics.Drawables;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using pTyping.Graphics.Drawables.Events;
using pTyping.Graphics.Player;

namespace pTyping.Songs {
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Event {
        [JsonProperty]
        public abstract EventType Type { get; }
        [JsonProperty]
        public double Time { get; set; }

        public static ManagedDrawable CreateEventDrawable(Event @event, Texture2D noteTexture, GameplayDrawableTweenArgs tweenArgs) {
            ManagedDrawable drawable = null;
            switch (@event.Type) {
                case EventType.BeatLineBar: {
                    BeatLineBarEventDrawable tempDrawable = new(@event);
                    tempDrawable.CreateTweens(tweenArgs);

                    drawable = tempDrawable;

                    break;
                }
                case EventType.BeatLineBeat: {
                    BeatLineBeatEventDrawable tempDrawable = new(@event);
                    tempDrawable.CreateTweens(tweenArgs);

                    drawable = tempDrawable;

                    break;
                }
                case EventType.TypingCutoff: {
                    TypingCutoffEventDrawable tempDrawable = new(noteTexture, @event);
                    tempDrawable.CreateTweens(tweenArgs);

                    drawable = tempDrawable;

                    break;
                }
            }

            return drawable;
        }
    }

    public enum EventType {
        Lyric,
        TypingCutoff,
        BeatLineBar,
        BeatLineBeat
    }
}
