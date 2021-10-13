namespace pTyping.Online {
    public class UserAction {
        public UserActionType Action;
        public string         ActionText;

        public UserAction(UserActionType action, string actionText) {
            this.Action     = action;
            this.ActionText = actionText;
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
}
