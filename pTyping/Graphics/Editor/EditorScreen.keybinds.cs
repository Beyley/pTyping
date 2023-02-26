using System;
using Furball.Engine;
using Furball.Engine.Engine.Input;
using Furball.Engine.Engine.Input.Events;
using ManagedBass;
using Silk.NET.Input;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen {
	private Keybind _pausePlayKeybind;
	private Keybind _saveKeybind;

	private enum Keybinds {
		PausePlay,
		Save
	}

	private void InitializeKeybinds() {
		FurballGame.InputManager.RegisterKeybind(this._pausePlayKeybind = new Keybind(Keybinds.PausePlay, "Pause/Play", Key.Space, Array.Empty<Key>(), this.PausePlay));
		FurballGame.InputManager.RegisterKeybind(this._saveKeybind = new Keybind(Keybinds.Save, "Save", Key.S, new[] {
			Key.ControlLeft
		}, this.SaveKeybind));

		FurballGame.InputManager.OnMouseScroll += this.MouseScroll;
		FurballGame.InputManager.OnMouseDown   += this.MouseDown;
		FurballGame.InputManager.OnKeyDown     += this.KeyDown;
	}

	private void SaveKeybind(KeyEventArgs keyEventArgs) {
		this.Save();
	}

	private void KeyDown(object sender, KeyEventArgs e) {
		if (FurballGame.InputManager.CharInputHandler != null)
			return;

		this._currentScene?.KeyDown(e);
	}

	private void MouseDown(object sender, MouseButtonEventArgs e) {
		if (this._openContextMenu?.IsHovered ?? true)
			return;

		this.CloseCurrentContextMenu();
	}

	private void MouseScroll(object sender, MouseScrollEventArgs e) {
		this.MoveTimeByNBeats(Math.Sign(-e.ScrollAmount.Y));
	}

	private void PausePlay(KeyEventArgs keyEventArgs) {
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

		FurballGame.InputManager.OnMouseScroll -= this.MouseScroll;
		FurballGame.InputManager.OnMouseDown   -= this.MouseDown;
	}
}
