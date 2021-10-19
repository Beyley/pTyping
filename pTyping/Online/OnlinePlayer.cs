using Furball.Engine.Engine.Helpers;

namespace pTyping.Online {
    public class OnlinePlayer {
        public Bindable<UserAction> Action = new(
            new(UserActionType.Idle, "Idling.")
        );
        public Bindable<int>    UserId = new(-1);
        public Bindable<string> Username = new("Unknown user!");

        public Bindable<long>   RankedScore = new(0);
        public Bindable<long>   TotalScore  = new(0);
        public Bindable<double> Accuracy    = new(1d);
        public Bindable<int>    PlayCount   = new(0);
    }
}
