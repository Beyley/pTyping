using System;
using System.Collections.Generic;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
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
	}
}
