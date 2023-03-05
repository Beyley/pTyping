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
using pTyping.Shared;
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

	public double MouseTime;

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
		this.Arguments.SelectedNotes = new ReaderWriterLockedObject<ObservableCollection<SelectableCompositeDrawable>>(new ObservableCollection<SelectableCompositeDrawable>());

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
		note.OnClick     += this.NoteClick;
		note.OnDragBegin += this.NoteDragBegin;
		note.OnDrag      += this.NoteDrag;
	}

	private void NoteClick(object sender, MouseButtonEventArgs e) {
		NoteDrawable note = (NoteDrawable)sender;

		if (e.Button == MouseButton.Right) {
			ContextMenuDrawable rightClickMenu = new ContextMenuDrawable(e.Mouse.Position, new List<(string, Action)> {
				("Delete", () => {
					foreach (Player.Player player in this.Players)
						player.RemoveNote(note);

					this.Arguments.SelectedNotes.Lock.EnterWriteLock();
					this.Arguments.SelectedNotes.Object.Remove(note);
					this.Arguments.SelectedNotes.Lock.ExitWriteLock();

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
		this.Arguments.SelectedNotes.Lock.EnterWriteLock();
		foreach (Player.Player player in this.Players) {
			foreach (SelectableCompositeDrawable selectedNote in this.Arguments.SelectedNotes.Object)
				player.RemoveNote(selectedNote);
		}

		//Clear the list of selected notes, as they are now deleted
		this.Arguments.SelectedNotes.Object.Clear();
		this.Arguments.SelectedNotes.Lock.ExitWriteLock();
	}

	public string AddNote(HitObject hitObject) {
		foreach (HitObject x in this.Editor.Beatmap.HitObjects)
			//Dont allow you to place 2 notes on the same time (with a tolerance of 10ms)
			if (Math.Abs(x.Time - hitObject.Time) < 10)
				return "Notes cannot overlap!";

		this.Editor.Beatmap.HitObjects.Add(hitObject);

		foreach (Player.Player player in this.Players) {
			NoteDrawable note = player.CreateNote(hitObject);

			this.HandleNoteCreated(note);

			player.AddNote(note);
		}

		return "";
	}

	private double                       _dragStartingMouseTime;
	private List<(NoteDrawable, double)> _dragStartingTimes = new List<(NoteDrawable, double)>();
	private void NoteDragBegin(object sender, MouseDragEventArgs e) {
		this._dragStartingMouseTime = this.MouseTime;

		this._dragStartingTimes.Clear();
		this.Arguments.SelectedNotes.Lock.EnterReadLock();
		foreach (SelectableCompositeDrawable selectableCompositeDrawable in this.Arguments.SelectedNotes.Object) {
			this._dragStartingTimes.Add(((NoteDrawable)selectableCompositeDrawable, ((NoteDrawable)selectableCompositeDrawable).Note.Time));
		}
		this.Arguments.SelectedNotes.Lock.ExitReadLock();
	}

	private void NoteDrag(object sender, MouseDragEventArgs e) {
		//Difference between the mouse time and the time when the drag started
		double timeDifference = this.MouseTime - this._dragStartingMouseTime;

		//For each selected note, move it to the time it was at when the drag started, plus the time difference
		for (int i = 0; i < this._dragStartingTimes.Count; i++) {
			NoteDrawable note = this._dragStartingTimes[i].Item1;

			note.Note.Time = this._dragStartingTimes[i].Item2 + timeDifference;

			//Set the tweens of the note
			note.CreateTweens(new GameplayDrawableTweenArgs(this.Players[0].CurrentApproachTime(note.Note.Time), this.Arguments.UseEditorNoteSpawnLogic));
		}
	}
}
