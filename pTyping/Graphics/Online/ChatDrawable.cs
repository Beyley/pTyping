using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using pTyping.Online;
using Silk.NET.Input;

namespace pTyping.Graphics.Online {
    public class ChatContentsDrawable : CompositeDrawable {
        public List<ManagedDrawable> PublicDrawables => this._drawables;

        public float TargetScroll = 0;

        public override Vector2 Size {
            get;
        }

        public ChatContentsDrawable(Vector2 size) => this.Size = size;

        public void Clear() {
            this.Drawables.ToList().ForEach(x => this._drawables.Remove(x));
        }

        public override void Update(double time) {
            if (this._drawables.Count != 0) {
                this.TargetScroll = Math.Max(this.TargetScroll, 0);

                foreach (ManagedDrawable drawable in this.Drawables) {
                    float startY = float.Parse(drawable.Tags.First());

                    float targetY = startY + this.TargetScroll;

                    float difference = targetY - drawable.Position.Y;

                    drawable.Position.Y += (float)(difference * time * 1000 * 0.01);
                }
            }

            base.Update(time);
        }

        public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
            // batch.End();
            //
            // Rectangle originalRect = FurballGame.Instance.GraphicsDevice.ScissorRectangle;
            //
            // FurballGame.Instance.GraphicsDevice.ScissorRectangle = new(
            // (int)(this.RealRectangle.X      * FurballGame.VerticalRatio),
            // (int)(this.RealRectangle.Y      * FurballGame.VerticalRatio),
            // (int)(this.RealRectangle.Width  * FurballGame.VerticalRatio),
            // (int)(this.RealRectangle.Height * FurballGame.VerticalRatio)
            // );
            //
            // batch.Begin();
            // base.Draw(time, batch, args);
            // batch.End();
            //
            // FurballGame.Instance.GraphicsDevice.ScissorRectangle = originalRect;
            //
            // batch.Begin();
        }
    }
    public class ChatDrawable : CompositeDrawable {
        private readonly ChatContentsDrawable       _channelContents;
        private readonly RectanglePrimitiveDrawable _background;
        public readonly  UiTextBoxDrawable          MessageInputDrawable;

        public Bindable<string> SelectedChannel = new("#general");

        public override Vector2 Size {
            get;
        }

        private readonly float _padding = 5f;

        public ChatDrawable(Vector2 pos, Vector2 size) {
            this.Position = pos;
            this.Size     = size;

            this._drawables.Add(
            this._background = new(Vector2.Zero, size, 2, true) {
                ColorOverride = new(100, 100, 100, 100)
            }
            );

            this._drawables.Add(
            this.MessageInputDrawable = new(new(0, size.Y), pTypingGame.JapaneseFontStroked, "", 35, size.X) {
                OriginType       = OriginType.BottomLeft,
                DeselectOnCommit = false
            }
            );

            this._drawables.Add(
            this._channelContents = new(new(size.X, size.Y - this.MessageInputDrawable.Size.Y - this._padding)) {
                OriginType = OriginType.BottomLeft,
                Position   = new(0, size.Y - this.MessageInputDrawable.Size.Y - this._padding)
            }
            );

            this.MessageInputDrawable.OnCommit += delegate {
                pTypingGame.OnlineManager.SendMessage(this.SelectedChannel, this.MessageInputDrawable.Text);

                this.MessageInputDrawable.Text = string.Empty;
            };

            pTypingGame.OnlineManager.ChatLog.CollectionChanged += this.ChatLogOnCollectionChanged;
            this.SelectedChannel.OnChange                       += this.SelectedChannelOnChange;

            this.OnClick += this.OnThisClick;

            FurballGame.InputManager.OnMouseScroll += this.OnMouseScroll;

            this.RecalculateAndUpdate_wait_thats_bars();
        }

        private void OnMouseScroll(object? sender, ((int scrollWheelId, float scrollAmount) scroll, string cursorName) valueTuple) {
            if (!this.IsHovered) return;

            this._channelContents.TargetScroll += valueTuple.scroll.scrollAmount;
        }

        private void OnThisClick(object? sender, (MouseButton button, Point pos) e) {
            if (this.Visible)
                this.MessageInputDrawable.OnMouseDown(this, ((MouseButton.Left, e.pos.ToVector2() - this.Position + this.LastCalculatedOrigin), ""));
        }

        private void SelectedChannelOnChange(object sender, string e) => this.RecalculateAndUpdate_wait_thats_bars();

        private void ChatLogOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => this.RecalculateAndUpdate_wait_thats_bars();

        private void RecalculateAndUpdate_wait_thats_bars() {
            IEnumerable<ChatMessage> chatMessages = pTypingGame.OnlineManager.ChatLog.Take(100).Where(message => message.Channel == this.SelectedChannel).Reverse();

            // this._channelContents.ForEach(x => this._drawables.Remove(x));
            // this._channelContents.Clear();
            this._channelContents.Clear();

            float y = this.Size.Y - this.MessageInputDrawable.Size.Y - 10;
            foreach (ChatMessage message in chatMessages) {
                TextDrawable messageDrawable = new(new(0, y), pTypingGame.JapaneseFontStroked, message.ToString(), 35) {
                    OriginType = OriginType.BottomLeft
                };

                messageDrawable.Tags.Add(messageDrawable.Position.Y.ToString());

                if (message.Message.Contains("trans")) {
                    messageDrawable.Colors = new[] {
                        Color.Cyan, Color.Pink, Color.White, Color.Pink
                    };
                    messageDrawable.ColorType = TextColorType.Repeating;
                }

                this._channelContents.PublicDrawables.Add(messageDrawable);

                y -= messageDrawable.Size.Y - 5;
            }

            this._channelContents.TargetScroll = 0;
        }

        public override void Dispose() {
            pTypingGame.OnlineManager.ChatLog.CollectionChanged -= this.ChatLogOnCollectionChanged;
            this.SelectedChannel.OnChange                       -= this.SelectedChannelOnChange;

            FurballGame.InputManager.OnMouseScroll -= this.OnMouseScroll;

            base.Dispose();
        }
    }
}
