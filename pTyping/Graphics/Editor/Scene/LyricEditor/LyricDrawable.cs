using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using FontStashSharp;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using Furball.Vixie.Backends.Shared.Renderers;
using pTyping.Graphics.Drawables;
using pTyping.Shared;
using pTyping.Shared.Events;
using pTyping.UiElements;
using Silk.NET.Input;

namespace pTyping.Graphics.Editor.Scene.LyricEditor;

public class LyricDrawable : SelectableCompositeDrawable {
	private readonly EditorScreen _editor;

	public readonly  Event             Event;
	private          float             _width;
	private readonly DynamicSpriteFont _font;

	public override Vector2 Size => new Vector2((float)(this.Event.Length * LyricEditorContents.PIXELS_PER_MILISECOND), LyricEditorContents.HEIGHT) * this.Scale;

	public LyricDrawable(EditorScreen editor, Event @event, ReaderWriterLockedObject<ObservableCollection<SelectableCompositeDrawable>> selectedList, Bindable<bool> selectEnabled) : base(selectedList, selectEnabled) {
		this._editor = editor;
		this.Event   = @event;

		if (this.Event.Type != EventType.Lyric)
			throw new ArgumentException("The event must be a Lyric event!", nameof (@event));

		this.TimeSource = pTypingGame.MusicTrackTimeSourceNoOffset;

		this._font = pTypingGame.JapaneseFont.GetFont(LyricEditorContents.HEIGHT - 4f);

		this.OnClick += this.Clicked;
	}

	private void Clicked(object sender, MouseButtonEventArgs e) {
		if (e.Button == MouseButton.Right) {
			ContextMenuDrawable rightClickMenu = new ContextMenuDrawable(e.Mouse.Position, new List<(string, Action)> {
				("Delete", () => {
					throw new NotImplementedException();
				})
			}, pTypingGame.JapaneseFont, 24);

			this._editor.OpenContextMenu(rightClickMenu);
		}
	}

	public override void Update(double time) {
		base.Update(time);

		if (!this.Visible)
			return;

		float timeFromStartToCurrentTime = (float)(this.Event.Start - this.TimeSource.GetCurrentTime());

		this.Position.X = (float)(timeFromStartToCurrentTime * LyricEditorContents.PIXELS_PER_MILISECOND);
	}

	public void Relayout(float width) {
		this._width = width;
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		if (!FurballGame.DisplayRect.IntersectsWith(this.RealRectangle))
			return;

		const float padding = 0.05f;

		float startFull = args.Position.X + this.RealSize.X * padding;
		float endFull   = args.Position.X + this.RealSize.X * (1 - padding);

		float fullLength = endFull - startFull;

		Color fullColor = new Color(255, 255, 255, 100);

		unsafe {
			MappedData data = batch.Reserve(4, 6, FurballGame.WhitePixel);

			const int topLeft     = 0;
			const int topRight    = 1;
			const int bottomLeft  = 2;
			const int bottomRight = 3;

			data.VertexPtr[topLeft].Position = args.Position;
			data.VertexPtr[topRight].Position = args.Position with {
				X = args.Position.X + this.RealSize.X * padding
			};
			data.VertexPtr[bottomLeft].Position = args.Position with {
				Y = args.Position.Y + this.RealSize.Y
			};
			data.VertexPtr[bottomRight].Position = args.Position with {
				X = args.Position.X + this.RealSize.X * padding,
				Y = args.Position.Y + this.RealSize.Y
			};

			data.VertexPtr[topLeft].Color     = fullColor with { A = 0 };
			data.VertexPtr[bottomLeft].Color  = fullColor with { A = 0 };
			data.VertexPtr[topRight].Color    = fullColor;
			data.VertexPtr[bottomRight].Color = fullColor;

			for (int i = 0; i < data.VertexCount; i++)
				data.VertexPtr[i].TexId = data.TextureId;

			data.IndexPtr[0] = (ushort)(topLeft     + data.IndexOffset);
			data.IndexPtr[1] = (ushort)(bottomLeft  + data.IndexOffset);
			data.IndexPtr[2] = (ushort)(topRight    + data.IndexOffset);
			data.IndexPtr[3] = (ushort)(bottomRight + data.IndexOffset);
			data.IndexPtr[4] = (ushort)(topRight    + data.IndexOffset);
			data.IndexPtr[5] = (ushort)(bottomLeft  + data.IndexOffset);
		}

		batch.Draw(
			FurballGame.WhitePixel,
			args.Position + new Vector2(this.Size.X * padding, 0),
			new Vector2(this.Size.X * (1f - padding * 2f), this.RealSize.Y),
			fullColor
		);

		unsafe {
			MappedData data = batch.Reserve(4, 6, FurballGame.WhitePixel);

			const int topLeft     = 0;
			const int topRight    = 1;
			const int bottomLeft  = 2;
			const int bottomRight = 3;

			data.VertexPtr[topLeft].Position = args.Position with {
				X = args.Position.X + this.RealSize.X * (1 - padding)
			};
			data.VertexPtr[topRight].Position = args.Position with {
				X = args.Position.X + this.RealSize.X
			};
			data.VertexPtr[bottomLeft].Position = args.Position with {
				X = args.Position.X + this.RealSize.X * (1 - padding),
				Y = args.Position.Y + this.RealSize.Y
			};
			data.VertexPtr[bottomRight].Position = args.Position with {
				X = args.Position.X + this.RealSize.X,
				Y = args.Position.Y + this.RealSize.Y
			};

			data.VertexPtr[topLeft].Color     = fullColor;
			data.VertexPtr[bottomLeft].Color  = fullColor;
			data.VertexPtr[topRight].Color    = fullColor with { A = 0 };
			data.VertexPtr[bottomRight].Color = fullColor with { A = 0 };

			for (int i = 0; i < data.VertexCount; i++)
				data.VertexPtr[i].TexId = data.TextureId;

			data.IndexPtr[0] = (ushort)(topLeft     + data.IndexOffset);
			data.IndexPtr[1] = (ushort)(bottomLeft  + data.IndexOffset);
			data.IndexPtr[2] = (ushort)(topRight    + data.IndexOffset);
			data.IndexPtr[3] = (ushort)(bottomRight + data.IndexOffset);
			data.IndexPtr[4] = (ushort)(topRight    + data.IndexOffset);
			data.IndexPtr[5] = (ushort)(bottomLeft  + data.IndexOffset);
		}

		Vector2 size = this._font.MeasureString(this.Event.Text);

		if (size.X <= fullLength) {
			batch.DrawString(this._font, this.Event.Text, args.Position + new Vector2(2f, 2f) + new Vector2(this.Size.X * padding, 0), Color.White);
		}
		else {
			Vector2 scale = new Vector2(fullLength / size.X);

			Vector2 scaledSize = this._font.MeasureString(this.Event.Text, scale);

			float y = args.Position.Y + this.RealSize.Y / 2f - scaledSize.Y / 2f;

			batch.DrawString(this._font, this.Event.Text, new Vector2(2f + args.Position.X, y) + new Vector2(this.Size.X * padding, 0), Color.White, 0, scale);
		}

		if (this.IsHovered) {
			const float stopperTopToLyricTop = 10f;

			const float stopperWidth = 2f;

			//Draw 2 stoppers on the left and right
			batch.Draw(FurballGame.WhitePixel, args.Position + new Vector2(0, -stopperTopToLyricTop), new Vector2(stopperWidth, this.RealSize.Y               + stopperTopToLyricTop * 2), Color.White);
			batch.Draw(FurballGame.WhitePixel, args.Position + new Vector2(this.RealSize.X, -stopperTopToLyricTop), new Vector2(stopperWidth, this.RealSize.Y + stopperTopToLyricTop * 2), Color.White);

			//Draw short lines at the top and bottom of the stopper pointing inwards
			batch.Draw(FurballGame.WhitePixel, args.Position + new Vector2(0, -stopperTopToLyricTop), new Vector2(10, stopperWidth), Color.White);
			batch.Draw(FurballGame.WhitePixel, args.Position + new Vector2(0, this.RealSize.Y + stopperTopToLyricTop), new Vector2(10, stopperWidth), Color.White);

			batch.Draw(FurballGame.WhitePixel, args.Position + new Vector2(this.RealSize.X + stopperWidth, -stopperTopToLyricTop + stopperWidth), new Vector2(10, stopperWidth), Color.White, MathF.PI);
			batch.Draw(FurballGame.WhitePixel, args.Position + new Vector2(this.RealSize.X + stopperWidth, this.RealSize.Y       + stopperTopToLyricTop + stopperWidth), new Vector2(10, stopperWidth), Color.White, MathF.PI);
		}

		base.Draw(time, batch, args);
	}
}
