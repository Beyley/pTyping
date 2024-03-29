using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;

namespace pTyping.Graphics.Editor.Scene;

public class SongProgressBarDrawable : DrawableProgressBar {
	public SongProgressBarDrawable() : base(Vector2.Zero, pTypingGame.JapaneseFont, 24, new Vector2(FurballGame.WindowWidth, 30), Color.White, Color.DarkGray, Color.White) {
		this.OnClick += this.Click;
		this.OnDrag  += this.Drag;
	}

	private void Click(object sender, MouseButtonEventArgs e) {
		this.Drag(sender, new MouseDragEventArgs(Vector2.Zero, Vector2.Zero, e.Mouse.Position, e.Button, e.Mouse));
	}

	private void Drag(object sender, MouseDragEventArgs e) {
		//get relative position
		Vector2 relativePosition = e.Position - this.RealPosition;

		float progress = relativePosition.X / this.RealSize.X;

		pTypingGame.MusicTrack.CurrentPosition = pTypingGame.MusicTrack.Length * progress;
	}

	public override void Update(double time) {
		base.Update(time);

		this.Progress = (float)(pTypingGame.MusicTrack.CurrentPosition / pTypingGame.MusicTrack.Length);
	}
}
