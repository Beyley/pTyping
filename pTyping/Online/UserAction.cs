namespace pTyping.Online {
    public class UserAction {
        public UserActionType Action;
        public string         ActionText;
        public PlayMode       Mode;

        public UserAction(UserActionType action, string actionText, PlayMode mode = PlayMode.pTyping) {
            this.Action     = action;
            this.ActionText = actionText;
            this.Mode       = mode;
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
