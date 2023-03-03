using System.Numerics;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;

namespace pTyping.Graphics.Drawables;

public class SettingsForm : DrawableForm {
	private class DrawableSettingsMenu : ScrollableContainer {
		public DrawableSettingsMenu() : base(new Vector2(300, 400)) {
			this.Add(new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, "Options (WIP)", 24));
		}
	}

	public bool StateChanging;

	public SettingsForm(OriginType startPosition = OriginType.Center) : base("Settings", new DrawableSettingsMenu(), startPosition) {}

	public void Hide() {
		this.ChildrenLock.EnterWriteLock();

		this.UnregisterChildrenForInput();

		this.ChildrenLock.ExitWriteLock();
	}

	public void Show() {
		this.ChildrenLock.EnterWriteLock();

		this.RegisterChildrenForInput();

		this.ChildrenLock.ExitWriteLock();
	}
}
