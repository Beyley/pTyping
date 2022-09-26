using System;
using System.Collections.Generic;
using System.Numerics;
using FontStashSharp;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using Furball.Vixie.Backends.Shared;

namespace pTyping.UiGenerator;

public class UiElement {
	public UiElementType Type { get; }

	public Drawable Drawable;

	public bool InUse = false;

	public float SpaceAfter = 10;

	public Bindable<bool> Visible = new Bindable<bool>(true);

	private UiElement(UiElementType type) {
		this.Type = type;
	}

	public static UiElement CreateButton(FontSystem font, string text, int size, Color buttonColor, Color textColor, Color outlineColor, Vector2 buttonSize) {
		UiElement element = new UiElement(UiElementType.Button) {
			Drawable = new DrawableButton(new Vector2(0f), font, size, text, buttonColor, textColor, outlineColor, buttonSize)
		};

		return element;
	}

	public static UiElement CreateDropdown(Dictionary<object, string> items, Vector2 buttonSize, FontSystem font, int fontSize) {
		UiElement element = new UiElement(UiElementType.Dropdown) {
			Drawable = new DrawableDropdown(new Vector2(0f), font, fontSize, buttonSize, items)
		};

		return element;
	}

	public static UiElement CreateProgressBar(FontSystem font, Vector2 size, Color outlineColor, Color textColor, Color barColor) {
		UiElement element = new UiElement(UiElementType.ProgressBar) {
			Drawable = new DrawableProgressBar(new Vector2(0), font, (int)(size.Y * 0.9f), size, outlineColor, barColor, textColor)
		};

		return element;
	}

	public static UiElement CreateTextBox(FontSystem font, string text, int size, float width) {
		UiElement element = new UiElement(UiElementType.TextBox) {
			Drawable = new DrawableTextBox(new Vector2(0), font, size, width, text)
		};

		return element;
	}

	public static UiElement CreateTickBox(string text, int size, bool initialState = false, bool managed = false) {
		UiElement element = new UiElement(UiElementType.TickBox) {
			Drawable = new DrawableTickbox(new Vector2(0), pTypingGame.JapaneseFont, size, text, initialState, managed)
		};

		return element;
	}

	public static UiElement CreateText(FontSystem font, string text, int size) {
		UiElement element = new UiElement(UiElementType.Text) {
			Drawable = new TextDrawable(new Vector2(0), font, text, size)
		};

		return element;
	}

	public static UiElement CreateColorPicker(FontSystem font, int size, Color initialColor) {
		UiElement element = new UiElement(UiElementType.ColorPicker) {
			Drawable = new DrawableColorPicker(new Vector2(0), font, size, initialColor)
		};

		return element;
	}

	#region Type Conversions

	public DrawableButton AsButton() {
		if (this.Type == UiElementType.Button)
			return this.Drawable as DrawableButton;

		throw new NotSupportedException();
	}

	public DrawableDropdown AsDropdown() {
		if (this.Type == UiElementType.Dropdown)
			return this.Drawable as DrawableDropdown;

		throw new NotSupportedException();
	}

	public DrawableProgressBar AsProgressBar() {
		if (this.Type == UiElementType.ProgressBar)
			return this.Drawable as DrawableProgressBar;

		throw new NotSupportedException();
	}

	public DrawableTextBox AsTextBox() {
		if (this.Type == UiElementType.TextBox)
			return this.Drawable as DrawableTextBox;

		throw new NotSupportedException();
	}

	public DrawableTickbox AsTickBox() {
		if (this.Type == UiElementType.TickBox)
			return this.Drawable as DrawableTickbox;

		throw new NotSupportedException();
	}

	public TextDrawable AsText() {
		if (this.Type == UiElementType.Text)
			return this.Drawable as TextDrawable;

		throw new NotSupportedException();
	}

	public DrawableColorPicker AsColorPicker() {
		if (this.Type == UiElementType.ColorPicker)
			return this.Drawable as DrawableColorPicker;

		throw new NotSupportedException();
	}

	#endregion
}

public enum UiElementType {
	Button,
	Dropdown,
	ProgressBar,
	TextBox,
	TickBox,
	Text,
	ColorPicker
}
