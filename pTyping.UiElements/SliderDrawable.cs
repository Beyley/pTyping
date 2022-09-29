using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using pTyping.Shared.ObjectModel;

namespace pTyping.UiElements;

public class SliderDrawable <T> : Drawable where T : struct, IComparable, IConvertible, IEquatable<T>, IComparable<T> {
	private readonly BoundNumber<T> Value;

	public const float LINE_WIDTH = 1.5f;
	public const float STOPPER_LINE_WIDTH = 1;
	public const float HEIGHT     = 35;

	public float Width;

	public override Vector2 Size => new Vector2(this.Width, HEIGHT);

	public SliderDrawable(BoundNumber<T> num, float width = 300) {
		this.Value = num;
		this.Width = width;
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		base.Draw(time, batch, args);

		const float middle                  = HEIGHT / 2f;
		const float stopperHeightFromMiddle = 7;

		//Draw the main line that you drag across
		batch.Draw(FurballGame.WhitePixel, args.Position with {
			Y = args.Position.Y + middle
		}, new Vector2(this.Width, LINE_WIDTH));

		//Draw the left stopper
		batch.Draw(FurballGame.WhitePixel, args.Position with {
			Y = args.Position.Y + middle - stopperHeightFromMiddle
		}, new Vector2(STOPPER_LINE_WIDTH, stopperHeightFromMiddle * 2f));

		//Draw the right stopper
		batch.Draw(FurballGame.WhitePixel, new Vector2(args.Position.X + this.RealSize.X, args.Position.Y + middle - stopperHeightFromMiddle), new Vector2(STOPPER_LINE_WIDTH, stopperHeightFromMiddle * 2f));
	}
}
