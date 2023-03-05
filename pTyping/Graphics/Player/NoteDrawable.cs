using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using FontStashSharp;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes.BezierPathTween;
using Furball.Vixie;
using Furball.Vixie.Backends.Shared;
using pTyping.Engine;
using pTyping.Graphics.Drawables;
using pTyping.Shared;
using pTyping.Shared.Beatmaps.HitObjects;
using Silk.NET.Input;

namespace pTyping.Graphics.Player;

public struct GameplayDrawableTweenArgs {
	public readonly double ApproachTime;
	public readonly bool   TweenKeepAlive;
	public readonly bool   IsEditor;

	public GameplayDrawableTweenArgs(double approachTime, bool tweenKeepAlive = false, bool isEditor = false) {
		this.ApproachTime   = approachTime;
		this.TweenKeepAlive = tweenKeepAlive;
		this.IsEditor       = isEditor;
	}
}

public class NoteDrawable : SelectableCompositeDrawable {
	public TextDrawable     RawTextDrawable;
	public TextDrawable     ToTypeTextDrawable;
	public TexturedDrawable NoteTexture;

	public HitObject Note;
	public bool      Added = false;

	public Texture Texture;

	public bool   EditorHitSoundQueued = false;
	public double TimeDifference;

	public NoteDrawable(Vector2 position, Texture texture, FontSystem font, int size, ReaderWriterLockedObject<ObservableCollection<SelectableCompositeDrawable>> selectedObjects, PlayerStateArguments args) : base(selectedObjects, args.EnableSelection) {
		this.Position = position;
		this.Texture  = texture;

		this.NoteTexture = new TexturedDrawable(this.Texture, new Vector2(0)) {
			OriginType  = OriginType.TopLeft,
			Clickable   = false,
			CoverClicks = false
		};

		this.RawTextDrawable = new TextDrawable(new Vector2(this.NoteTexture.Size.X / 2f, this.NoteTexture.Size.Y + 20), font, "", size) {
			Scale      = new Vector2(1.5f),
			OriginType = OriginType.TopCenter
		};
		this.ToTypeTextDrawable = new TextDrawable(new Vector2(this.NoteTexture.Size.X / 2f, this.RawTextDrawable.Position.Y + 40), font, "", size) {
			Scale      = new Vector2(1.5f),
			OriginType = OriginType.TopCenter
		};

		this.Children.Add(this.NoteTexture);
		this.Children.Add(this.RawTextDrawable);
		this.Children.Add(this.ToTypeTextDrawable);

		this.OriginType = OriginType.Center;
	}

	private readonly List<TexturedDrawable> _buttons = new List<TexturedDrawable>();
	public void SetButtons((string hiragana, ButtonName[]) noteButtonsToPress) {
		foreach (TexturedDrawable button in this._buttons)
			this.Children.Remove(button);

		this._buttons.Clear();

		float y = this.RawTextDrawable.Position.Y + this.RawTextDrawable.Size.Y + 10;
		for (int i = 0; i < noteButtonsToPress.Item2.Length; i++) {
			ButtonName button = noteButtonsToPress.Item2[i];
			string buttonTexturePath = button switch {
				ButtonName.A           => "ControllerIcons/Xbox One/XboxOne_A.png",
				ButtonName.B           => "ControllerIcons/Xbox One/XboxOne_B.png",
				ButtonName.X           => "ControllerIcons/Xbox One/XboxOne_X.png",
				ButtonName.Y           => "ControllerIcons/Xbox One/XboxOne_Y.png",
				ButtonName.LeftBumper  => "ControllerIcons/Xbox One/XboxOne_LB.png",
				ButtonName.RightBumper => "ControllerIcons/Xbox One/XboxOne_RB.png",
				ButtonName.LeftStick   => "ControllerIcons/Xbox One/XboxOne_Left_Stick_Click.png",
				ButtonName.RightStick  => "ControllerIcons/Xbox One/XboxOne_Right_Stick_Click.png",
				ButtonName.DPadUp      => "ControllerIcons/Xbox One/XboxOne_Dpad_Up.png",
				ButtonName.DPadRight   => "ControllerIcons/Xbox One/XboxOne_Dpad_Right.png",
				ButtonName.DPadDown    => "ControllerIcons/Xbox One/XboxOne_Dpad_Down.png",
				ButtonName.DPadLeft    => "ControllerIcons/Xbox One/XboxOne_Dpad_Left.png",
				_                      => throw new ArgumentOutOfRangeException()
			};

			bool odd = i % 2 != 0;

			TexturedDrawable buttonTexture = new TexturedDrawable(ContentManager.LoadTextureFromFileCached(buttonTexturePath, ContentSource.User), Vector2.Zero);
			buttonTexture.Position   = new Vector2(this.NoteTexture.Size.X / 2f, y);
			buttonTexture.OriginType = odd ? OriginType.TopLeft : OriginType.TopRight;
			buttonTexture.Scale      = new Vector2(0.7f);

			if (odd)
				y += buttonTexture.Size.Y;
			else if (i == noteButtonsToPress.Item2.Length - 1)
				buttonTexture.OriginType = OriginType.TopCenter;

			this.Children.Add(buttonTexture);
			this._buttons.Add(buttonTexture);
		}
	}

	public override Vector2 Size => (this.NoteTexture?.Size ?? Vector2.One) * this.Scale;

	public void CreateTweens(GameplayDrawableTweenArgs tweenArgs) {
		this.TweensLock.EnterWriteLock();
		this.Tweens.Clear();
		this.TweensLock.ExitWriteLock();

		Vector2 noteStartPos  = Player.NOTE_START_POS;
		Vector2 noteEndPos    = Player.NOTE_END_POS;
		Vector2 recepticlePos = Player.RECEPTICLE_POS;

		float travelDistance = noteStartPos.X - recepticlePos.X;
		float travelRatio    = (float)(tweenArgs.ApproachTime / travelDistance);

		float afterTravelTime = (recepticlePos.X - noteEndPos.X) * travelRatio;

		this.Tweens.Add(
			new VectorTween(
				TweenType.Movement,
				new Vector2(noteStartPos.X, noteStartPos.Y),
				recepticlePos,
				(int)(this.Note.Time - tweenArgs.ApproachTime),
				(int)this.Note.Time
			) {
				KeepAlive = tweenArgs.TweenKeepAlive
			}
		);

		this.Tweens.Add(
			new VectorTween(TweenType.Movement, recepticlePos, new Vector2(noteEndPos.X, recepticlePos.Y), (int)this.Note.Time, (int)(this.Note.Time + afterTravelTime)) {
				KeepAlive = tweenArgs.TweenKeepAlive
			}
		);
	}

	public void UpdateDrawables() {
		this.RawTextDrawable.Text   = this.Note.Text;
		this.RawTextDrawable.Colors = new FSColor[this.Note.Text.Length];
		for (var i = 0; i < this.RawTextDrawable.Colors.Length; i++) {
			this.RawTextDrawable.Colors[i] = FSColor.White;
		}
		this.ToTypeTextDrawable.Text = "";

		this.NoteTexture.FadeColor(this.Note.Color, 100);
	}

	public void Reset() {
		this.RawTextDrawable.Text    = this.Note.Text;
		this.ToTypeTextDrawable.Text = "";

		this.NoteTexture.ColorOverride = this.Note.Color;
	}

	/// <summary>
	///     Updates the positions of the text
	/// </summary>
	public void UpdateTextPositions() {
		this.RawTextDrawable.Position = new Vector2(this.NoteTexture.Size.X / 2f, this.NoteTexture.Size.Y + 20);

		this.ToTypeTextDrawable.Position = new Vector2(this.NoteTexture.Size.X / 2f, this.RawTextDrawable.Position.Y + 80);
	}

	public bool ButtonPress(string hiragana, double timeDifference, double rawTimeDifference, Player player) {
		//If this is the first time a character is being typed on this note, 
		if (this.Note.TypedRomaji.Length == 0 && this.Note.TypedText.Length == 0) {
			//Find the correct hit result for the users time
			if (timeDifference < player.TIMING_EXCELLENT)
				this.Note.HitResult = HitResult.Excellent;
			else if (timeDifference < player.TIMING_GOOD)
				this.Note.HitResult = HitResult.Good;
			else if (timeDifference < player.TIMING_FAIR)
				this.Note.HitResult = HitResult.Fair;
			else if (timeDifference < player.TIMING_POOR)
				this.Note.HitResult = HitResult.Poor;

			this.TimeDifference = rawTimeDifference;

			//Rotate the hue by 150 degrees
			Color finalColor = Helpers.RotateColor(this.Note.Color, 150);
			//The time it will take the note to fade to its partially hit state
			const int toFinalFadeTime = 100;
			//Set the notes colour to the final colour TODO: figure out why this is needed
			this.NoteTexture.ColorOverride = finalColor;
			this.NoteTexture.Tweens.Add(
				new ColorTween(TweenType.Color, this.NoteTexture.ColorOverride, finalColor, FurballGame.Time, FurballGame.Time + toFinalFadeTime)
			);
		}

		//Add the hiragana to the typed section
		this.Note.TypedText += hiragana;
		//Give the play the appropriate 
		player.Score.AddScore(Player.SCORE_PER_CHARACTER);

		//Are we finished with the whole note?
		if (string.Equals(this.Note.TypedText, this.Note.Text)) {
			this.Hit();
			return true;
		}

		return false;
	}

	/// <summary>
	///     Types a character
	/// </summary>
	/// <param name="hiragana">The hiragana being typed</param>
	/// <param name="romaji">The romaji path to take</param>
	/// <param name="timeDifference">The time difference from now to the note</param>
	/// <param name="score">The current score</param>
	/// <returns>Whether the note has been fully completed</returns>
	public bool TypeCharacter(string hiragana, string romaji, double timeDifference, double rawTimeDifference, Player player) {
		//If this is the first time a character is being typed on this note, 
		if (this.Note.TypedRomaji.Length == 0 && this.Note.TypedText.Length == 0) {
			//Find the correct hit result for the users time
			if (timeDifference < player.TIMING_EXCELLENT)
				this.Note.HitResult = HitResult.Excellent;
			else if (timeDifference < player.TIMING_GOOD)
				this.Note.HitResult = HitResult.Good;
			else if (timeDifference < player.TIMING_FAIR)
				this.Note.HitResult = HitResult.Fair;
			else if (timeDifference < player.TIMING_POOR)
				this.Note.HitResult = HitResult.Poor;

			this.TimeDifference = rawTimeDifference;

			//Rotate the hue by 150 degrees
			Color finalColor = Helpers.RotateColor(this.Note.Color, 150);
			//The time it will take the note to fade to its partially hit state
			const int toFinalFadeTime = 100;
			//Set the notes colour to the final colour TODO: figure out why this is needed
			this.NoteTexture.ColorOverride = finalColor;
			this.NoteTexture.Tweens.Add(
				new ColorTween(TweenType.Color, this.NoteTexture.ColorOverride, finalColor, FurballGame.Time, FurballGame.Time + toFinalFadeTime)
			);
		}

		//Add the next typed character to the string of currently typed romaji
		this.Note.TypedRomaji += romaji[this.Note.TypedRomaji.Length];

		//Have we finished checking th romaji sequence? if so,
		if (string.Equals(this.Note.TypedRomaji, romaji)) {
			//Add the hiragana to the typed section
			this.Note.TypedText += hiragana;
			//Give the play the appropriate 
			player.Score.AddScore(Player.SCORE_PER_CHARACTER);
			//Clear the typed romaji
			this.Note.TypedRomaji = string.Empty;

			//Are we finished with the whole note?
			if (string.Equals(this.Note.TypedText, this.Note.Text)) {
				this.Hit();
				return true;
			}
		}

		return false;
	}

	public void Hit() {
		this.ToTypeTextDrawable.Visible = false;
		this.RawTextDrawable.Visible    = false;

		Color     finalColor = Helpers.RotateColor(this.Note.Color, 150);
		const int timeToDie  = 250;

		//random bool
		bool right = FurballGame.Random.Next(-1, 2) == 1;

		this.TweensLock.EnterWriteLock();
		this.Tweens.Clear();
		this.TweensLock.ExitWriteLock();

		this.NoteTexture.Tweens.Add(
			new ColorTween(
				TweenType.Color,
				new Color((int)finalColor.R, finalColor.G, finalColor.B, 127),
				new Color((int)this.NoteTexture.ColorOverride.R, this.NoteTexture.ColorOverride.G, this.NoteTexture.ColorOverride.B, 0),
				FurballGame.Time,
				FurballGame.Time + timeToDie
			)
		);
		this.NoteTexture.Tweens.Add(
			new PathTween(
				new Path(
					new PathSegment(
						this.NoteTexture.Position,
						this.NoteTexture.Position + new Vector2(FurballGame.Random.Next(0, 30) * (right ? 1 : -1), -FurballGame.Random.Next(80, 120)),
						this.NoteTexture.Position + new Vector2(FurballGame.Random.Next(0, 60) * (right ? 1 : -1), 150)
					)
				),
				FurballGame.Time,
				FurballGame.Time + timeToDie
			)
		);
		// this.NoteTexture.Tweens.Add(new VectorTween(TweenType.Scale, this.NoteTexture.Scale, this.NoteTexture.Scale + new Vector2((float)(FurballGame.Random.NextDouble() / 4d + 1d)), (int)(FurballGame.Time + timeToDie / 2d), FurballGame.Time + timeToDie));
		this.Note.TypedText = this.Note.Text;

		FurballGame.GameTimeScheduler.ScheduleMethod(
			delegate {
				this.Visible = false;
			},
			FurballGame.Time + timeToDie
		);

		this.Note.Complete = true;
	}

	public void Miss() {
		// this.Visible        = false;
		pTypingGame.MusicTrackScheduler.ScheduleMethod(
			delegate {
				this.Visible = false;
			},
			pTypingGame.MusicTrackTimeSource.GetCurrentTime() + 1000
		);
		this.Note.TypedText = this.Note.Text;
		this.Note.HitResult = HitResult.Poor;
		this.Note.Complete  = true;
	}
}
