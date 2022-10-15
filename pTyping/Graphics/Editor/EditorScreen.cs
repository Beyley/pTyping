using System;
using System.Collections.Generic;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Editor.Scene.LyricEditor;
using pTyping.Graphics.Editor.Scene.NoteEditor;
using pTyping.Shared.Beatmaps;
using pTyping.UiElements;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen : pScreen {
	/// <summary>
	///     The working beatmap, aka the beatmap that is being edited
	/// </summary>
	public readonly Beatmap Beatmap;

	private DrawableProgressBar _songProgressBar;

	public EditorScreen(Beatmap beatmap) {
		//Clone the beatmap to not edit the original
		this.Beatmap = beatmap.Clone();
	}
	
	public override void Initialize() {
		base.Initialize();

		//Add the toolbar
		this.Manager.Add(new ToolbarDrawable(new Dictionary<string, Action> {
			{ "File", this.OpenFileMenu },
			{ "Note Editor", () => this.LoadScene(new NoteEditorScene(this)) },
			{ "Lyric Editor", () => this.LoadScene(new LyricEditorScene(this)) }
		}, pTypingGame.JapaneseFont));

		//Load the default scene when you first start the editor
		this.LoadScene(new NoteEditorScene(this));

		//Add an outline for the scene
		this.Manager.Add(this._sceneOutline = new RectanglePrimitiveDrawable(this.ScenePosition, this.SceneSize, 1, false));

		//Add a song progress bar to the bottom of the screen
		this.Manager.Add(this._songProgressBar = new DrawableProgressBar(Vector2.Zero, pTypingGame.JapaneseFont, 24, new Vector2(FurballGame.WindowWidth, 30), Color.White, Color.DarkGray, Color.White) {
			OriginType       = OriginType.BottomLeft,
			ScreenOriginType = OriginType.BottomLeft
		});
	}

	public override void Update(double gameTime) {
		base.Update(gameTime);

		this._songProgressBar.Progress = (float)(pTypingGame.MusicTrack.CurrentPosition / pTypingGame.MusicTrack.Length);
	}

	public override void Relayout(float newWidth, float newHeight) {
		base.Relayout(newWidth, newHeight);

		this.RelayoutScene(newWidth, newHeight);

		if (this._songProgressBar != null)
			this._songProgressBar.BarSize.X = newWidth;
	}
}
