using System;
using System.Collections.Generic;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using pTyping.Graphics.Player;
using pTyping.Shared.Mods;

namespace pTyping.Graphics.Editor.Scene.NoteEditor;

public sealed class NoteEditorPlayFieldContainer : CompositeDrawable {
	private readonly RectanglePrimitiveDrawable _outline;
	private readonly List<Player.Player>        _players;

	private readonly EditorScreen _editor;

	public override Vector2 Size => this.SizeOverride;

	public Vector2 SizeOverride;

	public NoteEditorPlayFieldContainer(EditorScreen editor, Vector2 position, Vector2 size) {
		this.Position     = position;
		this._editor      = editor;
		this.SizeOverride = size;

		this._outline = new RectanglePrimitiveDrawable(Vector2.Zero, this.Size, 1, false);

		this.Children.Add(this._outline);

		this._players = new List<Player.Player>();
	}

	public void CreateNewPlayer() {
		Player.Player player = new Player.Player(this._editor.Beatmap, Array.Empty<Mod>(), PlayerStateArguments.DefaultEditor);

		this._players.Add(player);

		this.Children.Add(player);
	}

	public void Relayout() {
		this._outline.RectSize = this.Size;
	}
}
