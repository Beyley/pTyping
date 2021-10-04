using System;
using System.IO;
using ManagedBass;
using pTyping.Songs;
using SpriteFontPlus;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace pTyping.Screens {
	public class SongSelectionScreen : Screen {
		private bool _editor;

		private TextDrawable _songInfo;

		private Texture2D        _defaultBackgroundImage;
		private Texture2D        _backgroundImage;
		private TexturedDrawable _backgroundImageDrawable;

		public SongSelectionScreen(bool editor) {
			this._editor      = editor;
		}
		
		public override void Initialize() {
			SongManager.UpdateSongs();
			
			#region Back button
			pTypingGame.LoadBackButtonTexture();
			
			TexturedDrawable backButton = new(pTypingGame.BackButtonTexture, new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT)) {
				OriginType = OriginType.BottomLeft,
				Scale      = new (0.4f, 0.4f)
			};
			
			backButton.OnClick += delegate {
				pTypingGame.MenuClickSound.Play();
				ScreenManager.ChangeScreen(new MenuScreen());
			};
			
			this.Manager.Add(backButton);

			EventHandler<Point> audioSpeed1onClick = delegate {
				pTypingGame.MusicTrack.Frequency *= 0.75f;
			};

			UiButtonDrawable audioSpeed1 = new(new(backButton.Size.X + 10, FurballGame.DEFAULT_WINDOW_HEIGHT), "0.75x speed", pTypingGame.DEFAULT_FONT, 30, Color.Red, Color.White, Color.White, new(0), audioSpeed1onClick);
			audioSpeed1.OriginType = OriginType.BottomLeft;

			this.Manager.Add(audioSpeed1);

			EventHandler<Point> audioSpeed2onClick = delegate {
				pTypingGame.MusicTrack.Frequency *= 1.5f;
			};
			
			UiButtonDrawable audioSpeed2 = new(new(backButton.Size.X + audioSpeed1.Size.X + 20, FurballGame.DEFAULT_WINDOW_HEIGHT), "1.5x speed", pTypingGame.DEFAULT_FONT, 30, Color.Red, Color.White, Color.White, new(0), audioSpeed2onClick);
			audioSpeed2.OriginType = OriginType.BottomLeft;

			this.Manager.Add(audioSpeed2);
			#endregion

			#region Create new song button
			if(this._editor) {
				EventHandler<Point> newSongOnClick = delegate {
					pTypingGame.MenuClickSound.Play();
					ScreenManager.ChangeScreen(new NewSongScreen());
				};

				UiButtonDrawable createNewSongButton = new(new Vector2(backButton.Size.X + 10f, FurballGame.DEFAULT_WINDOW_HEIGHT), "Create Song", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White, Vector2.Zero, newSongOnClick) {
					OriginType = OriginType.BottomLeft
				};

				this.Manager.Add(createNewSongButton);
			}
			#endregion
			
			#region Create new buttons for each song
			float tempY = 50;
			foreach (Song song in SongManager.Songs) {
				EventHandler<Point> songButtonOnClick = delegate {
					if (pTypingGame.CurrentSong.Value == song) {
						this.PlaySelectedMap();
						return;
					}

					pTypingGame.CurrentSong.Value = song;
				};

				UiButtonDrawable songButton = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 50, tempY), $"{song.Artist} - {song.Name} [{song.Difficulty}]", pTypingGame.UniFont, 30, Color.Aqua, Color.Black, Color.Black, new Vector2(650, 50), songButtonOnClick, 5f, new []{ CharacterRange.BasicLatin, CharacterRange.Hiragana, CharacterRange.Katakana, new CharacterRange('★') }) {
					OriginType = OriginType.TopRight,
					TextDrawable = {
						OriginType = OriginType.RightCenter
					}
				};

				this.Manager.Add(songButton);

				tempY += 60;
			}
			#endregion

			#region Song info
			this._songInfo = new TextDrawable(new Vector2(10, 10), pTypingGame.UniFont, "", 30);
			
			this.Manager.Add(this._songInfo);
			#endregion

			#region background image
			this.Manager.Add(pTypingGame.CurrentSongBackground);
			pTypingGame.CurrentSongBackground.Tweens.Add(
				new ColorTween(
						TweenType.Color, 
						pTypingGame.CurrentSongBackground.ColorOverride, 
						new Color(175, 175, 175), 
						pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(), 
						pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 100
					)
			);
			#endregion
			
			if (pTypingGame.CurrentSong.Value == null && SongManager.Songs.Count > 0) {
				pTypingGame.CurrentSong.Value = SongManager.Songs[0];
			} else if (pTypingGame.CurrentSong.Value != null) {
				this.UpdateSelectedSong(true);
			}
			
			pTypingGame.CurrentSong.OnChange += this.OnSongChange;

			base.Initialize();
		}
		
		private void OnSongChange(object sender, Song e) {
			this.UpdateSelectedSong();
		}

		public void PlaySelectedMap() {
			pTypingGame.MenuClickSound.Play();
			ScreenManager.ChangeScreen(this._editor ? new EditorScreen() : new PlayerScreen());
		}

		public void UpdateSelectedSong(bool fromPrevScreen = false) {
			this._songInfo.Text = $"{pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}]\nCreated by {pTypingGame.CurrentSong.Value.Creator}";

			string qualifiedAudioPath = Path.Combine(pTypingGame.CurrentSong.Value.FileInfo.DirectoryName ?? string.Empty, pTypingGame.CurrentSong.Value.AudioPath);
			
			if(!fromPrevScreen) {
				pTypingGame.LoadMusic(ContentManager.LoadRawAsset(qualifiedAudioPath, ContentSource.External));
				pTypingGame.PlayMusic();
			} else if (pTypingGame.MusicTrack.PlaybackState is PlaybackState.Paused or PlaybackState.Stopped) {
				pTypingGame.PlayMusic();
			}
			
			pTypingGame.LoadBackgroundFromSong(pTypingGame.CurrentSong.Value);
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
		}
	}
}
