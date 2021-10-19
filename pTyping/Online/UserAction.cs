using Furball.Engine.Engine.Helpers;

namespace pTyping.Online {
    public class UserAction {
        public Bindable<UserActionType> Action;
        public Bindable<string>         ActionText;
        public Bindable<PlayMode>       Mode;

        public UserAction(UserActionType action, string actionText, PlayMode mode = PlayMode.pTyping) {
            this.Action     = new(action);
            this.ActionText = new(actionText);
            this.Mode       = new(mode);
        }

        public UserAction() {
            
        }
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
        pTyping  = 4
    }
}
