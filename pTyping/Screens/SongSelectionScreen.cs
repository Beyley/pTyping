using System.IO;
using ManagedBass;
using pTyping.Songs;
using SpriteFontPlus;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Audio;
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

		private Song _selectedSong;
		public Song SelectedSong {
			get => this._selectedSong;
			set {
				this._selectedSong = value;
				
				this.UpdateSelectedSong();
			}
		}

		private Texture2D        _defaultBackgroundImage;
		private Texture2D        _backgroundImage;
		private TexturedDrawable _backgroundImageDrawable;

		private AudioStream _musicTrack;
		
		public SongSelectionScreen(bool editor, Song selectedSong) {
			this._editor      = editor;
			this._selectedSong = selectedSong;
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
				FurballGame.Instance.ChangeScreen(new MenuScreen(this._selectedSong));
			};
			
			this.Manager.Add(backButton);
			#endregion

			#region Create new song button
			if(this._editor) {
				UiButtonDrawable createNewSongButton = new(new Vector2(backButton.Size.X + 10f, FurballGame.DEFAULT_WINDOW_HEIGHT), "Create Song", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White, Vector2.Zero) {
					OriginType = OriginType.BottomLeft
				};
				
				createNewSongButton.OnClick += delegate {
					pTypingGame.MenuClickSound.Play();
					FurballGame.Instance.ChangeScreen(new NewSongScreen());
				};

				this.Manager.Add(createNewSongButton);
			}
			#endregion
			
			#region Create new buttons for each song
			float tempY = 50;
			foreach (Song song in SongManager.Songs) {
				UiButtonDrawable drawable = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 50, tempY), $"{song.Artist} - {song.Name} [{song.Difficulty}]", FurballGame.DEFAULT_FONT, 30, Color.Aqua, Color.Black, Color.Black, new Vector2(650, 50), 5f, new []{ CharacterRange.BasicLatin, CharacterRange.CyrillicSupplement, CharacterRange.Latin1Supplement, CharacterRange.LatinExtendedA, CharacterRange.LatinExtendedB, CharacterRange.Cyrillic, CharacterRange.Hiragana, CharacterRange.Katakana, new CharacterRange('★') }) {
					OriginType = OriginType.TopRight,
					TextDrawable = {
						OriginType = OriginType.RightCenter
					}
				};

				drawable.OnClick += delegate {
					if (this._selectedSong == song) {
						this.PlaySelectedMap();
						return;
					}

					this.SelectedSong = song;
				};

				this.Manager.Add(drawable);

				tempY += 60;
			}
			#endregion

			#region Song info
			this._songInfo = new TextDrawable(new Vector2(10, 10), FurballGame.DEFAULT_FONT, "", 35);
			
			this.Manager.Add(this._songInfo);
			#endregion

			this._musicTrack = new();
			
			Config.Volume.OnChange += this.OnVolumeChange;

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
			
			if (this.SelectedSong == null && SongManager.Songs.Count > 0) {
				this.SelectedSong = SongManager.Songs[0];
			} else if (this.SelectedSong != null) {
				this.UpdateSelectedSong();
			}

			base.Initialize();
		}
		
		private void OnVolumeChange(object _, float f) {
			this._musicTrack.Volume = f;
		}

		public void PlaySelectedMap() {
			pTypingGame.MenuClickSound.Play();
			FurballGame.Instance.ChangeScreen(this._editor ? new EditorScreen(this.SelectedSong) : new PlayerScreen(this.SelectedSong));
		}

		public void UpdateSelectedSong() {
			this._songInfo.Text = $"{this.SelectedSong.Artist} - {this.SelectedSong.Name} [{this.SelectedSong.Difficulty}]\nCreated by {this.SelectedSong.Creator}";

			if (this._musicTrack.IsValidHandle && this._musicTrack.PlaybackState == PlaybackState.Playing) {
				this._musicTrack.Stop();
				this._musicTrack.Free();
			}
			
			string qualifiedAudioPath = Path.Combine(this.SelectedSong.FileInfo.DirectoryName ?? string.Empty, this.SelectedSong.AudioPath);
			
			this._musicTrack.Load(ContentManager.LoadRawAsset(qualifiedAudioPath, ContentSource.External));
			this._musicTrack.Volume = Config.Volume;
			this._musicTrack.Play();
			
			pTypingGame.LoadBackgroundFromSong(this.SelectedSong);
		}

		protected override void Dispose(bool disposing) {
			Config.Volume.OnChange -= this.OnVolumeChange;

			if (this._musicTrack.IsValidHandle) {
				this._musicTrack.Free();
			}
			
			base.Dispose(disposing);
		}
	}
}
