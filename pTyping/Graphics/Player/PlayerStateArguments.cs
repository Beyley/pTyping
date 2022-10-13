namespace pTyping.Graphics.Player;

public class PlayerStateArguments {
	public static readonly PlayerStateArguments DefaultPlayer = new PlayerStateArguments {
		DisplayRomaji = true
	};
	public static readonly PlayerStateArguments DefaultEditor = new PlayerStateArguments {
		DisableTyping                  = true,
		DisableHitResults              = true,
		DisableMapEnding               = true,
		DisablePlayerMusicTrackControl = true,
		UseEditorNoteSpawnLogic        = true
	};

	/// <summary>
	///     Whether to forcefully disable the logic related to typing notes.
	/// </summary>
	public bool DisableTyping;
	/// <summary>
	///     Whether to forcefully disable the logic related to calculating hit results.
	/// </summary>
	public bool DisableHitResults;
	/// <summary>
	///     Whether to forcefully disable the logic related to ending the map.
	/// </summary>
	public bool DisableMapEnding;
	/// <summary>
	///     Whether to forcefully disable the logic related to controlling the music track.
	/// </summary>
	public bool DisablePlayerMusicTrackControl;
	/// <summary>
	///     Whether to use the editor's note spawn logic, which supports going backwards in time.
	/// </summary>
	public bool UseEditorNoteSpawnLogic;
	public bool DisplayRomaji;
}
