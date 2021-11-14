using System;
using System.Collections.Generic;
using System.Linq;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input;
using Kettu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using pTyping.Engine;
using pTyping.Graphics.Player;

namespace pTyping.Graphics.Editor {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ToolOptionAttribute : Attribute {
        public string Name;
        public string ToolTip;

        public ToolOptionAttribute(string name, string tooltip = "") {
            this.Name    = name;
            this.ToolTip = tooltip;
        }
    }
    
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

        /// <summary>
        ///     Register handlers and such here
        /// </summary>
        public virtual void Initialize()   {}
        /// <summary>
        ///     Unregister your handlers and such here
        /// </summary>
        public virtual void Deinitialize() {}

        public virtual void OnMouseClick((MouseButton mouseButton, Point position) args) {}

        public virtual void OnMouseDrag(Point position) {}

        public virtual void OnMouseMove(Point position) {}

        public virtual void OnKeyPress(Keys key) {}

        public virtual void OnTimeChange(double time) {}

        public virtual void OnNoteCreate(NoteDrawable note, bool isNew) {}

        public virtual void OnNoteDelete(NoteDrawable note) {}

        public virtual void OnEventCreate(ManagedDrawable @event, bool isNew) {}

        public virtual void OnEventDelete(ManagedDrawable note) {}

        public static List<EditorTool> GetAllTools() => ObjectHelper.GetEnumerableOfType<EditorTool>().ToList();
    }
}
