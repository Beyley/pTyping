using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Editor.Scene.NoteEditor.Tools;
using Silk.NET.Input;

namespace pTyping.Graphics.Editor.Scene.NoteEditor;

public sealed class NoteEditorScene : EditorScene {
	public readonly  NoteEditorPlayFieldContainer    PlayFieldContainer;
	public readonly  NoteEditorToolSelectionDrawable ToolSelection;
	private readonly TexturedDrawable                _mouseTimeDisplay;

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
		this.ToolSelection = new NoteEditorToolSelectionDrawable(new Vector2(MARGIN), () => {
			this.PlayFieldContainer!.Arguments.EnableSelection.Value = this.ToolSelection!.CurrentTool.AllowSelectionOfNotes;
		});
		this.PlayFieldContainer = new NoteEditorPlayFieldContainer(
			editor, new Vector2(this.ToolSelection.Size.X + MARGIN * 2, MARGIN),
			Vector2.Zero
		);
		this._mouseTimeDisplay = new TexturedDrawable(FurballGame.WhitePixel, Vector2.Zero) {
			Visible       = false,
			Scale         = new Vector2(1, 100),
			TimeSource    = pTypingGame.MusicTrackTimeSource,
			OriginType    = OriginType.Center,
			ColorOverride = new Color(255, 255, 255, 100)
		};

		this.Children.Add(this.ToolSelection);
		this.Children.Add(this.PlayFieldContainer);
	}

	private void ContainerClick(object sender, MouseButtonEventArgs e) {
		foreach (Player.Player player in this.PlayFieldContainer.Players)
			if (player.RealRectangle.Contains(e.Mouse.Position.ToPointF())) {
				this.ToolSelection.CurrentTool.OnTimelineClick(this, player, e);
				return;
			}
	}

	public override void Opening() {
		FurballGame.InputManager.RegisterKeybind(this._selectToolKeybind       = new Keybind(Keybinds.SelectTool, "Select Tool", Key.Number1, this.ActivateSelectTool));
		FurballGame.InputManager.RegisterKeybind(this._noteToolKeybind         = new Keybind(Keybinds.NoteTool, "Note Tool", Key.Number2, this.ActivateNoteTool));
		FurballGame.InputManager.RegisterKeybind(this._typingCutoffToolKeybind = new Keybind(Keybinds.TypingCutoffTool, "Typing Cutoff Tool", Key.Number3, this.ActivateTypingCutoffTool));

		FurballGame.InputManager.OnMouseMove += this.MouseMove;
		FurballGame.InputManager.OnMouseDown += this.ContainerClick;
	}

	public override void Closing() {
		FurballGame.InputManager.UnregisterKeybind(this._selectToolKeybind);
		FurballGame.InputManager.UnregisterKeybind(this._noteToolKeybind);
		FurballGame.InputManager.UnregisterKeybind(this._typingCutoffToolKeybind);

		FurballGame.InputManager.OnMouseMove -= this.MouseMove;
		FurballGame.InputManager.OnMouseDown -= this.ContainerClick;
	}

	private void MouseMove(object sender, MouseMoveEventArgs e) {
		if (!this.ToolSelection.CurrentTool.DisplayMousePosition)
			return;

		bool already = false;
		foreach (Player.Player player in this.PlayFieldContainer.Players) {
			//If the mouse is in the playfield, dont display the time, and skip to the next player
			if (already || !player.RealContains(e.Position)) {
				this._mouseTimeDisplay.Visible = false;
				player.Children.Remove(this._mouseTimeDisplay);
				continue;
			}

			//Convert to drawable coordinates
			Vector2 relativePos = e.Position - player.RealPosition;
			relativePos /= player.RealScale;

			double currentTime  = this.Editor.AudioTime;
			double reticuleXPos = player.Recepticle.Position.X;
			double noteStartPos = Player.Player.NOTE_START_POS.X;

			double currentApproachTime = player.CurrentApproachTime(currentTime);
			double speed               = (noteStartPos - reticuleXPos) / currentApproachTime;

			double relativeMousePosition = relativePos.X - reticuleXPos;

			double timeAtCursor = relativeMousePosition / speed + currentTime;

			if (this.ToolSelection.CurrentTool.TimeSnapMousePosition)
				timeAtCursor = this.Editor.SnapTime(timeAtCursor);

			this.ToolSelection.CurrentTool.MouseTime = timeAtCursor;

			this._mouseTimeDisplay.Visible = true;

			this.UpdateMouseTimeDisplay(timeAtCursor, currentApproachTime);
			player.Children.Add(this._mouseTimeDisplay);

			already = true;
		}
	}

	private void UpdateMouseTimeDisplay(double mouseTime, double approachTime) {
		this._mouseTimeDisplay.Tweens.Clear();

		Vector2 noteStartPos  = Player.Player.NOTE_START_POS;
		Vector2 noteEndPos    = Player.Player.NOTE_END_POS;
		Vector2 recepticlePos = Player.Player.RECEPTICLE_POS;

		float travelDistance = noteStartPos.X - recepticlePos.X;
		float travelRatio    = (float)(approachTime / travelDistance);

		float afterTravelTime = (recepticlePos.X - noteEndPos.X) * travelRatio;

		this._mouseTimeDisplay.Tweens.Add(
			new VectorTween(
				TweenType.Movement,
				new Vector2(noteStartPos.X, noteStartPos.Y),
				recepticlePos,
				(int)(mouseTime - approachTime),
				(int)mouseTime
			) {
				KeepAlive = true
			}
		);

		this._mouseTimeDisplay.Tweens.Add(
			new VectorTween(TweenType.Movement, recepticlePos, new Vector2(noteEndPos.X, recepticlePos.Y), (int)mouseTime, (int)(mouseTime + afterTravelTime)) {
				KeepAlive = true
			}
		);
	}

	public override void KeyDown(KeyEventArgs keyEventArgs) {
		if (keyEventArgs.Key == Key.Delete)
			this.PlayFieldContainer.DeleteSelected();
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

	public override void Relayout(float newWidth, float newHeight) {
		this.PlayFieldContainer.SizeOverride = new Vector2(this.Size.X - this.ToolSelection.Size.X - MARGIN * 3f, this.Size.Y - MARGIN * 2f);

		this.PlayFieldContainer.Relayout();
	}
}
