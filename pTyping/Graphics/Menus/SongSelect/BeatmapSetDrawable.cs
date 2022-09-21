using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using Furball.Vixie.Backends.Shared.Renderers;
using pTyping.Shared.Beatmaps;

namespace pTyping.Graphics.Menus.SongSelect;

public class BeatmapSetDrawable : CompositeDrawable {
    public readonly BeatmapSet BeatmapSet;

    private readonly SetTitleDrawable setTitle;

    public float YPositionInDrawable = 0;

    public BeatmapSetDrawable(BeatmapSet set) {
        this.BeatmapSet = set;

        this.Drawables.Add(this.setTitle = new SetTitleDrawable(Vector2.Zero, $"{set.Artist} - {set.Title}"));

        bool  first = true;
        float y     = this.setTitle.Size.Y;
        foreach (Beatmap map in set.Beatmaps) {
            DifficultyDrawable drawable = new(map, first) {
                OriginType = OriginType.TopLeft
            };
            drawable.Position = new Vector2(this.setTitle.Size.X - drawable.Size.X, y);

            this.Drawables.Add(drawable);

            y     += drawable.Size.Y;
            first =  false;
        }

        this.InvisibleToInput = true;
    }

    private sealed class DifficultyDrawable : CompositeDrawable {
        private const float FONT_SIZE = 30;
        private const int   MARGIN    = 2;

        private readonly TextDrawable _difficultyName;
        private readonly bool         _first;

        public readonly Beatmap Beatmap;
        public override Vector2 Size => new(700, FONT_SIZE + MARGIN * 2);

        public DifficultyDrawable(Beatmap map, bool first) {
            this.Beatmap = map;
            this._first  = first;

            this._difficultyName = new TextDrawable(new Vector2(MARGIN), pTypingGame.JapaneseFont, map.Info.DifficultyName.ToString(), FONT_SIZE) {
                Clickable   = false,
                CoverClicks = false,
                Hoverable   = false,
                CoverHovers = false
            };
            
            this.Drawables.Add(this._difficultyName);

            this.OnClick += this.OnMapClick;
        }
        private void OnMapClick(object sender, MouseButtonEventArgs e) {
            pTypingGame.CurrentSong.Value = this.Beatmap;
        }

        public override void Dispose() {
            base.Dispose();

            this.OnClick -= this.OnMapClick;
        }

        public override unsafe void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
            MappedData mappedData = batch.Reserve(4, 6);

            const int topLeft     = 0;
            const int topRight    = 1;
            const int bottomLeft  = 2;
            const int bottomRight = 3;

            mappedData.VertexPtr[topLeft].Position = args.Position with {
                X = this._first ? args.Position.X - 50 : args.Position.X
            };
            mappedData.VertexPtr[topRight].Position = args.Position with {
                X = args.Position.X + this.RealSize.X
            };
            mappedData.VertexPtr[bottomLeft].Position = args.Position with {
                Y = args.Position.Y + this.RealSize.Y
            };
            mappedData.VertexPtr[bottomRight].Position = new Vector2(args.Position.X + this.RealSize.X, args.Position.Y + this.RealSize.Y);

            long texId = batch.GetTextureIdForReserve(FurballGame.WhitePixel);
            for (int i = 0; i < mappedData.VertexCount; i++) {
                mappedData.VertexPtr[i].TexId = texId;
            }

            Color c = Equals(this.Beatmap, pTypingGame.CurrentSong.Value) ? new Color(200, 100, 100) : new Color(100, 100, 200);
            mappedData.VertexPtr[topLeft].Color     = new Color(c.R, c.G, c.B, (byte)200);
            mappedData.VertexPtr[bottomLeft].Color  = new Color(c.R, c.G, c.B, (byte)200);
            mappedData.VertexPtr[topRight].Color    = new Color(c.R, c.G, c.B, (byte)100);
            mappedData.VertexPtr[bottomRight].Color = new Color(c.R, c.G, c.B, (byte)100);

            mappedData.IndexPtr[0] = (ushort)(topLeft     + mappedData.IndexOffset);
            mappedData.IndexPtr[1] = (ushort)(bottomLeft  + mappedData.IndexOffset);
            mappedData.IndexPtr[2] = (ushort)(topRight    + mappedData.IndexOffset);
            mappedData.IndexPtr[3] = (ushort)(bottomRight + mappedData.IndexOffset);
            mappedData.IndexPtr[4] = (ushort)(topRight    + mappedData.IndexOffset);
            mappedData.IndexPtr[5] = (ushort)(bottomLeft  + mappedData.IndexOffset);

            base.Draw(time, batch, args);
        }
    }

    private sealed class SetTitleDrawable : CompositeDrawable {
        private const int   MARGIN    = 4;
        private const float FONT_SIZE = 35;

        public override Vector2 Size {
            get => new(750, FONT_SIZE + MARGIN * 2);
        }

        private readonly TextDrawable _text;
        public SetTitleDrawable(Vector2 position, string text) {
            this.Position = position;

            this._text = new TextDrawable(new Vector2(this.Size.X - MARGIN, MARGIN), pTypingGame.JapaneseFont, text, FONT_SIZE) {
                OriginType = OriginType.TopRight
            };
            this.Drawables.Add(this._text);
        }

        public override unsafe void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
            MappedData mappedData = batch.Reserve(4, 6);

            const int topLeft     = 0;
            const int topRight    = 1;
            const int bottomLeft  = 2;
            const int bottomRight = 3;

            mappedData.VertexPtr[topLeft].Position = new Vector2(args.Position.X, args.Position.Y);
            mappedData.VertexPtr[topRight].Position = args.Position with {
                X = args.Position.X + this.RealSize.X
            };
            mappedData.VertexPtr[bottomLeft].Position = args.Position with {
                Y = args.Position.Y + this.RealSize.Y
            };
            mappedData.VertexPtr[bottomRight].Position = new Vector2(args.Position.X + this.RealSize.X, args.Position.Y + this.RealSize.Y);

            long texId = batch.GetTextureIdForReserve(FurballGame.WhitePixel);
            for (int i = 0; i < mappedData.VertexCount; i++) {
                mappedData.VertexPtr[i].TexId = texId;
            }

            mappedData.VertexPtr[topLeft].Color     = new Color(200, 200, 200, 200);
            mappedData.VertexPtr[bottomLeft].Color  = new Color(200, 200, 200, 200);
            mappedData.VertexPtr[topRight].Color    = new Color(200, 200, 200, 100);
            mappedData.VertexPtr[bottomRight].Color = new Color(200, 200, 200, 100);

            mappedData.IndexPtr[0] = (ushort)(topLeft     + mappedData.IndexOffset);
            mappedData.IndexPtr[1] = (ushort)(bottomLeft  + mappedData.IndexOffset);
            mappedData.IndexPtr[2] = (ushort)(topRight    + mappedData.IndexOffset);
            mappedData.IndexPtr[3] = (ushort)(bottomRight + mappedData.IndexOffset);
            mappedData.IndexPtr[4] = (ushort)(topRight    + mappedData.IndexOffset);
            mappedData.IndexPtr[5] = (ushort)(bottomLeft  + mappedData.IndexOffset);

            base.Draw(time, batch, args);
        }
    }
}
