using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using Eto.Forms;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using pTyping.Online;
using pTyping.Shared;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.ObjectModel;
using pTyping.UiElements;
using TextDrawable = Furball.Engine.Engine.Graphics.Drawables.TextDrawable;

namespace pTyping.Graphics.Editor;

public class EditorSongFormContents : CompositeDrawable {
	private readonly Beatmap               _map;
	private readonly SliderDrawable<float> _strictnessSlider;
	private readonly DrawableTextBox       _difficultyInputAscii;
	private readonly DrawableTextBox       _difficultyInputUnicode;
	private readonly DrawableTextBox       _titleInputAscii;
	private readonly DrawableTextBox       _titleInputUnicode;
	private readonly DrawableTextBox       _artistInputAscii;
	private readonly DrawableTextBox       _artistInputUnicode;
	private readonly DrawableTextBox       _timingInputTime;
	private readonly DrawableTextBox       _timingInputTempo;
	private readonly DrawableDropdown      _typingConversionDropdown;
	private          DrawableTextBox       _tagsInput;
	private          DrawableTextBox       _descriptionInput;
	private readonly DrawableTextBox       _mapperInput;

	public EditorSongFormContents(Beatmap map, EditorScreen editor) {
		this._map = map;

		const float x = 5;
		float       y = 0;

		TextDrawable strictnessLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Strictness", 24);
		y += strictnessLabel.Size.Y;

		this._strictnessSlider = new SliderDrawable<float>(new BoundNumber<float> {
			Value     = map.Difficulty.Strictness,
			MaxValue  = 10,
			MinValue  = 1,
			Precision = 0.1f
		}) {
			Position = new Vector2(x, y)
		};
		y += this._strictnessSlider.Size.Y;

		this._strictnessSlider.Value.Changed += (_, f) => {
			map.Difficulty.Strictness = f;

			editor.SaveNeeded = true;
		};

		TextDrawable difficultyNameLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Difficulty Name", 24);
		y += difficultyNameLabel.Size.Y;

		this._difficultyInputAscii   =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, this._map.Info.DifficultyName.Ascii ?? "");
		y                            += this._difficultyInputAscii.Size.Y;
		this._difficultyInputUnicode =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, this._map.Info.DifficultyName.Unicode);
		y                            += this._difficultyInputUnicode.Size.Y;

		this._difficultyInputAscii.OnCommit += (_, s) => {
			this._map.Info.DifficultyName.Ascii = s;
			editor.SaveNeeded                   = true;
		};

		this._difficultyInputUnicode.OnCommit += (_, s) => {
			this._map.Info.DifficultyName.Unicode = s;
			editor.SaveNeeded                     = true;
		};

		TextDrawable artistLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Artist", 24);
		y += artistLabel.Size.Y;

		this._artistInputAscii   =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, editor.EditorState.Set.Artist.Ascii ?? "");
		y                        += this._artistInputAscii.Size.Y;
		this._artistInputUnicode =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, editor.EditorState.Set.Artist.Unicode);
		y                        += this._artistInputUnicode.Size.Y;

		this._artistInputAscii.OnCommit += delegate(object _, string s) {
			editor.EditorState.Set.Artist.Ascii = s;
			editor.SaveNeeded                   = true;
		};

		this._artistInputUnicode.OnCommit += delegate(object _, string s) {
			editor.EditorState.Set.Artist.Unicode = s;
			editor.SaveNeeded                     = true;
		};

		TextDrawable titleLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Title (Name of song)", 24);
		y += titleLabel.Size.Y;

		this._titleInputAscii   =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, editor.EditorState.Set.Title.Ascii ?? "");
		y                       += this._titleInputAscii.Size.Y;
		this._titleInputUnicode =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, editor.EditorState.Set.Title.Unicode);
		y                       += this._titleInputUnicode.Size.Y;

		this._titleInputAscii.OnCommit += delegate(object _, string s) {
			editor.EditorState.Set.Title.Ascii = s;
			editor.SaveNeeded                  = true;
		};

		this._titleInputUnicode.OnCommit += delegate(object _, string s) {
			editor.EditorState.Set.Title.Unicode = s;
			editor.SaveNeeded                    = true;
		};

		TextDrawable timingLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Timing", 24);
		y += timingLabel.Size.Y;

		this._timingInputTime =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, $"{this._map.TimingPoints[0].Time}");
		y                     += this._timingInputTime.Size.Y;

		this._timingInputTime.OnCommit += delegate(object _, string s) {
			if (float.TryParse(s, out float time)) {
				this._map.TimingPoints[0].Time = time;
				editor.SaveNeeded              = true;
			}
		};

		this._timingInputTempo =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, $"{this._map.TimingPoints[0].Tempo}");
		y                      += this._timingInputTempo.Size.Y;

		this._timingInputTempo.OnCommit += delegate(object _, string s) {
			if (float.TryParse(s, out float tempo)) {
				this._map.TimingPoints[0].Tempo = tempo;
				editor.SaveNeeded               = true;
			}
		};

		TextDrawable songLanguageLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Song Language", 24);
		y += songLanguageLabel.Size.Y;

		SongLanguage[] languages = (SongLanguage[])Enum.GetValues(typeof(SongLanguage));

		foreach (SongLanguage language in languages) {
			DrawableTickbox tickbox = new DrawableTickbox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, language.ToString(), map.Metadata.Languages.Contains(language));
			y += tickbox.Size.Y;

			tickbox.Selected.OnChange += delegate(object _, bool b) {
				if (b) {
					//Dont add the language if the map already has it
					if (map.Metadata.Languages.Contains(language))
						return;

					map.Metadata.BackingLanguages.Add((int)language);
					editor.SaveNeeded = true;
				}
				else {
					//If we were successful in removing the language, mark we need to save
					if (map.Metadata.BackingLanguages.Remove((int)language))
						editor.SaveNeeded = true;
				}
			};

			this.Drawables.Add(tickbox);
		}
		
		this._typingConversionDropdown = new DrawableDropdown(new Vector2(x, y), pTypingGame.JapaneseFont, 20, new Vector2(250, 35), new Dictionary<object, string> {
			{ TypingConversions.ConversionType.StandardLatin, "Standard Latin Only" },
			{ TypingConversions.ConversionType.StandardHiragana, "Japanese/Hiragana" },
			{ TypingConversions.ConversionType.StandardRussian, "Russian" },
			{ TypingConversions.ConversionType.StandardEsperanto, "Esperanto" }
		});
		y += this._typingConversionDropdown.Size.Y;

		this._typingConversionDropdown.SelectedItem.OnChange += delegate(object _, KeyValuePair<object, string> selection) {
			EtoHelper.MessageDialog((sender, result) => {
				if (result != DialogResult.Yes)
					return;

				//Schedule a new method to run ASAP on the main thread
				FurballGame.GameTimeScheduler.ScheduleMethod(_ => {
					//Set the conversion type of all hit objects to the new type
					foreach (HitObject hitObject in this._map.HitObjects)
						hitObject.TypingConversion = (TypingConversions.ConversionType)selection.Key;
				});
			}, $"Are you sure you want to do this? This will set *all* notes to {selection.Value}!", MessageBoxButtons.YesNo);
		};
		
		TextDrawable tagsLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Tags (comma separated)", 24);
		y += songLanguageLabel.Size.Y;

		this._tagsInput = new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, string.Join(", ", map.Metadata.Tags));
		y += this._tagsInput.Size.Y;
		
		this._tagsInput.OnCommit += delegate(object _, string s) {
			//Split the string by commas, and trim each tag
			string[] tags = s.Split(',').Select(tag => tag.Trim()).ToArray();

			map.Metadata.Tags.Clear();
			
			//Set the tags to the new tags
			foreach (string tag in tags) {
				map.Metadata.Tags.Add(tag);
			}
			editor.SaveNeeded = true;
		};
		
		TextDrawable descriptionLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Description", 24);
		y += descriptionLabel.Size.Y;

		//Multiline text box with 5 lines
		this._descriptionInput =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, map.Info.Description, 5);
		y                      += this._descriptionInput.Size.Y;
		
		this._descriptionInput.OnCommit += delegate(object _, string s) {
			map.Info.Description = s;
			editor.SaveNeeded    = true;
		};
		
		TextDrawable mapperLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Mapper", 24);
		y += mapperLabel.Size.Y;

		this._mapperInput =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, map.Info.Mapper);
		y                += this._mapperInput.Size.Y;
		
		this._mapperInput.OnCommit += delegate(object _, string s) {
			map.Info.Mapper = s;
			editor.SaveNeeded = true;
		};
		
		this.Drawables.Add(strictnessLabel);
		this.Drawables.Add(this._strictnessSlider);

		this.Drawables.Add(difficultyNameLabel);
		this.Drawables.Add(this._difficultyInputAscii);
		this.Drawables.Add(this._difficultyInputUnicode);

		this.Drawables.Add(artistLabel);
		this.Drawables.Add(this._artistInputAscii);
		this.Drawables.Add(this._artistInputUnicode);

		this.Drawables.Add(titleLabel);
		this.Drawables.Add(this._titleInputAscii);
		this.Drawables.Add(this._titleInputUnicode);

		this.Drawables.Add(timingLabel);
		this.Drawables.Add(this._timingInputTime);
		this.Drawables.Add(this._timingInputTempo);

		this.Drawables.Add(songLanguageLabel);
		this.Drawables.Add(this._typingConversionDropdown);
		
		this.Drawables.Add(tagsLabel);
		this.Drawables.Add(this._tagsInput);
		
		this.Drawables.Add(descriptionLabel);
		this.Drawables.Add(this._descriptionInput);
		
		this.Drawables.Add(mapperLabel);
		this.Drawables.Add(this._mapperInput);
	}
}
