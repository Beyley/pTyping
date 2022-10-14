#nullable enable

using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Kettu;
using pTyping.Engine;
using pTyping.Graphics.Editor.Scene;
using pTyping.UiElements;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen {
	private EditorScene? _currentScene;

	private RectanglePrimitiveDrawable? _sceneOutline;

	private void LoadScene(EditorScene newScene) {
		if (newScene.GetType() == this._currentScene?.GetType())
			return;

		Logger.Log($"Loading editor scene {newScene.GetType().Name}!", LoggerLevelEditorInfo.Instance);
		
		// Unload the current scene, if there is one
		this.CloseScene();

		// Load the new scene
		newScene.Opening();

		// Set the current scene to the new scene
		this._currentScene = newScene;

		// Relayout the scene
		this.Relayout(FurballGame.WindowWidth, FurballGame.WindowHeight);

		//Position the scene to the center of the empty space
		this._currentScene.Position = this.ScenePosition;

		// Add the new scene to the editor
		this.Manager.Add(newScene);
	}

	private void CloseScene() {
		// If there is no current scene, return
		if (this._currentScene == null)
			return;

		// Close the current scene
		this._currentScene.Closing();

		// Remove the current scene from the editor
		this.Manager.Remove(this._currentScene);

		// Set the current scene to null
		this._currentScene = null;
	}

	public override void Relayout(float newWidth, float newHeight) {
		base.Relayout(newWidth, newHeight);

		if (this._currentScene == null)
			return;

		this.SceneSize = new Vector2(newWidth - MARGIN_AROUND_SCENE * 2, newHeight - ToolbarDrawable.HEIGHT - MARGIN_AROUND_SCENE * 2);

		// Relayout the scene
		this._currentScene?.Relayout(this.SceneSize.X, this.SceneSize.Y);

		if (this._sceneOutline != null)
			this._sceneOutline.RectSize = this.SceneSize;
	}
}
