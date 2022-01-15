using System;
using System.Collections.Generic;
using FontStashSharp;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using Microsoft.Xna.Framework;

namespace pTyping.UiGenerator;

public class UiElement {
    public UiElementType Type { get; }

    public ManagedDrawable Drawable = null;

    public bool InUse = false;

    public float SpaceAfter = 10;

    public Bindable<bool> Visible = new(true);

    private UiElement(UiElementType type) => this.Type = type;

    public static UiElement CreateButton(FontSystem font, string text, int size, Color buttonColor, Color textColor, Color outlineColor, Vector2 buttonSize) {
        UiElement element = new(UiElementType.Button) {
            Drawable = new UiButtonDrawable(new(0f), text, font, size, buttonColor, textColor, outlineColor, buttonSize)
        };

        return element;
    }

    public static UiElement CreateDropdown(List<string> items, Vector2 buttonSize, FontSystem font, int fontSize) {
        UiElement element = new(UiElementType.Dropdown) {
            Drawable = new UiDropdownDrawable(new(0f), items, buttonSize, font, fontSize)
        };

        return element;
    }

    public static UiElement CreateProgressBar(FontSystem font, Vector2 size, Color outlineColor, Color textColor, Color barColor) {
        UiElement element = new(UiElementType.ProgressBar) {
            Drawable = new UiProgressBarDrawable(new(0), font, size, outlineColor, barColor, textColor)
        };

        return element;
    }

    public static UiElement CreateTextBox(FontSystem font, string text, int size, float width) {
        UiElement element = new(UiElementType.TextBox) {
            Drawable = new UiTextBoxDrawable(new(0), font, text, size, width)
        };

        return element;
    }

    public static UiElement CreateTickBox(string text, int size, bool initialState = false, bool managed = false) {
        UiElement element = new(UiElementType.TickBox) {
            Drawable = new UiTickboxDrawable(new(0), text, size, initialState, managed)
        };

        return element;
    }

    public static UiElement CreateText(FontSystem font, string text, int size) {
        UiElement element = new(UiElementType.Text) {
            Drawable = new TextDrawable(new(0), font, text, size)
        };

        return element;
    }

    public static UiElement CreateColorPicker(FontSystem font, int size, Color initialColor) {
        UiElement element = new(UiElementType.ColorPicker) {
            Drawable = new UiColorPickerDrawable(new(0), font, size, initialColor)
        };

        return element;
    }

    #region Type Conversions

    public UiButtonDrawable AsButton() {
        if (this.Type == UiElementType.Button)
            return this.Drawable as UiButtonDrawable;

        throw new NotSupportedException();
    }

    public UiDropdownDrawable AsDropdown() {
        if (this.Type == UiElementType.Dropdown)
            return this.Drawable as UiDropdownDrawable;

        throw new NotSupportedException();
    }

    public UiProgressBarDrawable AsProgressBar() {
        if (this.Type == UiElementType.ProgressBar)
            return this.Drawable as UiProgressBarDrawable;

        throw new NotSupportedException();
    }

    public UiTextBoxDrawable AsTextBox() {
        if (this.Type == UiElementType.TextBox)
            return this.Drawable as UiTextBoxDrawable;

        throw new NotSupportedException();
    }

    public UiTickboxDrawable AsTickBox() {
        if (this.Type == UiElementType.TickBox)
            return this.Drawable as UiTickboxDrawable;

        throw new NotSupportedException();
    }

    public TextDrawable AsText() {
        if (this.Type == UiElementType.Text)
            return this.Drawable as TextDrawable;

        throw new NotSupportedException();
    }

    public UiColorPickerDrawable AsColorPicker() {
        if (this.Type == UiElementType.ColorPicker)
            return this.Drawable as UiColorPickerDrawable;

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