using System;
using System.Collections.Generic;
using pTyping.Graphics.Editor.Scene;
using pTyping.UiElements;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen : pScreen {
	public override void Initialize() {
		base.Initialize();

		//Add the toolbar
		this.Manager.Add(new ToolbarDrawable(new Dictionary<string, Action> {
			{ "File", this.OpenFileMenu }
		}, pTypingGame.JapaneseFont));

		//Load the default scene when you first start the editor
		this.LoadScene(new NoteEditorScene(this));
	}
}
