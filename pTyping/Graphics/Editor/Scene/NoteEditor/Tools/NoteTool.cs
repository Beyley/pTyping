using Furball.Engine.Engine.Input.Events;
using pTyping.Shared.Beatmaps.HitObjects;
using Silk.NET.Input;

namespace pTyping.Graphics.Editor.Scene.NoteEditor.Tools;

public class NoteTool : NoteEditorTool {
	public NoteTool() {
		this.DisplayMousePosition  = true;
		this.TimeSnapMousePosition = true;
	}

	public override void OnTimelineClick(NoteEditorScene scene, Player.Player player, MouseButtonEventArgs mouseButtonEventArgs) {
		base.OnTimelineClick(scene, player, mouseButtonEventArgs);

		if (mouseButtonEventArgs.Button == MouseButton.Left) {
			HitObject hitObject = new HitObject {
				Time = this.MouseTime
			};

			string result = scene.PlayFieldContainer.AddNote(hitObject);
			if (!string.IsNullOrWhiteSpace(result))
				pTypingGame.NotificationManager.CreatePopup($"Unable to create note! {result}");
		}
	}
}
