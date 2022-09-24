using pTyping.Shared.Mods;
using sowelipisona;

namespace pTyping.Graphics.Player;

public class PlayerGameState : IGameState {
	private readonly AudioStream _musicTrack;
	private readonly Player      _player;

	public PlayerGameState(AudioStream musicTrack, Player player) {
		this._musicTrack = musicTrack;
		this._player     = player;

		//When starting the song, lets ensure we start out at 1x speed
		this._musicTrack.SetSpeed(1);
	}

	public void EffectSpeed(double effect) {
		double speed = this._musicTrack.GetSpeed();

		this._musicTrack.SetSpeed(speed * effect);
	}

	public void EffectApproachTime(double effect) {
		this._player.BaseApproachTime *= effect;
	}

	public double ApproachTimeAt(double time) {
		return this._player.CurrentApproachTime(time);
	}
}
