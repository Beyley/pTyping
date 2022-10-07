#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.Filters;
using pTyping.Shared.Beatmaps.Sorting;

namespace pTyping.Graphics.Menus.SongSelect;

public class SongSelectDrawable : CompositeDrawable {
	private readonly List<BeatmapSetDrawable> _registeredSetButtons = new List<BeatmapSetDrawable>();

	public readonly ObservableCollection<IBeatmapSetFilter> FilterOperations = new ObservableCollection<IBeatmapSetFilter>();

	public Action<float> ChangeScroll;

	public SongSelectDrawable(Vector2 pos, Action<float> changeScroll) {
		this.ChangeScroll = changeScroll;
		this.Position     = pos;

		this.FilterOperations.CollectionChanged        += this.OnFilterOperationChange;
		pTypingGame.BeatmapDatabase.Realm.RealmChanged += this.OnRealmUpdate;

		this.UpdateDrawables();

		this.InvisibleToInput = true;
	}

	private void OnRealmUpdate(object sender, EventArgs e) {
		this.UpdateDrawables();
	}

	private void OnFilterOperationChange(object? sender, NotifyCollectionChangedEventArgs e) {
		this.UpdateDrawables();
	}

	public void UpdateDrawables() {
		IQueryable<BeatmapSet> sets = pTypingGame.BeatmapDatabase.Realm.All<BeatmapSet>();

		foreach (IBeatmapSetFilter filter in this.FilterOperations)
			sets = filter.Filter(sets);

		ImmutableSortedSet<BeatmapSet> sortedSets = sets.ToList().ToImmutableSortedSet(new BeatmapSetArtistComparer());

		this.Children.Clear();
		this._registeredSetButtons.Clear();

		foreach (BeatmapSet set in sortedSets) {
			BeatmapSetDrawable drawable = new BeatmapSetDrawable(set);

			this._registeredSetButtons.Add(drawable);
		}

		float y = 0;
		foreach (BeatmapSetDrawable button in this._registeredSetButtons) {
			button.Position = new Vector2(0, y);
			// button.OriginType = OriginType.TopRight;

			button.YPositionInDrawable = y;

			this.Children.Add(button);

			y += button.Size.Y + 5;
		}

		if (pTypingGame.CurrentSong.Value != null) {
			BeatmapSetDrawable? setSelected = this._registeredSetButtons.FirstOrDefault(x => x.BeatmapSet.Beatmaps.Any(y => y.Id == pTypingGame.CurrentSong.Value.Id));

			if (setSelected != null)
				this.ChangeScroll?.Invoke(setSelected.YPositionInDrawable - FurballGame.WindowHeight / 2f + setSelected.Size.Y / 2f);
		}
	}
}
