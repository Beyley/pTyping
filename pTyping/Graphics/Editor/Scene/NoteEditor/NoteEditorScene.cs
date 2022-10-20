using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Input;
using Furball.Engine.Engine.Input.Events;
using pTyping.Graphics.Editor.Scene.NoteEditor.Tools;
using Silk.NET.Input;

namespace pTyping.Graphics.Editor.Scene.NoteEditor;

public sealed class NoteEditorScene : EditorScene {
	public readonly  NoteEditorToolSelectionDrawable ToolSelection;
	private readonly NoteEditorPlayFieldContainer    _playFieldContainer;

	private Keybind _selectToolKeybind;
	private Keybind _noteToolKeybind;
	private Keybind _typingCutoffToolKeybind;

	private enum Keybinds {
		//Tools
		SelectTool,
		NoteTool,
		TypingCutoffTool
	}
	
	private const float MARGIN = 5f;

	public NoteEditorScene(EditorScreen editor) : base(editor) {
		this.ToolSelection = new NoteEditorToolSelectionDrawable(new Vector2(MARGIN), () => {});
		this._playFieldContainer = new NoteEditorPlayFieldContainer(
			editor, new Vector2(this.ToolSelection.Size.X + MARGIN * 2, MARGIN),
			Vector2.Zero
		);

		this.Children.Add(this.ToolSelection);
		this.Children.Add(this._playFieldContainer);
	}

	public override void Opening() {
		FurballGame.InputManager.RegisterKeybind(this._selectToolKeybind       = new Keybind(Keybinds.SelectTool, "Select Tool", Key.Number1, this.ActivateSelectTool));
		FurballGame.InputManager.RegisterKeybind(this._noteToolKeybind         = new Keybind(Keybinds.NoteTool, "Note Tool", Key.Number2, this.ActivateNoteTool));
		FurballGame.InputManager.RegisterKeybind(this._typingCutoffToolKeybind = new Keybind(Keybinds.TypingCutoffTool, "Typing Cutoff Tool", Key.Number3, this.ActivateTypingCutoffTool));
	}

	public override void KeyDown(KeyEventArgs keyEventArgs) {
		if (keyEventArgs.Key == Key.Delete)
			this._playFieldContainer.DeleteSelected();
	}

	private void ActivateSelectTool(FurballKeyboard keyboard) {
		//Ignore this bind if the user is typing somewhere
		if (FurballGame.InputManager.CharInputHandler != null)
			return;

		this.ToolSelection.SelectTool(new SelectTool());
	}

	private void ActivateNoteTool(FurballKeyboard keyboard) {
		//Ignore this bind if the user is typing somewhere
		if (FurballGame.InputManager.CharInputHandler != null)
			return;

		this.ToolSelection.SelectTool(new NoteTool());
	}

	private void ActivateTypingCutoffTool(FurballKeyboard keyboard) {
		//Ignore this bind if the user is typing somewhere
		if (FurballGame.InputManager.CharInputHandler != null)
			return;

		this.ToolSelection.SelectTool(new TypingCutoffTool());
	}

	public override void Closing() {
		FurballGame.InputManager.UnregisterKeybind(this._selectToolKeybind);
		FurballGame.InputManager.UnregisterKeybind(this._noteToolKeybind);
		FurballGame.InputManager.UnregisterKeybind(this._typingCutoffToolKeybind);
	}

	public override void Relayout(float newWidth, float newHeight) {
		this._playFieldContainer.SizeOverride = new Vector2(this.Size.X - this.ToolSelection.Size.X - MARGIN * 3f, this.Size.Y - MARGIN * 2f);

		this._playFieldContainer.Relayout();
	}
}
