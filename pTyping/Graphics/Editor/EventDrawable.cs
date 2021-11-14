using Furball.Engine.Engine.Graphics.Drawables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pTyping.Songs;

namespace pTyping.Graphics.Editor {
    public static class EventDrawable {
        public static ManagedDrawable GetDrawableFromEvent(Event songEvent, Texture2D noteTexture) {
            ManagedDrawable drawable = new BlankDrawable();

            switch (songEvent.Type) {
                case EventType.TypingCutoff: {
                    drawable = new TexturedDrawable(noteTexture, Vector2.Zero) {
                        TimeSource    = pTypingGame.MusicTrack,
                        ColorOverride = Color.LightBlue,
                        Scale         = new(0.3f),
                        OriginType    = OriginType.Center
                    };
                    break;
                }
            }

            return drawable;
        }
    }
}
