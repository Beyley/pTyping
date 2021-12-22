using System;
using System.IO;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;
using pTyping.Songs;

namespace pTyping.Graphics.Menus.SongSelect {
    public class NewSongScreen : pScreen {
        private UiTextBoxDrawable _songArtistTextBox;
        private UiTextBoxDrawable _songCreatorTextBox;
        private UiTextBoxDrawable _songDifficultyTextBox;
        private UiTextBoxDrawable _songNameTextBox;

        public override void Initialize() {
            base.Initialize();

            #region Back button to song select screen

            pTypingGame.LoadBackButtonTexture();

            TexturedDrawable backButton = new(pTypingGame.BackButtonTexture, new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT)) {
                OriginType = OriginType.BottomLeft,
                Scale      = pTypingGame.BackButtonScale
            };

            backButton.OnClick += delegate {
                pTypingGame.MenuClickSound.PlayNew();
                ScreenManager.ChangeScreen(new SongSelectionScreen(true));
            };

            this.Manager.Add(backButton);

            #endregion

            #region Create buttons for all mp3 files in songs folder

            string[] songs = Directory.GetFiles("songs", "*.mp3");

            float y = 20f;
            foreach (string songPath in songs) {
                FileInfo songInfo = new(songPath);

                EventHandler<Point> songOnClick = delegate {
                    this.CreateSong(songInfo);
                };

                UiButtonDrawable buttonForSong = new(
                new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 20, y),
                songInfo.Name,
                FurballGame.DEFAULT_FONT,
                25,
                Color.Blue,
                Color.White,
                Color.White,
                Vector2.Zero,
                songOnClick
                ) {
                    OriginType = OriginType.TopRight
                };

                this.Manager.Add(buttonForSong);

                y += buttonForSong.Size.Y + 20f;
            }

            #endregion

            #region Create text boxes for info about the song

            y = 20f;

            this._songNameTextBox       =  new(new Vector2(20f, y), FurballGame.DEFAULT_FONT, "Name of Song", 50, 400f);
            y                           += 20 + this._songNameTextBox.Size.Y;
            this._songArtistTextBox     =  new(new Vector2(20f, y), FurballGame.DEFAULT_FONT, "Artist", 50, 400f);
            y                           += 20 + this._songArtistTextBox.Size.Y;
            this._songCreatorTextBox    =  new(new Vector2(20f, y), FurballGame.DEFAULT_FONT, "Map Creator Name", 50, 400f);
            y                           += 20 + this._songCreatorTextBox.Size.Y;
            this._songDifficultyTextBox =  new(new Vector2(20f, y), FurballGame.DEFAULT_FONT, "Difficulty Name", 50, 400f);

            this.Manager.Add(this._songNameTextBox);
            this.Manager.Add(this._songArtistTextBox);
            this.Manager.Add(this._songCreatorTextBox);
            this.Manager.Add(this._songDifficultyTextBox);

            #endregion

            pTypingGame.UserStatusListening();
        }

        private void CreateSong(FileInfo musicFileInfo) {
            if (musicFileInfo.DirectoryName is null) throw new Exception("Song directory is null? how???? wtf are you doing??????");

            Song song = new() {
                Name       = this._songNameTextBox.Text,
                Artist     = this._songArtistTextBox.Text,
                Creator    = this._songCreatorTextBox.Text,
                Difficulty = this._songDifficultyTextBox.Text,
                AudioPath  = musicFileInfo.Name
            };

            //Add a default timing point so we dont crash
            song.TimingPoints.Add(
            new() {
                Tempo = 100,
                Time  = 0
            }
            );

            // Get the new directory for the song
            string newSongFolder = Path.Combine(
            musicFileInfo.DirectoryName,
            $"{song.Artist.Replace(" ", "")}-{song.Name.Replace(" ", "")}-by-{song.Creator.Replace(" ", "")}/"
            );

            if (Directory.Exists(newSongFolder))//TODO: Add error message here to tell them that the metadata is too close to an existing song
                return;

            // Create the directory
            Directory.CreateDirectory(newSongFolder);

            musicFileInfo.MoveTo(Path.Combine(newSongFolder, Path.GetFileName(musicFileInfo.FullName)));

            // Open the filestream to the file
            FileStream stream = File.Create(Path.Combine(newSongFolder, $"{song.Difficulty} - {song.Creator}.pts"));
            // Set the fileinfo for the song
            song.FileInfo = new FileInfo(stream.Name);
            // Close the filestream
            stream.Close();
            // Save the song to the file
            SongManager.PTYPING_SONG_HANDLER.SaveSong(song);

            //Play the menu click sound (since we clicked a button)
            pTypingGame.MenuClickSound.PlayNew();
            SongManager.UpdateSongs();
            //Change the screen to the song select screen
            ScreenManager.ChangeScreen(new SongSelectionScreen(true));
        }
        public override string Name    => "New Song";
        public override string State   => "Getting ready to map!";
        public override string Details => "";
    }
}
