using System.Numerics;

namespace pTyping.Graphics.Editor.Scene.NoteEditor;

public sealed class NoteEditorScene : EditorScene {
	private readonly NoteEditorToolSelectionDrawable _toolSelection;
	private readonly NoteEditorPlayFieldContainer    _playFieldContainer;

	private const float MARGIN = 5f;

	public NoteEditorScene(EditorScreen editor) : base(editor) {
		this._toolSelection = new NoteEditorToolSelectionDrawable(new Vector2(MARGIN), () => {});
		this._playFieldContainer = new NoteEditorPlayFieldContainer(
			editor, new Vector2(this._toolSelection.Size.X + MARGIN * 2, MARGIN),
			Vector2.Zero
		);

		this.Children.Add(this._toolSelection);
		this.Children.Add(this._playFieldContainer);
	}

	public override void Opening() {
		//
	}

	public override void Closing() {
		//
	}

	public override void Relayout(float newWidth, float newHeight) {
		this._playFieldContainer.SizeOverride = new Vector2(this.Size.X - this._toolSelection.Size.X - MARGIN * 3f, this.Size.Y - MARGIN * 2f);

		this._playFieldContainer.Relayout();
	}
}
