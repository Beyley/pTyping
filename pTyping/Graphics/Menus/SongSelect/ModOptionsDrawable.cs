#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Vixie.Backends.Shared;
using pTyping.Shared.Mods;
using pTyping.Shared.Mods.Attributes;
using pTyping.Shared.ObjectModel;
using pTyping.UiElements;
using TextDrawable = Furball.Engine.Engine.Graphics.Drawables.TextDrawable;

namespace pTyping.Graphics.Menus.SongSelect;

public sealed class ModOptionsDrawable : CompositeDrawable {
	private readonly List<Mod>           _mods;
	private readonly ScrollableContainer _scrollable;

	public override Vector2 Size => new Vector2(350, 200);

	public ModOptionsDrawable(List<Mod> mods) {
		this._mods = mods;

		this.Children.Add(new RectanglePrimitiveDrawable(Vector2.Zero, this.Size, 0, true) {
			ColorOverride = new Color(100, 100, 100, 100),
			Clickable     = false,
			CoverClicks   = false,
			Hoverable     = false,
			CoverHovers   = false
		});
		this.Children.Add(this._scrollable = new ScrollableContainer(this.Size) {
			InvisibleToInput = true
		});

		this.InvisibleToInput = true;
	}

	public void RecalculateUi() {
		const float x = 10;
		float       y = 0;

		//Remove all drawables from the list and update the max scroll
		this._scrollable.Children.Clear();
		this._scrollable.RecalculateMax();

		//Iterate over all mods 
		foreach (Mod mod in this._mods) {
			//Get the ModSettingAttribute from all the field in the mod
			foreach (FieldInfo property in mod.GetType().GetFields()) {
				ModSettingAttribute? attribute = property.GetCustomAttribute<ModSettingAttribute>();

				//If the property has the attribute
				if (attribute != null)
					//Check if the property type is of type BoundNumber<>
					if (property.FieldType.IsGenericType && property.FieldType.GetGenericTypeDefinition() == typeof(BoundNumber<>)) {
						//Get the generic type of the property
						Type genericType = property.FieldType.GetGenericArguments()[0];

						//Get the value of the property
						object? value = property.GetValue(mod);

						//If the value is null, skip it
						if (value == null) continue;

						//Create a label for the option
						TextDrawable text = new TextDrawable(new Vector2(x, y), FurballGame.DefaultFont, attribute.Name, 24) {
							ToolTip = attribute.Tooltip
						};
						y += text.Size.Y;

						this._scrollable.Add(text);

						if (genericType == typeof(double)) {
							BoundNumber<double> val = (BoundNumber<double>)value;

							SliderDrawable<double> slider = new SliderDrawable<double>(val) {
								Position = new Vector2(x, y)
							};
							y += slider.Size.Y;

							this._scrollable.Add(slider);
						}
						else if (genericType == typeof(float)) {
							BoundNumber<double> val = (BoundNumber<double>)value;

							SliderDrawable<double> slider = new SliderDrawable<double>(val) {
								Position = new Vector2(x, y)
							};
							y += slider.Size.Y;

							this._scrollable.Add(slider);
						}
					}
			}
		}
	}
}
