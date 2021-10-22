using System.Diagnostics.CodeAnalysis;
using Furball.Engine.Engine.Helpers;

namespace pTyping.Online {
    public class UserAction {
        public Bindable<UserActionType> Action     = new(UserActionType.Idle);
        public Bindable<string>         ActionText = new("");
        public Bindable<PlayMode>       Mode       = new(PlayMode.Standard);

        public UserAction(UserActionType action, string actionText, PlayMode mode = PlayMode.pTyping) {
            this.Action     = new(action);
            this.ActionText = new(actionText);
            this.Mode       = new(mode);
        }

        public UserAction() {}
    }

    public enum UserActionType {
        Unknown = 0,
        Idle,
        Ingame,
        Leaving,
        Editing
    }

    public enum PlayMode : byte {
        Standard = 0,
        Taiko    = 1,
        Catch    = 2,
        Mania    = 3,
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        pTyping = 4
    }
}
