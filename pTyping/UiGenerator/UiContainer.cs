using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Helpers;
using JetBrains.Annotations;

namespace pTyping.UiGenerator;

public class UiContainer : CompositeDrawable {
	public Bindable<OriginType> ElementOriginType;

	private readonly ObservableCollection<UiElement> _elements = new ObservableCollection<UiElement>();

	public ReadOnlyCollection<UiElement> Elements => new ReadOnlyCollection<UiElement>(this._elements);

	public int EasingTime = 100;

	/// <summary>
	///     Creates a UiContainer
	/// </summary>
	/// <param name="originType">The origin type for the internal elements</param>
	public UiContainer(OriginType originType) {
		this.ElementOriginType = new Bindable<OriginType>(originType);

		this._elements.CollectionChanged += this.Recalculate;
		this.ElementOriginType.OnChange  += this.OnOriginTypeChange;

		this.InvisibleToInput = true;
	}

	private void OnOriginTypeChange(object sender, OriginType e) {
		this.Recalculate(null, null);
	}

	private readonly Queue<UiElement> _queuedForDeletion = new Queue<UiElement>();
	private void Recalculate(object sender, [CanBeNull] NotifyCollectionChangedEventArgs e) {
		float y = 0f;

		UiElement removedElement = e?.Action == NotifyCollectionChangedAction.Remove ? e.OldItems?[0] as UiElement : null;

		if (removedElement != null)
			this.Children.Remove(removedElement.Drawable);

		for (int i = 0; i < this._elements.Count; i++) {
			UiElement element = this._elements[i];

			if (element == null)
				throw new NoNullAllowedException("UiElement cannot be null!");

			if (!element.Visible.Value) {
				element.Drawable.Visible = false;
				element.Drawable.Tweens.Clear();
				continue;
			}

			element.Drawable.Visible = true;

			//TODO: support when multiple are added
			bool elementIsAdded = e?.Action == NotifyCollectionChangedAction.Add && e.NewItems?[0] == element;

			//Update the origin type
			element.Drawable.OriginType = this.ElementOriginType;

			element.Drawable.MoveTo(new Vector2(0, y), elementIsAdded ? 0 : this.EasingTime);

			if (elementIsAdded)
				element.Drawable.FadeInFromZero(this.EasingTime);

			//Update the global y
			y += element.Drawable.Size.Y + element.SpaceAfter;

			//Only add the new element
			if (elementIsAdded)
				this.Children.Add(element.Drawable);
		}
	}

	public void RegisterElement(UiElement element, int index = -1) {
		if (element.InUse)
			return;

		if (element.Drawable == null)
			throw new InvalidOperationException();

		element.InUse = true;

		element.Visible.OnChange += this.OnElementVisibleChange;

		if (index != -1)
			this._elements.Insert(index, element);
		else
			this._elements.Add(element);
	}

	private void OnElementVisibleChange(object sender, bool e) {
		this.Recalculate(null, null);
	}

	public void UnRegisterElement(UiElement element) {
		if (this._elements.Remove(element)) {
			element.InUse = false;
			element.Drawable.Tweens.Clear();

			element.Visible.OnChange -= this.OnElementVisibleChange;
		}
	}

	public override void Dispose() {
		this._elements.CollectionChanged -= this.Recalculate;
		this.ElementOriginType.OnChange  -= this.OnOriginTypeChange;

		base.Dispose();
	}
}
