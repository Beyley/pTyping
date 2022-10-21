using System.Collections.ObjectModel;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using Silk.NET.Input;

namespace pTyping.Graphics.Drawables;

public class SelectableCompositeDrawable : CompositeDrawable {
	private          ObservableCollection<SelectableCompositeDrawable> _selectedList;
	private readonly Bindable<bool>                                    _selectEnabled;

	public readonly Bindable<bool> Selected = new Bindable<bool>(false);

	private bool _enabled;

	protected SelectableCompositeDrawable(ObservableCollection<SelectableCompositeDrawable> selectedList, Bindable<bool> selectEnabled) {
		this._selectedList  = selectedList;
		this._selectEnabled = selectEnabled;

		this.OnClick                       += this.MouseDown;
		FurballGame.InputManager.OnKeyDown += this.KeyDown;

		selectEnabled.OnChange += this.EnableChange;

		this._enabled = selectEnabled.Value;
	}

	private void EnableChange(object sender, bool e) {
		this._enabled = e;

		if (!e) {
			this.Selected.Value = false;
			this._selectedList.Remove(this);
		}
	}

	private void KeyDown(object sender, KeyEventArgs e) {
		if (FurballGame.InputManager.CharInputHandler != null)
			return;

		if (e.Key != Key.Escape)
			return;

		if (this._selectedList != null) {
			foreach (SelectableCompositeDrawable drawable in this._selectedList)
				drawable.Selected.Value = false;

			this._selectedList.Clear();
		}

		this.Selected.Value = false;
	}

	public override void Dispose() {
		this.OnClick                       -= this.MouseDown;
		FurballGame.InputManager.OnKeyDown -= this.KeyDown;

		this._selectEnabled.OnChange -= this.EnableChange;

		this._selectedList?.Remove(this);
		this._selectedList = null;

		base.Dispose();
	}

	private void MouseDown(object sender, MouseButtonEventArgs e) {
		if (!this.Clickable)
			return;

		bool ctrlHeld = FurballGame.InputManager.ControlHeld;

		if (this._selectedList != null) {
			if (!ctrlHeld) {
				foreach (SelectableCompositeDrawable drawable in this._selectedList)
					drawable.Selected.Value = false;

				this._selectedList.Clear();
			}

			if (this._selectedList.Contains(this)) {
				if (ctrlHeld) {
					this._selectedList.Remove(this);
					this.Selected.Value = false;
				}

				return;
			}

			if (this._enabled)
				this._selectedList.Add(this);
		}

		if (this._enabled)
			this.Selected.Value = true;
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		base.Draw(time, batch, args);

		if (this.Selected.Value) {
			const float margin = 5f;

			batch.DrawRectangle(this.RealPosition - new Vector2(margin), this.RealSize + new Vector2(margin * 2), 1f, Color.White);
		}
	}
}
