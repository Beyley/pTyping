using System;
using System.Globalization;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Helpers;
using Furball.Vixie;
using SixLabors.ImageSharp;

namespace pTyping.Graphics.Drawables; 

public class ScrollableContainer : CompositeDrawable {
    private float _targetScroll;

    private Vector2 _size;
    public override Vector2 Size => this._size;

    public ScrollableContainer(Vector2 size) {
        FurballGame.InputManager.OnMouseScroll += this.OnMouseScroll;
        FurballGame.InputManager.OnMouseMove   += this.OnMouseMove;

        this._size = size;
    }
    private void OnMouseMove(object sender, (Vector2 position, string cursorName) e) {
        this._realHovered = this.RealContains(e.position.ToPoint());
    }

    public void Add(ManagedDrawable drawable) {
        //Save the original Y position
        drawable.Tags.Add(drawable.Position.Y.ToString(CultureInfo.InvariantCulture));
        
        this.Drawables.Add(drawable);

        if (drawable.Position.Y + drawable.Size.Y - this.Size.Y > this._lastMax) {
            this._lastMax = Math.Max(0, drawable.Position.Y + drawable.Size.Y - this.Size.Y);
        }
    }

    private bool _realHovered;
    
    private void OnMouseScroll(object sender, ((int scrollWheelId, float scrollAmount) scroll, string cursorName) e) {
        if(this._realHovered) {
            this._targetScroll -= e.scroll.scrollAmount * this.ScrollSpeed;
            if(!this.InfiniteScrolling)
                this._targetScroll =  Math.Clamp(this._targetScroll, 0, this._lastMax);
        }
    }

    public float ScrollSpeed = 4.0f;

    public  bool  InfiniteScrolling = false;
    private float _lastMax          = 0;

    public override void Update(double time) {
        foreach (ManagedDrawable drawable in this.Drawables) {
            float origY = float.Parse(drawable.Tags[0]);

            float target = origY - this._targetScroll;
            float diff = target - drawable.Position.Y;

            drawable.Position.Y += (float)(diff * 0.985d * time * 8);
        }

        base.Update(time);
    }

    public override void Dispose() {
        FurballGame.InputManager.OnMouseScroll -= this.OnMouseScroll;
        FurballGame.InputManager.OnMouseMove   -= this.OnMouseMove;

        base.Dispose();
    }

    public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
        Rectangle rect = GraphicsBackend.Current.ScissorRect;

        GraphicsBackend.Current.ScissorRect = new Rectangle(new((int) this.RealPosition.X, (int) this.RealPosition.Y), new((int) this._size.X, (int) this._size.Y));
        base.Draw(time, batch, args);

        GraphicsBackend.Current.ScissorRect = rect;
    }

    public void RecalculateMax() {
        this._lastMax = 0;

        foreach (ManagedDrawable drawable in this.Drawables) {
            if (drawable.Position.Y + drawable.Size.Y - this.Size.Y > this._lastMax) {
                this._lastMax = Math.Max(0, drawable.Position.Y + drawable.Size.Y - this.Size.Y);
            }
        }
    }
}
