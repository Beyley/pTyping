using System;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Input;
using Kettu;
using Microsoft.Xna.Framework;

namespace pTyping.Graphics.Drawables;

public class FurballForm : CompositeDrawable {
    public ManagedDrawable Contents {
        get;
    }

    private readonly TextDrawable               _title;
    private readonly TextDrawable               _closeButton;
    private readonly RectanglePrimitiveDrawable _titleBar;
    private readonly RectanglePrimitiveDrawable _background;

    public event EventHandler OnTryClose;

    public FurballForm(string title, ManagedDrawable contents, OriginType startPosition = OriginType.Center) {
        this.Contents = contents;

        this._title = new(new(2), pTypingGame.JapaneseFont, title, 20) {
            Clickable   = false,
            CoverClicks = false,
            Hoverable   = false,
            CoverHovers = false
        };
        this._closeButton = new(new(this.Contents.Size.X + 8, 2), pTypingGame.JapaneseFont, "x", 20) {
            OriginType = OriginType.TopRight,
            Depth      = 2f
        };
        this._titleBar = new RectanglePrimitiveDrawable(new(0), new(this.Contents.Size.X + 10, 24), 0, true) {
            ColorOverride = new(45, 45, 45, 175)
        };

        //We make the background a little bigger so there is margin
        this._background = new RectanglePrimitiveDrawable(new(0, 24), this.Contents.Size + new Vector2(10), 0, true) {
            ColorOverride = new(30, 30, 30, 175),
            Depth         = 0f
        };

        //Center it in the margin
        this.Contents.Position = new(5, 29);

        //Make sure the contents are above the background
        this.Contents.Depth = 2f;

        this._drawables.Add(this._titleBar);
        this._drawables.Add(this._title);
        this._drawables.Add(this._closeButton);

        this._drawables.Add(this._background);
        this._drawables.Add(this.Contents);

        Vector2 size = base.Size;

        this.Position = startPosition switch {
            OriginType.Center => new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f - size.X / 2, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f - size.Y / 2f),
            _                 => this.Position
        };

        #region Events

        this._titleBar.OnDragBegin += this.OnTitleBarDragBegin;
        this._titleBar.OnDrag      += this.OnTitleBarDrag;

        this._closeButton.OnClick += this.OnCloseButtonClick;

        #endregion

        Logger.Log($"Created form with title {title}");
    }

    private void OnCloseButtonClick(object _, (Point pos, MouseButton button) e) {
        if (this.OnTryClose == null) {
            Logger.Log("Unhandled FurballForm close!");
            return;
        }

        this.OnTryClose.Invoke(this, EventArgs.Empty);
    }

    private Vector2 _startDiffFromPos;
    private Vector2 _startDragMousePos;

    private void OnTitleBarDragBegin(object _, Point startDragPos) {
        this._startDragMousePos = startDragPos.ToVector2();

        this._startDiffFromPos = this._startDragMousePos - this.Position;
    }

    private void OnTitleBarDrag(object _, Point currentDragPos) {
        Vector2 differenceFromStart = currentDragPos.ToVector2() - this._startDragMousePos;

        this.MoveTo(this._startDragMousePos + differenceFromStart - this._startDiffFromPos, 3);
    }
}
