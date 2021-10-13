namespace pTyping.Online {
    public class OnlinePlayer {
        public UserAction Action = new() {
            Action     = UserActionType.Idle,
            ActionText = "Idling."
        };
        public string     Username;
        public int        UserId;
    }
}
