using System;
using System.Threading.Tasks;
using Furball.Engine;
using pTyping.Shared;
using pTyping.Shared.Beatmaps;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen {
	public bool SaveNeeded;

	private bool _isSaving;
	public void Save() {
		if (this._isSaving) {
			pTypingGame.NotificationManager.CreatePopup("Save already in progress...");
			return;
		}

		Guid       setId  = this.BeatmapSet.Id;
		BeatmapSet toSave = this.BeatmapSet.Clone();
		this._isSaving = true;

		Task.Factory.StartNew(() => {
			//Create a instance of the database for this thread
			BeatmapDatabase database = new BeatmapDatabase(FurballGame.DataFolder);

			BeatmapSet databaseSet = database.Realm.Find<BeatmapSet>(setId);

			database.Realm.Write(() => {
				toSave.CopyInto(databaseSet);
			});

			databaseSet.Realm.Refresh();

			this._isSaving  = false;
			this.SaveNeeded = false;

			FurballGame.GameTimeScheduler.ScheduleMethod(_ => {
				pTypingGame.NotificationManager.CreatePopup("Saved!");
			});
		}, TaskCreationOptions.PreferFairness);
	}
}
