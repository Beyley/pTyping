using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Helpers;
using pTyping.Graphics.Player;
using pTyping.Shared.Mods;

namespace pTyping.Graphics.Editor.Scene.NoteEditor;

public sealed class NoteEditorPlayFieldContainer : CompositeDrawable {
	private readonly RectanglePrimitiveDrawable _outline;
	private readonly List<Player.Player>        _players;

	private readonly EditorScreen _editor;

	public override Vector2 Size => this.SizeOverride * this.Scale;

	public Vector2 SizeOverride;

	public NoteEditorPlayFieldContainer(EditorScreen editor, Vector2 position, Vector2 size) {
		this.Position     = position;
		this._editor      = editor;
		this.SizeOverride = size;

		this._outline = new RectanglePrimitiveDrawable(Vector2.Zero, this.Size, 1, false) {
			Depth = 1
		};

		this.Children.Add(this._outline);

		this._players = new List<Player.Player>();

		//TODO: support mapping multiple players
		this.CreateNewPlayer();
	}

	public void CreateNewPlayer() {
		Player.Player player = new Player.Player(this._editor.Beatmap, Array.Empty<Mod>(), PlayerStateArguments.DefaultEditor) {
			OriginType = OriginType.LeftCenter
		};

		this._players.Add(player);

		this.Children.Add(player);

		this.RelayoutPlayers();
	}

	private void RelayoutPlayers() {
		for (int i = 0; i < this._players.Count; i++) {
			Player.Player player = this._players[i];

			//TODO: handle multiple players properly
			player.Position = new Vector2(0, this.Size.Y / 2f);
		}
	}

	public void Relayout() {
		this._outline.RectSize = this.Size;

		foreach (Player.Player player in this._players)
			player.Scale = new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / this.Size.X);

		this.RelayoutPlayers();
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		batch.ScissorPush(this, new Rectangle(this.RealPosition.ToPoint(), (this.RealSize + new Vector2(2, 0)).ToSize()));

		base.Draw(time, batch, args);

		batch.ScissorPop(this);
	}
}
