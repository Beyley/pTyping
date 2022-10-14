namespace pTyping.Graphics.Editor.Scene.LyricEditor;

public class LyricEditorScene : EditorScene {
	private readonly LyricEditorContents _contents;

	public LyricEditorScene(EditorScreen editor) : base(editor) {
		this._contents = new LyricEditorContents(editor);

		this.Children.Add(this._contents);
	}

	public override void Opening() {}

	public override void Closing() {}

	public override void Relayout(float newWidth, float newHeight) {
		this._contents.Relayout(newWidth, newHeight);
	}
}
