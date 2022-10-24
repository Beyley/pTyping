using Furball.Engine.Engine.Input.Events;

namespace pTyping.Graphics.Editor.Scene.NoteEditor.Tools;

public abstract class NoteEditorTool {
	public bool DisplayMousePosition  = false;
	public bool TimeSnapMousePosition = true;

	public bool AllowSelectionOfNotes = false;

	public double MouseTime;

	public virtual void OnTimelineClick(NoteEditorScene scene, Player.Player player, MouseButtonEventArgs mouseButtonEventArgs) {}
}
