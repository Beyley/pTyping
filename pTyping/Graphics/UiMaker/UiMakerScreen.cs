using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie;
using Furball.Vixie.Backends.Shared;
using JetBrains.Annotations;
using Newtonsoft.Json;
using pTyping.UiGenerator;
using Silk.NET.Input;

namespace pTyping.Graphics.UiMaker;

public enum UiMakerElementType {
	None,
	Text,
	Texture,
	Button
}

[JsonObject(MemberSerialization.OptIn)]
public class UiMakerElement {
	[JsonProperty]
	public string Identifier;
	[JsonProperty]
	public UiMakerElementType Type = UiMakerElementType.None;

	public Drawable Drawable;
	[CanBeNull]
	public SelectBoxDrawable SelectDrawable;
	private UiMakerScreen _screen;

	public Vector2 DragBeginOffset;
	public bool    ClickedButNotDragged;

	public void SetDrawableProperties(UiMakerScreen screen, bool disableHeavy = true) {
		this._screen = screen;

		this.Drawable.OriginType     = this.OriginType;
		this.Drawable.Position       = this.Position;
		this.Drawable.Rotation       = this.Rotation;
		this.Drawable.Scale          = this.Scale;
		this.Drawable.RotationOrigin = this.RotationOrigin;
		this.Drawable.ColorOverride  = this.Color;
		this.Drawable.Depth          = this.Depth;

		if (this.SelectDrawable != null)
			this.SelectDrawable.OriginType = this.OriginType;

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
			case DrawableButton button:
				button.ButtonColor   = this.ButtonColor;
				button.ColorOverride = this.ButtonColor;
				button.OutlineColor  = this.ButtonOutlineColor;
				button.TextColor     = this.Color;
				button.ButtonSize    = this.ButtonSize;

				button.Tweens.Add(new ColorTween(TweenType.Color, button.ColorOverride, this.ButtonColor, FurballGame.Time, FurballGame.Time + 100));

				button.Text = this.Text;

				if (!disableHeavy)
					this.UpdateFontSize();

				break;
			default:
				throw new NotSupportedException("That drawable type is not supported!");
		}
	}

	public void UpdateFontSize() {
		switch (this.Drawable) {
			case TextDrawable text: {
				text.SetFont(pTypingGame.JapaneseFont, this.FontSize);
				break;
			}
			case DrawableButton button: {
				button.TextDrawable.SetFont(pTypingGame.JapaneseFont, this.FontSize);
				break;
			}
		}
	}

	public void UpdateTexture() {
		TexturedDrawable texture = (TexturedDrawable)this.Drawable;

		texture.Texture = this._screen.GetTexture(this.Texture);
	}

	public void SetEvents() {
		this.Drawable.OnClick   += this.OnClick;
		this.Drawable.OnClickUp += this.OnClickUp;

		this.Drawable.OnDragBegin += this.OnDragBegin;
		this.Drawable.OnDragEnd   += this.OnDragEnd;
		this.Drawable.OnDrag      += this.OnDrag;
	}

	private void OnDragBegin(object sender, MouseDragEventArgs mouseDragEventArgs) {
		if (!this._screen.Selected.Contains(this))
			return;

		this._screen.DragBegin(mouseDragEventArgs.Position);
	}
	private void OnDrag(object sender, MouseDragEventArgs mouseDragEventArgs) {
		if (!this._screen.Selected.Contains(this))
			return;

		this._screen.Drag(mouseDragEventArgs.Position);
	}
	private void OnDragEnd(object sender, MouseDragEventArgs mouseDragEventArgs) {
		if (!this._screen.Selected.Contains(this))
			return;

		this._screen.DragEnd(mouseDragEventArgs.Position);
	}

	private void OnClick(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		this.ClickedButNotDragged = true;
	}

	private void OnClickUp(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		if (mouseButtonEventArgs.Button == MouseButton.Left && this.ClickedButNotDragged) {
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
	public Vector2 Position = Vector2.Zero;
	[JsonProperty]
	public Vector2 RotationOrigin = Vector2.Zero;
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

	[JsonProperty]
	public string OnClickFuncName = "";
	[JsonProperty]
	public string OnClickUpFuncName = "";
	[JsonProperty]
	public string OnHoverFuncName = "";
	[JsonProperty]
	public string OnHoverLostFuncName = "";

	#endregion

	#region Button

	[JsonProperty]
	public Vector2 ButtonSize = Vector2.Zero;
	[JsonProperty]
	public Color ButtonColor = Color.Blue;
	[JsonProperty]
	public Color ButtonOutlineColor = Color.Black;

	#endregion

	#region Text

	[JsonProperty]
	public string Text = "";
	[JsonProperty]
	public int FontSize = 20;

	#endregion

	#region Texture

	[JsonProperty]
	public string Texture = "";

	#endregion
}

[JsonObject(MemberSerialization.OptIn)]
public class UiMakerElementContainer {
	[JsonProperty]
	public string Name;

	[JsonProperty]
	public List<UiMakerElement> Elements = new List<UiMakerElement>();
}

public class UiMakerScreenContent : CompositeDrawable {
	public UiMakerScreenContent() {
		this.InvisibleToInput = true;
	}
}

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
	private int _defaultIdentifierIndex;
	private string GetDefaultIdentifier() {
		return $"item{this._defaultIdentifierIndex++}";
	}

	private readonly UiMakerElementContainer _currentContainer;

	private const string UI_ELEMENTS_FOLDER = "uielements";

	private UiMakerScreenContent _content;

	public readonly ObservableCollection<UiMakerElement> Selected = new ObservableCollection<UiMakerElement>();

	public List<Drawable> SelectedDrawables = new List<Drawable>();

	private UiContainer _createThingsContainer;
	private UiContainer _editThingsContainer;

	private UiElement _colourPicker;
	private UiElement _buttonColourPicker;
	private UiElement _buttonOutlineColourPicker;
	private UiElement _depthPicker;
	private UiElement _rotationPicker;
	private UiElement _textPicker;
	private UiElement _texturePicker;
	private UiElement _originTypePicker;

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

		this._content = new() {
			Position = new(50),
			Depth    = 1f
		};
		this.Manager.Add(this._content);

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
			this._editThingsContainer = new UiContainer(OriginType.TopLeft) {
				OriginType = OriginType.TopRight,
				Position   = new(FurballGame.DEFAULT_WINDOW_WIDTH, 0)
			}
		);

		UiElement button = UiElement.CreateButton(pTypingGame.JapaneseFont, "Add Text Object", 20, Color.Blue, Color.White, Color.Black, Vector2.Zero);

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

		button = UiElement.CreateButton(pTypingGame.JapaneseFont, "Add Texture Object", 20, Color.Blue, Color.White, Color.Black, Vector2.Zero);

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

		button = UiElement.CreateButton(pTypingGame.JapaneseFont, "Add Button", 20, Color.Blue, Color.White, Color.Black, Vector2.Zero);

		button.AsButton().OnClick += delegate {
			this._currentContainer.Elements.Add(
				new UiMakerElement {
					Identifier         = this.GetDefaultIdentifier(),
					Type               = UiMakerElementType.Button,
					Text               = "Button Text",
					Color              = Color.White,
					ButtonColor        = Color.Blue,
					ButtonOutlineColor = Color.Black,
					Rotation           = 0f,
					Position           = new Vector2(50, 50)
				}
			);

			this.ResetLayout();
		};

		this._createThingsContainer.RegisterElement(button);

		button = UiElement.CreateButton(pTypingGame.JapaneseFont, "Run code generation", 20, Color.Blue, Color.White, Color.Black, Vector2.Zero);

		button.AsButton().OnClick += delegate {
			File.WriteAllText("Output.cs", UiMakerCodeGen.GenerateClass(this._currentContainer));
		};

		this._createThingsContainer.RegisterElement(button);

		this._editThingsContainer.RegisterElement(UiElement.CreateText(pTypingGame.JapaneseFont, "Colour:", 25));
		this._editThingsContainer.RegisterElement(this._colourPicker = UiElement.CreateColorPicker(pTypingGame.JapaneseFont, 20, Color.White));
		this._colourPicker.AsColorPicker().Color.OnChange += delegate(object _, Color e) {
			foreach (UiMakerElement uiMakerElement in this.Selected) {
				uiMakerElement.Color = e;
				uiMakerElement.SetDrawableProperties(this);
			}
		};

		this._editThingsContainer.RegisterElement(UiElement.CreateText(pTypingGame.JapaneseFont, "Button Colour:", 25));
		this._editThingsContainer.RegisterElement(this._buttonColourPicker = UiElement.CreateColorPicker(pTypingGame.JapaneseFont, 20, Color.White));
		this._buttonColourPicker.AsColorPicker().Color.OnChange += delegate(object _, Color e) {
			foreach (UiMakerElement uiMakerElement in this.Selected) {
				uiMakerElement.ButtonColor = e;
				uiMakerElement.SetDrawableProperties(this);
			}
		};

		this._editThingsContainer.RegisterElement(UiElement.CreateText(pTypingGame.JapaneseFont, "Button Outline Colour:", 25));
		this._editThingsContainer.RegisterElement(this._buttonOutlineColourPicker = UiElement.CreateColorPicker(pTypingGame.JapaneseFont, 20, Color.White));
		this._buttonOutlineColourPicker.AsColorPicker().Color.OnChange += delegate(object _, Color e) {
			foreach (UiMakerElement uiMakerElement in this.Selected) {
				uiMakerElement.ButtonOutlineColor = e;
				uiMakerElement.SetDrawableProperties(this);
			}
		};

		this._editThingsContainer.RegisterElement(UiElement.CreateText(pTypingGame.JapaneseFont, "Depth:", 25));
		this._editThingsContainer.RegisterElement(this._depthPicker = UiElement.CreateTextBox(pTypingGame.JapaneseFont, "0", 20, 100));
		this._depthPicker.AsTextBox().OnCommit += delegate(object _, string e) {
			if (!float.TryParse(e, out float res)) {
				this._depthPicker.AsTextBox().Tweens.Add(
					new ColorTween(TweenType.Color, this._depthPicker.AsTextBox().ColorOverride, Color.Red, FurballGame.Time, FurballGame.Time + 100)
				);
				return;
			}

			this._depthPicker.AsTextBox().Tweens.Add(
				new ColorTween(TweenType.Color, this._depthPicker.AsTextBox().ColorOverride, Color.White, FurballGame.Time, FurballGame.Time + 100)
			);

			foreach (UiMakerElement uiMakerElement in this.Selected) {
				uiMakerElement.Depth = res;
				uiMakerElement.SetDrawableProperties(this);
			}
		};

		this._editThingsContainer.RegisterElement(UiElement.CreateText(pTypingGame.JapaneseFont, "Rotation:", 25));
		this._editThingsContainer.RegisterElement(this._rotationPicker = UiElement.CreateTextBox(pTypingGame.JapaneseFont, "0", 20, 100));
		this._rotationPicker.AsTextBox().OnCommit += delegate(object _, string e) {
			if (!float.TryParse(e, out float res)) {
				this._rotationPicker.AsTextBox().Tweens.Add(
					new ColorTween(TweenType.Color, this._rotationPicker.AsTextBox().ColorOverride, Color.Red, FurballGame.Time, FurballGame.Time + 100)
				);
				return;
			}

			this._rotationPicker.AsTextBox().Tweens.Add(
				new ColorTween(TweenType.Color, this._rotationPicker.AsTextBox().ColorOverride, Color.White, FurballGame.Time, FurballGame.Time + 100)
			);

			foreach (UiMakerElement uiMakerElement in this.Selected) {
				uiMakerElement.Rotation = res;
				uiMakerElement.SetDrawableProperties(this);
			}
		};

		this._editThingsContainer.RegisterElement(UiElement.CreateText(pTypingGame.JapaneseFont, "Text:", 25));
		this._editThingsContainer.RegisterElement(this._textPicker = UiElement.CreateTextBox(pTypingGame.JapaneseFont, "", 20, 300));
		this._textPicker.AsTextBox().OnCommit += delegate(object _, string e) {
			foreach (UiMakerElement uiMakerElement in this.Selected) {
				uiMakerElement.Text = e;
				uiMakerElement.SetDrawableProperties(this);
			}
		};

		this._editThingsContainer.RegisterElement(UiElement.CreateText(pTypingGame.JapaneseFont, "Texture:", 25));
		this._editThingsContainer.RegisterElement(this._texturePicker = UiElement.CreateTextBox(pTypingGame.JapaneseFont, "", 20, 300));
		this._texturePicker.AsTextBox().OnCommit += delegate(object _, string e) {
			foreach (UiMakerElement uiMakerElement in this.Selected) {
				uiMakerElement.Texture = e;
				uiMakerElement.UpdateTexture();
			}
		};

		Dictionary<object, string> items = new Dictionary<object, string>();
		items.Add(OriginType.TopLeft, "Top Left");
		items.Add(OriginType.TopRight, "Top Right");
		items.Add(OriginType.BottomLeft, "Bottom Left");
		items.Add(OriginType.BottomRight, "Bottom Right");
		items.Add(OriginType.Center, "Center");
		items.Add(OriginType.TopCenter, "Top Center");
		items.Add(OriginType.BottomCenter, "Bottom Center");
		items.Add(OriginType.LeftCenter, "Left Center");
		items.Add(OriginType.RightCenter, "Right Center");
		this._editThingsContainer.RegisterElement(UiElement.CreateText(pTypingGame.JapaneseFont, "Origin Type:", 25));
		this._editThingsContainer.RegisterElement(this._originTypePicker = UiElement.CreateDropdown(items, new(100, 30), pTypingGame.JapaneseFont, 20));
		this._originTypePicker.AsDropdown().SelectedItem.OnChange += (_, pair) => {
			foreach (UiMakerElement uiMakerElement in this.Selected) {
				uiMakerElement.OriginType = (OriginType)pair.Key;
				uiMakerElement.SetDrawableProperties(this);
			}
		};
	}

	private void UpdateUi() {
		if (this.Selected.Count == 1) {
			UiMakerElement selected = this.Selected[0];

			this._colourPicker.AsColorPicker().Color.Value              = selected.Color;
			this._buttonColourPicker.AsColorPicker().Color.Value        = selected.ButtonColor;
			this._buttonOutlineColourPicker.AsColorPicker().Color.Value = selected.ButtonOutlineColor;
			this._depthPicker.AsTextBox().Text                          = $"{selected.Depth:0.###}";
			this._rotationPicker.AsTextBox().Text                       = $"{selected.Rotation:0.###}";
			this._textPicker.AsTextBox().Text                           = selected.Text;
			this._texturePicker.AsTextBox().Text                        = selected.Texture;
			this._originTypePicker.AsDropdown().SelectedItem.Value      = this._originTypePicker.AsDropdown().Items.First(x => (OriginType)x.Key == selected.OriginType);
		}
	}

	public void DragBegin(Vector2 e) {
		foreach (UiMakerElement element in this.Selected) {
			element.ClickedButNotDragged = false;

			element.DragBeginOffset = e - element.Position;
		}
	}

	public void Drag(Vector2 e) {
		foreach (UiMakerElement element in this.Selected) {
			element.Position = e - element.DragBeginOffset;

			element.SetDrawableProperties(this);
		}
	}

	public void DragEnd(Vector2 e) {
		//
	}

	private void OnKeyDown(object sender, KeyEventArgs keyEventArgs) {
		switch (keyEventArgs.Key) {
			case Key.Escape: {
				this.Selected.Clear();

				break;
			}
		}
	}

	private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
		foreach (Drawable d in this.SelectedDrawables)
			this._content.Children.Remove(d);

		this._currentContainer.Elements.ForEach(x => x.SelectDrawable = null);
		this.SelectedDrawables.Clear();

		foreach (UiMakerElement element in this.Selected) {
			SelectBoxDrawable drawable = new SelectBoxDrawable(element.Drawable) {
				OriginType = element.OriginType
			};

			element.SelectDrawable = drawable;
			this.SelectedDrawables.Add(drawable);
			this._content.Children.Add(drawable);
		}

		this.UpdateUi();
	}

	private readonly Dictionary<string, Texture> _textureCache = new Dictionary<string, Texture>();

	private Drawable CreateDrawableFromElement(UiMakerElement element) {
		Drawable drawable = element.Type switch {
			UiMakerElementType.Text    => new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, element.Text, element.FontSize),
			UiMakerElementType.Texture => new TexturedDrawable(null, Vector2.Zero),
			UiMakerElementType.Button => new DrawableButton(
				Vector2.Zero,
				pTypingGame.JapaneseFont,
				element.FontSize,
				element.Text,
				element.ButtonColor,
				element.Color,
				element.ButtonOutlineColor,
				element.ButtonSize
			),
			_ => throw new ArgumentOutOfRangeException(nameof (element))
		};

		element.Drawable = drawable;

		element.SetDrawableProperties(this, false);

		element.SetEvents();

		return drawable;
	}

	public Texture GetTexture(string name) {
		if (!this._textureCache.TryGetValue(name, out Texture tex)) {
			tex = ContentManager.LoadTextureFromFileCached(name, ContentSource.User);

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
		this._content.Children.Clear();

		foreach (UiMakerElement element in this._currentContainer.Elements) {
			Drawable drawable = this.CreateDrawableFromElement(element);

			this._content.Children.Add(drawable);
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
