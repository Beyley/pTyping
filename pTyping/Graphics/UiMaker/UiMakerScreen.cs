using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Helpers;
using Furball.Vixie.Backends.Shared;
using Newtonsoft.Json;
using pTyping.UiGenerator;
using Silk.NET.Input;
using Color=Furball.Vixie.Backends.Shared.Color;

namespace pTyping.Graphics.UiMaker;

public enum UiMakerElementType {
    None,
    Text,
    Texture
}

[JsonObject(MemberSerialization.OptIn)]
public class UiMakerElement {
    [JsonProperty]
    public string Identifier;
    [JsonProperty]
    public UiMakerElementType Type = UiMakerElementType.None;

    public  Drawable      Drawable;
    private UiMakerScreen _screen;

    public Vector2 DragBeginOffset;
    public bool    ClickedButNotDragged;

    public void SetDrawableProperties(UiMakerScreen screen, bool disableHeavy = true) {
        this._screen = screen;

        switch (this.Drawable) {
            case TextDrawable text:
                text.Text = this.Text;
                if (!disableHeavy)
                    this.UpdateFontSize();

                break;
            case TexturedDrawable texture:
                if (!disableHeavy)
                    this.UpdateTexture();
                
                break;
            default:
                throw new NotSupportedException("That drawable type is not supported!");
        }

        this.Drawable.OriginType     = this.OriginType;
        this.Drawable.Position       = this.Position;
        this.Drawable.Rotation       = this.Rotation;
        this.Drawable.Scale          = this.Scale;
        this.Drawable.RotationOrigin = this.RotationOrigin;
        this.Drawable.ColorOverride  = this.Color;
        this.Drawable.Depth          = this.Depth;
    }

    public void UpdateFontSize() {
        TextDrawable text = (TextDrawable)this.Drawable;

        text.SetFont(pTypingGame.JapaneseFontStroked, this.FontSize);
    }

    public void UpdateTexture() {
        TexturedDrawable texture = (TexturedDrawable)this.Drawable;

        texture.SetTexture(this._screen.GetTexture(this.Texture));
    }

    public void SetEvents() {
        this.Drawable.OnClick   += this.OnClick;
        this.Drawable.OnClickUp += this.OnClickUp;

        this.Drawable.OnDragBegin += this.OnDragBegin;
        this.Drawable.OnDragEnd   += this.OnDragEnd;
        this.Drawable.OnDrag      += this.OnDrag;
    }

    private void OnDragBegin(object sender, Point e) {
        if (!this._screen.Selected.Contains(this))
            return;

        this._screen.DragBegin(e);
    }
    private void OnDrag(object sender, Point e) {
        if (!this._screen.Selected.Contains(this))
            return;

        this._screen.Drag(e);
    }
    private void OnDragEnd(object sender, Point e) {
        if (!this._screen.Selected.Contains(this))
            return;

        this._screen.DragEnd(e);
    }

    private void OnClick(object sender, (MouseButton button, Point pos) e) {
        this.ClickedButNotDragged = true;
    }

    private void OnClickUp(object sender, (MouseButton button, Point pos) e) {
        if (e.button == MouseButton.Left && this.ClickedButNotDragged) {
            if (!FurballGame.InputManager.ControlHeld)
                this._screen.Selected.Clear();

            if (this._screen.Selected.Contains(this))
                this._screen.Selected.Remove(this);
            else
                this._screen.Selected.Add(this);
        }
    }

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

public class SelectBoxDrawable : RectanglePrimitiveDrawable {
    public Drawable Parent;

    public SelectBoxDrawable(Drawable parent) {
        this.Parent = parent;

        this.Clickable   = false;
        this.CoverClicks = false;
        this.Hoverable   = false;
        this.CoverHovers = false;

        this.ColorOverride = Color.White;
        this.Thickness     = 1f;
        this.Filled        = false;
    }

    public override void Update(double time) {
        base.Update(time);

        this.Position = this.Parent.Position;
        this.RectSize = this.Parent.Size;
    }
}

public class UiMakerScreen : pScreen {
    private int    _defaultIdentifierIndex = 0;
    private string GetDefaultIdentifier() => $"item{this._defaultIdentifierIndex++}";

    private readonly UiMakerElementContainer _currentContainer;

    private const string UI_ELEMENTS_FOLDER = "uielements";

    private UiMakerScreenContent Content;

    public readonly ObservableCollection<UiMakerElement> Selected = new();

    public List<Drawable> SelectedDrawables = new();

    private UiContainer _createThingsContainer;
    private UiContainer _editThingsContainer;

    private UiElement _colourPicker;

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

        FurballGame.DrawInputOverlay = true;

        this.Content = new() {
            Position = new(50),
            Depth    = 1f
        };
        this.Manager.Add(this.Content);

        this._currentContainer.Elements.Add(
        new UiMakerElement {
            Identifier = this.GetDefaultIdentifier(),
            Type       = UiMakerElementType.Text,
            FontSize   = 24,
            Text       = "This is a test!"
        }
        );

        this._currentContainer.Elements.Add(
        new UiMakerElement {
            Identifier = this.GetDefaultIdentifier(),
            Type       = UiMakerElementType.Text,
            FontSize   = 24,
            Text       = "This is also a test!",
            Color      = Color.Red,
            Rotation   = 1f,
            Position   = new Vector2(20, 20)
        }
        );

        this.ResetLayout();

        this.AddUi();

        this.Selected.CollectionChanged += this.OnCollectionChanged;

        FurballGame.InputManager.OnKeyDown += this.OnKeyDown;
    }

    private void AddUi() {
        this.Manager.Add(this._createThingsContainer = new UiContainer(OriginType.TopLeft));
        this.Manager.Add(
        this._editThingsContainer = new UiContainer(OriginType.TopRight) {
            OriginType = OriginType.TopRight,
            Position   = new(FurballGame.DEFAULT_WINDOW_WIDTH, 0)
        }
        );

        UiElement button = UiElement.CreateButton(pTypingGame.JapaneseFontStroked, "Add Text Object", 20, Color.Blue, Color.White, Color.Black, Vector2.Zero);

        button.AsButton().OnClick += delegate {
            this._currentContainer.Elements.Add(
            new UiMakerElement {
                Identifier = this.GetDefaultIdentifier(),
                Type       = UiMakerElementType.Text,
                FontSize   = 24,
                Text       = "This element was added at runtime!",
                Color      = Color.White,
                Rotation   = 0f,
                Position   = new Vector2(50, 50)
            }
            );

            this.ResetLayout();
        };

        this._createThingsContainer.RegisterElement(button);

        button = UiElement.CreateButton(pTypingGame.JapaneseFontStroked, "Add Texture Object", 20, Color.Blue, Color.White, Color.Black, Vector2.Zero);

        button.AsButton().OnClick += delegate {
            this._currentContainer.Elements.Add(
            new UiMakerElement {
                Identifier = this.GetDefaultIdentifier(),
                Type       = UiMakerElementType.Texture,
                Texture    = "note.png",
                Color      = Color.White,
                Rotation   = 0f,
                Position   = new Vector2(50, 50)
            }
            );

            this.ResetLayout();
        };

        this._createThingsContainer.RegisterElement(button);

        this._colourPicker = UiElement.CreateColorPicker(pTypingGame.JapaneseFontStroked, 20, Color.White);
        this._colourPicker.AsColorPicker().Color.OnChange += delegate(object _, Color e) {
            foreach (UiMakerElement uiMakerElement in this.Selected) {
                uiMakerElement.Color = e;
                uiMakerElement.SetDrawableProperties(this);
            }
        };
        this._editThingsContainer.RegisterElement(this._colourPicker);
    }

    private void UpdateUi() {
        if (this.Selected.Count == 1) {
            UiMakerElement selected = this.Selected[0];

            this._colourPicker.AsColorPicker().Color.Value = selected.Color;
        }

    }

    public void DragBegin(Point e) {
        foreach (UiMakerElement element in this.Selected) {
            element.ClickedButNotDragged = false;

            element.DragBeginOffset = e.ToVector2() - element.Position;
        }
    }

    public void Drag(Point e) {
        foreach (UiMakerElement element in this.Selected) {
            element.Position = e.ToVector2() - element.DragBeginOffset;

            element.SetDrawableProperties(this);
        }
    }

    public void DragEnd(Point e) {
        //
    }

    private void OnKeyDown(object sender, Key e) {
        switch (e) {
            case Key.Escape: {
                this.Selected.Clear();

                break;
            }
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        foreach (Drawable d in this.SelectedDrawables)
            this.Content.Drawables.Remove(d);

        this.SelectedDrawables.Clear();

        foreach (UiMakerElement uiMakerElement in this.Selected) {
            SelectBoxDrawable drawable = new(uiMakerElement.Drawable);

            this.SelectedDrawables.Add(drawable);
            this.Content.Drawables.Add(drawable);
        }

        this.UpdateUi();
    }

    private readonly Dictionary<string, Texture> _textureCache = new();

    private Drawable CreateDrawableFromElement(UiMakerElement element) {

        Drawable drawable = element.Type switch {
            UiMakerElementType.Text    => new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, element.Text, element.FontSize),
            UiMakerElementType.Texture => new TexturedDrawable(null, Vector2.Zero),
            _                          => throw new ArgumentOutOfRangeException(nameof (element))
        };

        element.Drawable = drawable;

        element.SetDrawableProperties(this, false);

        element.SetEvents();

        return drawable;
    }

    public Texture GetTexture(string name) {
        if (!this._textureCache.TryGetValue(name, out Texture tex)) {
            tex = ContentManager.LoadTextureFromFile(name, ContentSource.User);

            this._textureCache[name] = tex;
        }

        return tex;
    }

    public override void Dispose() {
        base.Dispose();

        foreach ((_, Texture tex) in this._textureCache)
            tex.Dispose();

        this.Selected.CollectionChanged -= this.OnCollectionChanged;

        FurballGame.InputManager.OnKeyDown -= this.OnKeyDown;

        FurballGame.DrawInputOverlay = false;
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
