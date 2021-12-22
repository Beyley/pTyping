using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using JetBrains.Annotations;
using Kettu;
using pTyping.Engine;
using pTyping.Graphics.Player;
using Silk.NET.Input;

namespace pTyping.Graphics.Editor {
    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class EditorTool : IComparable<EditorTool> {
        public const           int     ITEMTEXTSIZE       = 30;
        public const           int     LABELTEXTSIZE      = 35;
        public const           float   TEXTBOXWIDTH       = 300;
        public static readonly Vector2 DROPDOWNBUTTONSIZE = new(250, 35);
        public const           float   LABELAFTERDISTANCE = 5f;
        
        public          UiTickboxDrawable TickBoxDrawable;
        [NotNull]
        public abstract string            Name    { get; }
        [NotNull]
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

        public virtual void OnMouseClick((MouseButton mouseButton, Vector2 position) args) {}

        public virtual void OnMouseDrag(Vector2 position) {}

        public virtual void OnMouseMove(Vector2 position) {}

        public virtual void OnKeyPress(Key key) {}

        public virtual void OnTimeChange(double time) {}

        public virtual void OnNoteCreate(NoteDrawable note, bool isNew) {}

        public virtual void OnNoteDelete(NoteDrawable note) {}

        public virtual void OnEventCreate(ManagedDrawable @event, bool isNew) {}

        public virtual void OnEventDelete(ManagedDrawable note) {}

        public static List<EditorTool> GetAllTools() => ObjectHelper.GetEnumerableOfType<EditorTool>().ToList();
    }
}
