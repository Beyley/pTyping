using System;
using System.Collections.Generic;
using System.Linq;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using pTyping.Graphics.Editor.Scene;
using pTyping.Graphics.Editor.Scene.LyricEditor;
using pTyping.Graphics.Editor.Scene.NoteEditor;
using pTyping.Shared.Beatmaps;
using pTyping.UiElements;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen : pScreen {
	/// <summary>
	///     The live working beatmap
	/// </summary>
	public readonly Beatmap Beatmap;
	public readonly BeatmapSet BeatmapSet;

	private DrawableProgressBar _songProgressBar;

	public EditorScreen(BeatmapSet set, string beatmapId) {
		//Clone the beatmap to not edit the original
		this.BeatmapSet = set.Clone();

		//Get the beatmap from the cloned set, rather then the original
		this.Beatmap = this.BeatmapSet.Beatmaps.First(x => x.Id == beatmapId);
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
		this.Manager.Add(this._sceneOutline = new RectanglePrimitiveDrawable(this.ScenePosition, this.SceneSize, 1, false) {
			Clickable   = false,
			CoverClicks = false,
			Hoverable   = false,
			CoverHovers = false
		});

		//Add a song progress bar to the bottom of the screen
		this.Manager.Add(this._songProgressBar = new SongProgressBarDrawable {
			OriginType       = OriginType.BottomLeft,
			ScreenOriginType = OriginType.BottomLeft
		});

		this.InitializeKeybinds();
	}

	public override void Update(double gameTime) {
		base.Update(gameTime);

		this._openedThisFrame = false;

		this.AudioTime = pTypingGame.MusicTrack.CurrentPosition;
	}

	public override void Relayout(float newWidth, float newHeight) {
		base.Relayout(newWidth, newHeight);

		this.RelayoutScene(newWidth, newHeight);

		if (this._songProgressBar != null)
			this._songProgressBar.BarSize.X = newWidth;
	}

	public override void Dispose() {
		base.Dispose();

		this.RemoveKeybinds();
	}
}
