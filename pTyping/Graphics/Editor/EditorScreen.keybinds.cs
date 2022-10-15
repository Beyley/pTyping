using Furball.Engine;
using Furball.Engine.Engine.Input;
using ManagedBass;
using Silk.NET.Input;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen {
	private Keybind _pausePlayKeybind;

	private enum Keybinds {
		PausePlay
	}

	private void InitializeKeybinds() {
		FurballGame.InputManager.RegisterKeybind(this._pausePlayKeybind = new Keybind(Keybinds.PausePlay, "Pause/Play", Key.Space, this.PausePlay));
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
	}
}
