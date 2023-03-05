using System.Collections.ObjectModel;
using Furball.Engine.Engine.Helpers;
using pTyping.Graphics.Drawables;
using pTyping.Shared;

namespace pTyping.Graphics.Player;

public class PlayerStateArguments {
	public static PlayerStateArguments DefaultPlayer => new PlayerStateArguments {
		DisplayRomaji = true
	};
	public static PlayerStateArguments DefaultEditor => new PlayerStateArguments {
		DisableTyping                  = true,
		DisableHitResults              = true,
		DisableMapEnding               = true,
		DisablePlayerMusicTrackControl = true,
		UseEditorNoteSpawnLogic        = true,
		EnableSelection                = new Bindable<bool>(true)
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
	public bool Controller;

	public Bindable<bool> EnableSelection = new Bindable<bool>(false);

	public ReaderWriterLockedObject<ObservableCollection<SelectableCompositeDrawable>> SelectedNotes = new ReaderWriterLockedObject<ObservableCollection<SelectableCompositeDrawable>>(new ObservableCollection<SelectableCompositeDrawable>());
}
