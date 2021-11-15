using Furball.Engine.Engine.Helpers;
using JetBrains.Annotations;
using pTyping.Graphics.Online;

namespace pTyping.Online {
    public class OnlinePlayer {
        private UserCardDrawable     _userCard;
        public  Bindable<double>     Accuracy  = new(1d);
        public  Bindable<UserAction> Action    = new(new(UserActionType.Idle, "Idling."));
        public  Bindable<int>        PlayCount = new(0);
        public  Bindable<int>        Rank      = new(0);

        public Bindable<long>   RankedScore = new(0);
        public Bindable<long>   TotalScore  = new(0);
        public Bindable<int>    UserId      = new(-1);
        public Bindable<string> Username    = new("Unknown user!");

        [Pure]
        public UserCardDrawable GetUserCard() {
            if (this._userCard is null) {
                this._userCard = new(new(0), this) {
                    Scale = new(0.3f)
                };

                this._userCard.Player.OnChange                               += (_, _) => this._userCard.UpdateDrawable();
                this._userCard.Player.Value.TotalScore.OnChange              += (_, _) => this._userCard.UpdateDrawable();
                this._userCard.Player.Value.RankedScore.OnChange             += (_, _) => this._userCard.UpdateDrawable();
                this._userCard.Player.Value.Accuracy.OnChange                += (_, _) => this._userCard.UpdateDrawable();
                this._userCard.Player.Value.PlayCount.OnChange               += (_, _) => this._userCard.UpdateDrawable();
                this._userCard.Player.Value.Action.OnChange                  += (_, _) => this._userCard.UpdateDrawable();
                this._userCard.Player.Value.Action.Value.Action.OnChange     += (_, _) => this._userCard.UpdateDrawable();
                this._userCard.Player.Value.Action.Value.Mode.OnChange       += (_, _) => this._userCard.UpdateDrawable();
                this._userCard.Player.Value.Action.Value.ActionText.OnChange += (_, _) => this._userCard.UpdateDrawable();
            }

            return this._userCard;
        }

        [Pure]
        public override string ToString() => this.Username;
    }
}
