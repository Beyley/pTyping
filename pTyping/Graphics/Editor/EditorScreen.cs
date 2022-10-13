using System;
using System.Collections.Generic;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using pTyping.Graphics.Editor.Scene;
using pTyping.UiElements;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen : pScreen {
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

		this.Manager.Add(this._sceneOutline = new RectanglePrimitiveDrawable(this.ScenePosition, this.SceneSize, 1, false));
	}
}
