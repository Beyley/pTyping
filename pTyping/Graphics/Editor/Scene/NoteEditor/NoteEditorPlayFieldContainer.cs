using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input.Events;
using pTyping.Graphics.Drawables;
using pTyping.Graphics.Player;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.Mods;
using pTyping.UiElements;
using Silk.NET.Input;

namespace pTyping.Graphics.Editor.Scene.NoteEditor;

public sealed class NoteEditorPlayFieldContainer : CompositeDrawable {
	private readonly RectanglePrimitiveDrawable _outline;
	public readonly  List<Player.Player>        Players;
	public readonly  PlayerStateArguments       Arguments;

	public readonly EditorScreen Editor;

	public override Vector2 Size => this.SizeOverride * this.Scale;

	public Vector2 SizeOverride;

	public NoteEditorPlayFieldContainer(EditorScreen editor, Vector2 position, Vector2 size) {
		this.Position     = position;
		this.Editor       = editor;
		this.SizeOverride = size;

		this.InvisibleToInput = true;

		this._outline = new RectanglePrimitiveDrawable(Vector2.Zero, this.Size, 1, false) {
			Depth       = 1,
			Clickable   = false,
			CoverClicks = false,
			Hoverable   = false,
			CoverHovers = false
		};

		this.Children.Add(this._outline);

		this.Arguments               = PlayerStateArguments.DefaultEditor;
		this.Arguments.SelectedNotes = new ObservableCollection<SelectableCompositeDrawable>();

		this.Players = new List<Player.Player>();

		this.CreateNewPlayer();
	}

	public void CreateNewPlayer() {
		Player.Player player = new Player.Player(this.Editor.Beatmap, Array.Empty<Mod>(), this.Arguments) {
			OriginType = OriginType.LeftCenter
		};

		foreach (NoteDrawable noteDrawable in player.Notes)
			this.HandleNoteCreated(noteDrawable);

		this.Players.Add(player);

		this.Children.Add(player);

		this.RelayoutPlayers();
	}

	public void HandleNoteCreated(NoteDrawable note) {
		note.OnClick += this.NoteClick;
	}

	private void NoteClick(object sender, MouseButtonEventArgs e) {
		NoteDrawable note = (NoteDrawable)sender;

		if (e.Button == MouseButton.Right) {
			ContextMenuDrawable rightClickMenu = new ContextMenuDrawable(e.Mouse.Position, new List<(string, Action)> {
				("Delete", () => {
					foreach (Player.Player player in this.Players)
						player.RemoveNote(note);

					this.Arguments.SelectedNotes.Remove(note);

					this.Editor.CloseCurrentContextMenu();
				})
			}, pTypingGame.JapaneseFont, 24);

			this.Editor.OpenContextMenu(rightClickMenu);
		}
	}

	private void RelayoutPlayers() {
		for (int i = 0; i < this.Players.Count; i++) {
			Player.Player player = this.Players[i];

			player.Position = new Vector2(0, this.Size.Y / (this.Players.Count + 1) * (i + 1));
		}
	}

	public void Relayout() {
		this._outline.RectSize = this.Size;

		foreach (Player.Player player in this.Players)
			player.Scale = new Vector2(this.Size.X / FurballGame.DEFAULT_WINDOW_WIDTH);

		this.RelayoutPlayers();
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		batch.ScissorPush(this, new Rectangle(this.RealPosition.ToPoint(), (this.RealSize + new Vector2(2, 0)).ToSize()));

		base.Draw(time, batch, args);

		batch.ScissorPop(this);
	}

	public void DeleteSelected() {
		foreach (Player.Player player in this.Players) {
			foreach (SelectableCompositeDrawable selectedNote in this.Arguments.SelectedNotes)
				player.RemoveNote(selectedNote);
		}

		//Clear the list of selected notes, as they are now deleted
		this.Arguments.SelectedNotes.Clear();
	}

	public string AddNote(HitObject hitObject) {
		foreach (HitObject x in this.Editor.Beatmap.HitObjects)
			//Dont allow you to place 2 notes on the same time (with a tolerance of 10ms)
			if (Math.Abs(x.Time - hitObject.Time) < 10)
				return "Notes cannot overlap!";

		this.Editor.Beatmap.HitObjects.Add(hitObject);

		foreach (Player.Player player in this.Players) {
			NoteDrawable noteDrawable = player.CreateNote(hitObject);

			player.AddNote(noteDrawable);
		}

		return "";
	}
}
