using System.Text;
using Furball.Engine.Engine.Graphics.Drawables;
using pTyping.Shared.Beatmaps.HitObjects;

namespace pTyping.Shared.Mods;

public abstract class Mod {
	public abstract double ScoreMultiplier { get; }

	public abstract string Name          { get; }
	public abstract string ToolTip       { get; }
	public abstract string ShorthandName { get; }

	public virtual void PreStart(IGameState state) {}

	public virtual void PreEnd(IGameState state) {}

	public virtual void NoteCreate(IGameState state, Drawable drawable, HitObject hitObject) {}

	public virtual void CharacterTyped(IGameState state, char c, bool success) {}

	public virtual void OnNoteHit(HitObject note) {}

	public abstract bool IsIncompatible(Mod mod);

	public static string ModsShorthandString(Mod[] mods) {
		StringBuilder builder = new();

		foreach (Mod mod in mods)
			builder.Append(mod.ShorthandName);

		return builder.ToString();
	}

	public static double AggregateScoreMultiplier(IEnumerable<Mod> mods) {
		return mods.Aggregate(1d, (d, mod) => mod.ScoreMultiplier * d);
	}

	public static Mod[] RegisteredMods = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.IsSubclassOf(typeof(Mod)) select (Mod)Activator.CreateInstance(type)).ToArray();
}
