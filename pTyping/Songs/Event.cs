using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Vixie;
using Furball.Vixie.Backends.Shared;
using JetBrains.Annotations;
using Newtonsoft.Json;
using pTyping.Graphics.Drawables.Events;
using pTyping.Graphics.Player;

namespace pTyping.Songs;

[JsonObject(MemberSerialization.OptIn)]
public abstract class Event {
    [JsonProperty]
    public abstract EventType Type { get; }
    [JsonProperty]
    public double Time { get; set; }

    [Pure, CanBeNull]
    public static Drawable CreateEventDrawable(Event @event, Texture noteTexture, GameplayDrawableTweenArgs tweenArgs) {
        Drawable drawable = null;

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
            case EventType.Lyric: {
                if (tweenArgs.IsEditor) {
                    LyricEventDrawable tempDrawable = new(noteTexture, @event);
                    tempDrawable.CreateTweens(tweenArgs);

                    drawable = tempDrawable;
                }

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