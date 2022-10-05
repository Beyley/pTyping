using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using FontStashSharp;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using pTyping.Online;

namespace pTyping.Graphics.Online;

public class ChatContentsDrawable : CompositeDrawable {
	public List<Drawable> PublicDrawables => this.Drawables;

	public float TargetScroll;

	public override Vector2 Size {
		get;
	} //TODO

	public ChatContentsDrawable(Vector2 size) {
		this.Size = size;
	}

	public void Clear() {
		this.Drawables.Clear();
	}

	public override void Update(double time) {
		if (this.Drawables.Count != 0) {
			this.TargetScroll = Math.Max(this.TargetScroll, 0);

			foreach (Drawable drawable in this.Drawables) {
				float startY = float.Parse(drawable.Tags.First());

				float targetY = startY + this.TargetScroll;

				float difference = targetY - drawable.Position.Y;

				drawable.Position.Y += (float)(difference * time * 1000 * 0.01);
			}
		}

		base.Update(time);
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		batch.End();
		//
		// Rectangle originalRect = FurballGame.Instance.GraphicsDevice.ScissorRectangle;
		//
		// FurballGame.Instance.GraphicsDevice.ScissorRectangle = new(
		// (int)(this.RealRectangle.X      ),
		// (int)(this.RealRectangle.Y      ),
		// (int)(this.RealRectangle.Width  ),
		// (int)(this.RealRectangle.Height )
		// );
		//
		batch.Begin();
		base.Draw(time, batch, args);
		batch.End();

		// FurballGame.Instance.GraphicsDevice.ScissorRectangle = originalRect;

		batch.Begin();
	}
}

public class ChatDrawable : CompositeDrawable {
	private readonly ChatContentsDrawable       _channelContents;
	private readonly RectanglePrimitiveDrawable _background;
	public readonly  DrawableTextBox            MessageInputDrawable;

	public Bindable<string> SelectedChannel = new Bindable<string>("#general");

	public override Vector2 Size {
		get;
	} //TODO

	private readonly List<DrawableButton> _channelButtons = new List<DrawableButton>();

	private readonly float _padding = 5f;

	private void UpdateChannelButtons(object _, NotifyCollectionChangedEventArgs __) {
		lock (pTypingGame.OnlineManager.KnownChannels) {
			this._channelButtons.ForEach(x => this.Drawables.Remove(x));
			this._channelButtons.Clear();

			float x = 0f;
			foreach (string channel in pTypingGame.OnlineManager.KnownChannels) {
				DrawableButton button = new DrawableButton(new Vector2(x, 0), FurballGame.DefaultFont, 30, channel, Color.Blue, Color.White, Color.Black, new Vector2(100, 32)) {
					OriginType = OriginType.BottomLeft
				};
				x += button.Size.X;

				button.OnClick += delegate {
					this.SelectedChannel.Value = channel;
				};

				this.Drawables.Add(button);
			}
		}
	}

	public ChatDrawable(Vector2 pos, Vector2 size) {
		this.Position = pos;
		this.Size     = size;

		this.Drawables.Add(
			this._background = new RectanglePrimitiveDrawable(Vector2.Zero, size, 2, true) {
				ColorOverride = new Color(100, 100, 100, 100)
			}
		);

		this.Drawables.Add(
			this.MessageInputDrawable = new DrawableTextBox(new Vector2(0, size.Y), pTypingGame.JapaneseFont, 35, size.X) {
				OriginType       = OriginType.BottomLeft,
				DeselectOnCommit = false
			}
		);

		this.Drawables.Add(
			this._channelContents = new ChatContentsDrawable(new Vector2(size.X, size.Y - this.MessageInputDrawable.Size.Y - this._padding)) {
				OriginType = OriginType.BottomLeft,
				Position   = new Vector2(0, size.Y - this.MessageInputDrawable.Size.Y - this._padding)
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

		this.UpdateChannelButtons(null, null);

		pTypingGame.OnlineManager.KnownChannels.CollectionChanged += this.UpdateChannelButtons;
	}

	private void OnMouseScroll(object sender, MouseScrollEventArgs mouseScrollEventArgs) {
		if (!this.IsHovered) return;

		this._channelContents.TargetScroll += mouseScrollEventArgs.ScrollAmount.Y;
	}

	private void OnThisClick(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		if (this.Visible) //((MouseButton.Left, mouseButtonEventArgs.Mouse.Position - this.Position + this.LastCalculatedOrigin), "")
			this.MessageInputDrawable.OnMouseDown(this, mouseButtonEventArgs);
	}

	private void SelectedChannelOnChange(object sender, string e) {
		this.RecalculateAndUpdate_wait_thats_bars();
	}

	private void ChatLogOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
		this.RecalculateAndUpdate_wait_thats_bars();
	}

	protected class ChatMessageDrawable : TextDrawable {
		private readonly ChatDrawable _chat;

		public ChatMessageDrawable(ChatDrawable d, Vector2 position, FontSystem font, string text, int size) : base(position, font, text, size) {
			this._chat = d;
		}

		public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
			if (this._chat.RealContains(this.RealPosition))
				base.Draw(time, batch, args);
		}
	}

	private void RecalculateAndUpdate_wait_thats_bars() {
		lock (pTypingGame.OnlineManager.ChatLog) {
			IEnumerable<ChatMessage> chatMessages = pTypingGame.OnlineManager.ChatLog.Skip(Math.Max(0, pTypingGame.OnlineManager.ChatLog.Count - 100))
															   .Where(message => message.Channel == this.SelectedChannel).Reverse();

			// this._channelContents.ForEach(x => this._drawables.Remove(x));
			// this._channelContents.Clear();
			this._channelContents.Clear();

			float y = this.Size.Y - this.MessageInputDrawable.Size.Y - 10;
			foreach (ChatMessage message in chatMessages) {
				ChatMessageDrawable messageDrawable = new ChatMessageDrawable(this, new Vector2(0, y), pTypingGame.JapaneseFont, message.ToString(), 35) {
					OriginType = OriginType.BottomLeft
				};

				messageDrawable.Tags.Add(messageDrawable.Position.Y.ToString());

				if (message.Message.Contains("trans")) {
					messageDrawable.Colors = new[] {
						FSColor.Cyan, FSColor.Pink, FSColor.White, FSColor.Pink
					};
					messageDrawable.ColorType = TextColorType.Repeating;
				}

				this._channelContents.PublicDrawables.Add(messageDrawable);

				y -= messageDrawable.Size.Y - 5;
			}

			this._channelContents.TargetScroll = 0;
		}
	}

	public override void Dispose() {
		pTypingGame.OnlineManager.ChatLog.CollectionChanged -= this.ChatLogOnCollectionChanged;
		this.SelectedChannel.OnChange                       -= this.SelectedChannelOnChange;

		FurballGame.InputManager.OnMouseScroll -= this.OnMouseScroll;

		pTypingGame.OnlineManager.KnownChannels.CollectionChanged -= this.UpdateChannelButtons;

		base.Dispose();
	}
}
