using System;
using System.Linq;
using pTyping.Shared.Beatmaps;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen {
	public TimingPoint GetTimingPointAt(double time) {
		TimingPoint tp = this.Beatmap.TimingPoints.FirstOrDefault();
		
		foreach (TimingPoint t in this.Beatmap.TimingPoints) {
			if (t.Time > time) break;
			tp = t;
		}

		return tp;
	}

	/// <summary>
	///     Snaps the time to the tempo of the current timing point.
	/// </summary>
	/// <param name="time"></param>
	public double SnapTime(double time) {
		TimingPoint tp = this.GetTimingPointAt(time);

		double dividedBeatLength = tp.Tempo / tp.TimeSignature;

		return Math.Round((time - tp.Time) / dividedBeatLength) * dividedBeatLength + tp.Time;
	}

	public void MoveTimeByNBeats(int beatAmount) {
		if (beatAmount == 0) return;

		double pos = this.SnapTime(pTypingGame.MusicTrack.CurrentPosition);

		//get whether we are moving left (negative) or right (positive)
		int direction = Math.Sign(beatAmount);

		int absAmount = Math.Abs(beatAmount);
		for (int i = 0; i < absAmount; i++) {
			TimingPoint tp = this.GetTimingPointAt(pos);

			pos += direction * tp.Tempo / tp.TimeSignature;

			pos = this.SnapTime(pos);
		}

		pTypingGame.MusicTrack.CurrentPosition = pos;
	}
}
