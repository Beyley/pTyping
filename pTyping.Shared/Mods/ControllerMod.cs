namespace pTyping.Shared.Mods;

public class ControllerMod : Mod {
	public override double ScoreMultiplier => 1.0;
	public override string Name            => "Controller";
	public override string ToolTip         => "Use a controller to play the game.";
	public override string ShorthandName   => "CT";

	public override bool IsIncompatible(Mod mod) {
		return false;
	}
}
