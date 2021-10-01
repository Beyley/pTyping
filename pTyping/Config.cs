using Furball.Engine.Engine.Helpers;

namespace pTyping {
	public static class Config {
		/// <summary>
		/// The master volume of the audio
		/// </summary>
		public static Bindable<float> Volume = new(0.05f);
		public static float BackgroundDim = 0.5f;
		/// <summary>
		/// The time it takes the notes to go from the right side of the screen to the left 
		/// </summary>
		public static readonly int BaseApproachTime = 2000;

		public static int HitWindow = 200;
	}
}
