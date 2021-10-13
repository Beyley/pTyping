using Furball.Engine.Engine.Helpers;

namespace pTyping {
    public static class Config {
        /// <summary>
        /// The master volume of the audio
        /// </summary>
        public static Bindable<float> Volume = new(0.05f);
        /// <summary>
        /// How much to dim the background during gameplay
        /// </summary>
        public static float BackgroundDim = 0.5f;
        /// <summary>
        /// The target FPS to reach
        /// </summary>
        public static Bindable<int> TargetFPS = new(1000);

        public static Bindable<string> Username = new("Guest");
        public static Bindable<string> Password = new("password");
        
        /// <summary>
        /// The time it takes the notes to go from the right side of the screen to the left 
        /// </summary>
        public static readonly int BaseApproachTime = 2000;
    }
}
