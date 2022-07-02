using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Vixie.Backends.Shared;
using Newtonsoft.Json;

namespace pTyping.Graphics.UiMaker;

public enum UiMakerElementType {
    Text,
    Texture
}

[JsonObject(MemberSerialization.OptIn)]
public class UiMakerElement {
    [JsonProperty]
    public string Identifier;
    [JsonProperty]
    public UiMakerElementType Type;

    #region Global

    [JsonProperty]
    public Vector2 Position;
    [JsonProperty]
    public Vector2 RotationOrigin;
    [JsonProperty]
    public Vector2 Scale = Vector2.One;
    [JsonProperty]
    public Color Color = Color.White;
    [JsonProperty]
    public float Rotation;
    [JsonProperty]
    public OriginType OriginType = OriginType.TopLeft;
    [JsonProperty]
    public float Depth;

    #endregion

    #region Text

    [JsonProperty]
    public string Text;
    [JsonProperty]
    public int FontSize;

    #endregion

    #region Texture

    [JsonProperty]
    public string Texture;

    #endregion
}

[JsonObject(MemberSerialization.OptIn)]
public class UiMakerElementContainer {
    [JsonProperty]
    public string Name;

    [JsonProperty]
    public List<UiMakerElement> Elements = new();
}

public class UiMakerScreenContent : CompositeDrawable {}

public class UiMakerScreen : pScreen {
    private readonly UiMakerElementContainer _currentContainer;

    private const string UI_ELEMENTS_FOLDER = "uielements";

    private UiMakerScreenContent Content;

    public UiMakerScreen(string name) {
        if (!Directory.Exists(UI_ELEMENTS_FOLDER))
            Directory.CreateDirectory(UI_ELEMENTS_FOLDER);

        string path = Path.Combine(UI_ELEMENTS_FOLDER, name);

        if (!File.Exists(path)) {
            this._currentContainer = new UiMakerElementContainer {
                Name = name
            };

            return;
        }

        this._currentContainer = JsonConvert.DeserializeObject<UiMakerElementContainer>(File.ReadAllText(path));
    }

    public override void Initialize() {
        base.Initialize();

        this.Content = new();
        this.Manager.Add(this.Content);

        this._currentContainer.Elements.Add(
        new UiMakerElement {
            Identifier = "test",
            Type       = UiMakerElementType.Text,
            FontSize   = 24,
            Text       = "This is a test!"
        }
        );

        this._currentContainer.Elements.Add(
        new UiMakerElement {
            Identifier = "test2",
            Type       = UiMakerElementType.Text,
            FontSize   = 24,
            Text       = "This is also a test!",
            Color      = Color.Red,
            Rotation   = 10f,
            Position   = new Vector2(20, 20)
        }
        );

        this.ResetLayout();
    }

    private readonly Dictionary<string, Texture> _textureCache = new();

    private Drawable CreateDrawableFromElement(UiMakerElement element) {
        Drawable drawable;

        switch (element.Type) {
            case UiMakerElementType.Text:
                TextDrawable text = new(Vector2.Zero, pTypingGame.JapaneseFontStroked, element.Text, element.FontSize);

                drawable = text;
                break;
            case UiMakerElementType.Texture:
                if (!this._textureCache.TryGetValue(element.Texture, out Texture tex)) {
                    tex = ContentManager.LoadTextureFromFile(element.Texture, ContentSource.User);

                    this._textureCache[element.Texture] = tex;
                }

                TexturedDrawable texture = new(tex, Vector2.Zero);

                drawable = texture;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof (element));
        }

        drawable.OriginType     = element.OriginType;
        drawable.Position       = element.Position;
        drawable.Rotation       = element.Rotation;
        drawable.Scale          = element.Scale;
        drawable.RotationOrigin = element.RotationOrigin;
        drawable.ColorOverride  = element.Color;
        drawable.Depth          = element.Depth;

        return drawable;
    }

    public override void Dispose() {
        base.Dispose();

        foreach ((_, Texture tex) in this._textureCache)
            tex.Dispose();
    }

    private void ResetLayout() {
        this.Content.Drawables.Clear();

        foreach (UiMakerElement element in this._currentContainer.Elements) {
            Drawable drawable = this.CreateDrawableFromElement(element);

            this.Content.Drawables.Add(drawable);
        }
    }

    public override string               Name                 => "UiMaker";
    public override string               State                => "Making new UI";
    public override string               Details              => "";
    public override bool                 ForceSpeedReset      => true;
    public override float                BackgroundFadeAmount => 0.5f;
    public override MusicLoopState       LoopState            => MusicLoopState.None;
    public override ScreenType           ScreenType           => ScreenType.Menu;
    public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.Listening;
}
