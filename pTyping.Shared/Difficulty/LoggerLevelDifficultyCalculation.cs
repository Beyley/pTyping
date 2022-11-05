using Kettu;

namespace pTyping.Shared.Difficulty;

public class LoggerLevelDifficultyCalculation : LoggerLevel {
	protected LoggerLevelDifficultyCalculation() {}

	public override string Name => "DifficultyCalculation";

	public static readonly LoggerLevelDifficultyCalculation Instance = new LoggerLevelDifficultyCalculation();
}
