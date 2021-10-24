using System;
using System.Collections.Specialized;
using System.Linq;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input;
using Microsoft.Xna.Framework;

namespace pTyping.Drawables {
    public class ChatDrawable : CompositeDrawable {
        private readonly TextDrawable      _channelContents;
        public           UiTextBoxDrawable MessageInputDrawable;

        public Bindable<string> SelectedChannel = new("#general");

        public ChatDrawable(Vector2 pos) {
            this.Position = pos;

            this.Drawables.Add(this._channelContents     = new(new(0), pTypingGame.JapaneseFontStroked, "", 35));
            this.Drawables.Add(this.MessageInputDrawable = new(new(0), pTypingGame.JapaneseFontStroked, "", 35, FurballGame.DEFAULT_WINDOW_WIDTH - 20, true));

            this.MessageInputDrawable.OnCommit += delegate {
                pTypingGame.OnlineManager.SendMessage(this.SelectedChannel, this.MessageInputDrawable.Text);

                this.MessageInputDrawable.Text = string.Empty;
            };

            pTypingGame.OnlineManager.ChatLog.CollectionChanged += this.ChatLogOnCollectionChanged;
            this.SelectedChannel.OnChange                       += this.SelectedChannelOnChange;

            this.OnClick += this.OnThisClick;

            this.RecalculateAndUpdate_wait_thats_bars();
        }

        private void OnThisClick(object sender, Point e) {
            if (this.Visible)
                this.MessageInputDrawable.OnMouseDown(this, ((MouseButton.LeftButton, e - this.Position.ToPoint() + this.LastCalculatedOrigin.ToPoint()), ""));
        }

        private void SelectedChannelOnChange(object sender, string e) => this.RecalculateAndUpdate_wait_thats_bars();

        private void ChatLogOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => this.RecalculateAndUpdate_wait_thats_bars();

        private void RecalculateAndUpdate_wait_thats_bars() {
            string final = "";

            pTypingGame.OnlineManager.ChatLog.Skip(pTypingGame.OnlineManager.ChatLog.Count - Math.Min(8, pTypingGame.OnlineManager.ChatLog.Count))
                       .Where(message => message.Channel == this.SelectedChannel).ToList().ForEach(message => final += message + "\n");

            this._channelContents.Text         = final.Trim();
            this.MessageInputDrawable.Position = new(0, this._channelContents.Size.Y + 15);
        }

        public override void Dispose(bool disposing) {
            pTypingGame.OnlineManager.ChatLog.CollectionChanged -= this.ChatLogOnCollectionChanged;
            this.SelectedChannel.OnChange                       -= this.SelectedChannelOnChange;

            base.Dispose(disposing);
        }
    }
}