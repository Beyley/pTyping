using System.Numerics;
using FontStashSharp;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Localization;
using Furball.Engine.Engine.Localization.Exceptions;
using Furball.Engine.Engine.Localization.Languages;

namespace pTyping.UiElements;

public class TextDrawable : Drawable {
	private string _text = null!;

	public readonly Bindable<object>     LocalizationTag;
	public readonly Bindable<string>     Format = new Bindable<string>("{0}");
	public readonly Bindable<FontSystem> FontSystem;
	public          DynamicSpriteFont    Font { get; private set; } = null!;
	public readonly Bindable<float>      FontSize;

	public override Vector2 Size => this.Font.MeasureString(this._text);

	public TextDrawable(object localizationTag, FontSystem font, float fontSize) {
		this.LocalizationTag = new Bindable<object>(localizationTag);
		this.FontSystem      = new Bindable<FontSystem>(font);
		this.FontSize        = new Bindable<float>(fontSize);

		this.LocalizationTag.OnChange += (_, _) => this.LanguageChanged(this, LocalizationManager.CurrentLanguage);
		this.FontSystem.OnChange      += (_, _) => this.CreateFont();
		this.FontSize.OnChange        += (_, _) => this.CreateFont();

		LocalizationManager.LanguageChanged += this.LanguageChanged;

		this.LanguageChanged(this, LocalizationManager.CurrentLanguage);
		this.CreateFont();
	}

	private void CreateFont() {
		this.Font = this.FontSystem.Value.GetFont(this.FontSize);
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		batch.DrawString(this.Font, this._text, args.Position, args.Color, args.Rotation, args.Scale, this.RotationOrigin);
	}

	public override void Dispose() {
		base.Dispose();

		LocalizationManager.LanguageChanged -= this.LanguageChanged;
	}

	private void LanguageChanged(object? sender, Language e) {
		try {
			this._text = string.Format(this.Format.Value, LocalizationManager.GetLocalizedString(this.LocalizationTag, e));
		}
		catch (NoTranslationException) {
			this._text = string.Format(this.Format.Value, this.LocalizationTag);
		}
	}
}
