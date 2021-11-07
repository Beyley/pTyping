using System;
using System.Collections.Generic;
using System.Linq;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input;
using Kettu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using pTyping.LoggingLevels;
using pTyping.Screens;

namespace pTyping.Editor {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ToolOptionAttribute : Attribute {}
    
    public abstract class EditorTool : IComparable<EditorTool> {

        public          UiTickboxDrawable TickBoxDrawable;
        public abstract string            Name    { get; }
        public abstract string            Tooltip { get; }

        protected DrawableManager DrawableManager;
        protected EditorScreen    EditorInstance;

        public int CompareTo(EditorTool other) {
            if (ReferenceEquals(this, other))
                return 0;
            if (ReferenceEquals(null, other))
                return 1;
            int nameComparison = string.Compare(this.Name, other.Name, StringComparison.Ordinal);
            if (nameComparison != 0)
                return nameComparison;
            return string.Compare(this.Tooltip, other.Tooltip, StringComparison.Ordinal);
        }

        public void SelectTool(EditorScreen editor, ref DrawableManager drawableManager) {
            this.DrawableManager = drawableManager;
            this.EditorInstance  = editor;

            this.Initialize();

            Logger.Log($"EditorTool {this.GetType().Name} has been initialized!", LoggerLevelEditorInfo.Instance);
        }

        public void DeselectTool(EditorScreen editor) {
            this.Deinitialize();

            Logger.Log($"EditorTool {this.GetType().Name} has been deinitialized!", LoggerLevelEditorInfo.Instance);
        }

        public virtual void Initialize()   {}
        public virtual void Deinitialize() {}

        public virtual void OnMouseClick((MouseButton mouseButton, Point position) args) {}

        public virtual void OnMouseDrag(Point position) {}

        public virtual void OnMouseMove(Point position) {}

        public virtual void OnKeyPress(Keys key) {}

        public static List<EditorTool> GetAllTools() => ObjectHelper.GetEnumerableOfType<EditorTool>().ToList();
    }
}
