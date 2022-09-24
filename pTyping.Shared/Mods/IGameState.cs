namespace pTyping.Shared.Mods;

public interface IGameState {
	public void EffectSpeed(double        effect);
	public void EffectApproachTime(double effect);

	double ApproachTimeAt(double time);
}
