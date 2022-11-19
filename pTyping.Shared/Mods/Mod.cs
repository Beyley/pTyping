using System.Reflection;
using System.Text;
using Furball.Engine.Engine.Graphics.Drawables;
using Newtonsoft.Json;
using pTyping.Shared.Beatmaps.HitObjects;

namespace pTyping.Shared.Mods;

[JsonObject(MemberSerialization.OptIn)]
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
		StringBuilder builder = new StringBuilder();

		foreach (Mod mod in mods)
			builder.Append(mod.ShorthandName);

		return builder.ToString();
	}

	public static double AggregateScoreMultiplier(IEnumerable<Mod> mods) {
		return mods.Aggregate(1d, (d, mod) => mod.ScoreMultiplier * d);
	}

	private static Type[] GetAllMods() {
		List<Type> list       = new List<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

		for (int i = 0; i < assemblies.Length; i++) {
			try {
				Assembly assembly = assemblies[i];
				Type[]   types    = assembly.GetTypes();
				for (int j = 0; j < types.Length; j++) {
					Type type = types[j];
					if (type.IsSubclassOf(typeof(Mod)))
						list.Add(type);
				}
			}
			catch {}
		}
		return list.ToArray();
	}

	public static readonly Type[] RegisteredMods = GetAllMods();
}
