using System;
using Furball.Engine;
using Furball.Engine.Engine.Input;
using Furball.Engine.Engine.Input.Events;
using ManagedBass;
using pTyping.Graphics.Editor.Scene.NoteEditor;
using pTyping.Graphics.Editor.Scene.NoteEditor.Tools;
using Silk.NET.Input;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen {
	private Keybind _pausePlayKeybind;
	private Keybind _selectToolKeybind;
	private Keybind _noteToolKeybind;
	private Keybind _typingCutoffToolKeybind;

	private enum Keybinds {
		PausePlay,

		//Tools
		SelectTool,
		NoteTool,
		TypingCutoffTool
	}

	private void InitializeKeybinds() {
		FurballGame.InputManager.RegisterKeybind(this._pausePlayKeybind        = new Keybind(Keybinds.PausePlay, "Pause/Play", Key.Space, this.PausePlay));
		FurballGame.InputManager.RegisterKeybind(this._selectToolKeybind       = new Keybind(Keybinds.SelectTool, "Select Tool", Key.Number1, this.ActivateSelectTool));
		FurballGame.InputManager.RegisterKeybind(this._noteToolKeybind         = new Keybind(Keybinds.NoteTool, "Note Tool", Key.Number2, this.ActivateNoteTool));
		FurballGame.InputManager.RegisterKeybind(this._typingCutoffToolKeybind = new Keybind(Keybinds.TypingCutoffTool, "Typing Cutoff Tool", Key.Number3, this.ActivateTypingCutoffTool));

		FurballGame.InputManager.OnMouseScroll += this.MouseScroll;
		FurballGame.InputManager.OnMouseDown   += this.MouseDown;
	}

	private void ActivateSelectTool(FurballKeyboard keyboard) {
		//Ignore this bind if the user is typing somewhere
		if (FurballGame.InputManager.CharInputHandler != null)
			return;
		
		if (this._currentScene is not NoteEditorScene noteEditorScene)
			return;

		noteEditorScene.ToolSelection.SelectTool(new SelectTool());
	}

	private void ActivateNoteTool(FurballKeyboard keyboard) {
		//Ignore this bind if the user is typing somewhere
		if (FurballGame.InputManager.CharInputHandler != null)
			return;
		
		if (this._currentScene is not NoteEditorScene noteEditorScene)
			return;

		noteEditorScene.ToolSelection.SelectTool(new NoteTool());
	}

	private void ActivateTypingCutoffTool(FurballKeyboard keyboard) {
		//Ignore this bind if the user is typing somewhere
		if (FurballGame.InputManager.CharInputHandler != null)
			return;
		
		if (this._currentScene is not NoteEditorScene noteEditorScene)
			return;

		noteEditorScene.ToolSelection.SelectTool(new TypingCutoffTool());
	}

	private void MouseDown(object sender, MouseButtonEventArgs e) {
		if (this._openContextMenu?.IsHovered ?? true)
			return;

		this.CloseCurrentContextMenu();
	}

	private void MouseScroll(object sender, MouseScrollEventArgs e) {
		this.MoveTimeByNBeats(Math.Sign(-e.ScrollAmount.Y));
	}

	private void PausePlay(FurballKeyboard keyboard) {
		//Ignore this bind if the user is typing somewhere
		if (FurballGame.InputManager.CharInputHandler != null)
			return;

		if (pTypingGame.MusicTrack.PlaybackState == PlaybackState.Playing)
			pTypingGame.MusicTrack.Pause();
		else
			pTypingGame.MusicTrack.Resume();
	}

	private void RemoveKeybinds() {
		FurballGame.InputManager.UnregisterKeybind(this._pausePlayKeybind);
		FurballGame.InputManager.UnregisterKeybind(this._selectToolKeybind);
		FurballGame.InputManager.UnregisterKeybind(this._noteToolKeybind);
		FurballGame.InputManager.UnregisterKeybind(this._typingCutoffToolKeybind);

		FurballGame.InputManager.OnMouseScroll -= this.MouseScroll;
		FurballGame.InputManager.OnMouseDown   -= this.MouseDown;
	}
}
