using System.IO;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pTyping.Drawables;
using pTyping.Songs;

namespace pTyping.Screens {
	public class MenuScreen : Screen {
		private TextDrawable _musicTitle;
		private AudioStream  _musicTrack;
		private Song         _playingSong;
		
		public override void Initialize() {
			#region Title
			TextDrawable titleText = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.2f), FurballGame.DEFAULT_FONT, "pTyping", 75) {
				OriginType = OriginType.Center
			};
			
			this.Manager.Add(titleText);
			#endregion
			
			#region Main buttons
			Texture2D menuButtonsTexture = ContentManager.LoadMonogameAsset<Texture2D>("menubuttons", ContentSource.User);

			float y = FurballGame.DEFAULT_WINDOW_HEIGHT * 0.35f;

			TexturedDrawable playButton    = new(menuButtonsTexture, new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y), TexturePositions.MENU_PLAY_BUTTON) {
				OriginType = OriginType.Center,
				Scale = new(0.75f)
			};
			TexturedDrawable editButton    = new(menuButtonsTexture, new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += playButton.Size.Y + 10), TexturePositions.MENU_EDIT_BUTTON) {
				OriginType = OriginType.Center,
				Scale      = new(0.75f)
			};
			TexturedDrawable optionsButton = new(menuButtonsTexture, new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += editButton.Size.Y + 10), TexturePositions.MENU_OPTIONS_BUTTON) {
				OriginType = OriginType.Center,
				Scale = new(0.75f)
			};
			TexturedDrawable exitButton    = new(menuButtonsTexture, new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += optionsButton.Size.Y + 10), TexturePositions.MENU_EXIT_BUTTON) {
				OriginType = OriginType.Center,
				Scale      = new(0.75f)
			};

			playButton.OnClick += delegate {
				pTypingGame.MenuClickSound.Play(Config.Volume);
				FurballGame.Instance.ChangeScreen(new SongSelectionScreen(false, this._playingSong));
			};
			
			editButton.OnClick += delegate {
				pTypingGame.MenuClickSound.Play(Config.Volume);
				FurballGame.Instance.ChangeScreen(new SongSelectionScreen(true, this._playingSong));
			};
			
			exitButton.OnClick += delegate {
				pTypingGame.MenuClickSound.Play(Config.Volume);
				FurballGame.Instance.Exit();
			};

			optionsButton.OnClick += delegate {
				pTypingGame.MenuClickSound.Play(Config.Volume);
				FurballGame.Instance.ChangeScreen(new OptionsScreen());
			};
			
			this.Manager.Add(playButton);
			this.Manager.Add(editButton);
			this.Manager.Add(optionsButton);
			this.Manager.Add(exitButton);
			#endregion

			SongManager.UpdateSongs();
			
			#region Menu music
			this._musicTitle = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 5, 5), FurballGame.DEFAULT_FONT, $"None", 35) {
				OriginType = OriginType.TopRight
			};

			Texture2D editorButtonsTexture2D = ContentManager.LoadMonogameAsset<Texture2D>("editorbuttons", ContentSource.User);

			TexturedDrawable musicPlayButton = new(editorButtonsTexture2D, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 105, this._musicTitle.Size.Y + 10), TexturePositions.EDITOR_PLAY) {
				Scale      = new (0.5f, 0.5f),
				OriginType = OriginType.TopRight
			};
			TexturedDrawable musicPauseButton = new(editorButtonsTexture2D, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 55, this._musicTitle.Size.Y + 10), TexturePositions.EDITOR_PAUSE) {
				Scale      = new (0.5f, 0.5f),
				OriginType = OriginType.TopRight
			};
			TexturedDrawable musicNextButton = new (editorButtonsTexture2D, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 5, this._musicTitle.Size.Y + 10), TexturePositions.EDITOR_RIGHT) {
				Scale      = new (0.5f, 0.5f),
				OriginType = OriginType.TopRight
			};

			musicPlayButton.OnClick += delegate {
				this._musicTrack.Play();
			};

			musicPauseButton.OnClick += delegate {
				switch (this._musicTrack.PlaybackState) {
					case PlaybackState.Playing:
						this._musicTrack.Pause();
						break;
					case PlaybackState.Paused:
						this._musicTrack.Resume();
						break;
				}
			};

			musicNextButton.OnClick += delegate {
				this.LoadRandomSong();
			};
			
			this.Manager.Add(this._musicTitle);

			this.Manager.Add(musicPlayButton);
			this.Manager.Add(musicPauseButton);
			this.Manager.Add(musicNextButton);
			#endregion

			this._musicTrack = new();
			
			this.LoadRandomSong();

			base.Initialize();
		}

		public void LoadRandomSong() {
			int songToChoose = FurballGame.Random.Next(SongManager.Songs.Count);

			this._playingSong = SongManager.Songs[songToChoose];
			
			string qualifiedAudioPath = Path.Combine(this._playingSong.FileInfo.DirectoryName ?? string.Empty, this._playingSong.AudioPath);

			if (this._musicTrack.IsValidHandle) {
				if(this._musicTrack.PlaybackState == PlaybackState.Playing)
					this._musicTrack.Stop();
				
				this._musicTrack.Free();
			}
			
			this._musicTrack.Load(ContentManager.LoadRawAsset(qualifiedAudioPath, ContentSource.External));
			this._musicTrack.Volume = Config.Volume;
			this._musicTrack.Play();

			this._musicTitle.Text = $"{this._playingSong.Artist} - {this._playingSong.Name}";
		}

		protected override void Dispose(bool disposing) {
			this._musicTrack.Stop();
			this._musicTrack.Free();
			
			base.Dispose(disposing);
		}
	}
}
