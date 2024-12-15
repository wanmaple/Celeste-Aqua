using Celeste.Mod.Aqua.Debug;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    /// <summary>
    /// 暂时不支持缩放和Justify，懒得写。
    /// </summary>
    public class Image9Slice : Image
    {
        public enum RenderMode
        {
            Border,
            Fill,
        }

        public int RenderWidth { get; set; }
        public int RenderHeight { get; set; }
        public RenderMode Mode { get; set; }

        public Image9Slice(MTexture texture, int width, int height, RenderMode mode = RenderMode.Fill)
            : base(texture)
        {
            AquaDebugger.Assert(texture.Width == 24 && texture.Height == 24, "A 9-slice texture must be 24x24.");
            AquaDebugger.Assert(width >= 16 && height >= 16 && width % 8 == 0 && height % 8 == 0, "A 9-slice image should be 16x16 at least and 8-times value.");
            RenderWidth = width;
            RenderHeight = height;
            Mode = mode;
        }

        public override void Render()
        {
            if (Texture != null)
            {
                int borderX = (RenderWidth - 16) / 8;
                int borderY = (RenderHeight - 16) / 8;
                // Cornors
                Texture.Draw(RenderPosition, Origin, Color, Vector2.One, 0.0f, new Rectangle(0, 0, 8, 8));
                Texture.Draw(RenderPosition, Origin - new Vector2(RenderWidth - 8, 0), Color, Vector2.One, 0.0f, new Rectangle((int)Width - 8, 0, 8, 8));
                Texture.Draw(RenderPosition, Origin - new Vector2(0, RenderHeight - 8), Color, Vector2.One, 0.0f, new Rectangle(0, (int)Height - 8, 8, 8));
                Texture.Draw(RenderPosition, Origin - new Vector2(RenderWidth - 8, RenderHeight - 8), Color, Vector2.One, 0.0f, new Rectangle((int)Width - 8, (int)Height - 8, 8, 8));
                Rectangle borderUp = new Rectangle(8, 0, 8, 8);
                Rectangle borderDown = new Rectangle(8, 16, 8, 8);
                for (int x = 1; x <= borderX; x++)
                {
                    Texture.Draw(RenderPosition, Origin - new Vector2(x * 8.0f, 0.0f), Color, Vector2.One, 0.0f, borderUp);
                    Texture.Draw(RenderPosition, Origin - new Vector2(x * 8.0f, RenderHeight - 8), Color, Vector2.One, 0.0f, borderDown);
                }
                Rectangle borderLeft = new Rectangle(0, 8, 8, 8);
                Rectangle borderRight = new Rectangle(16, 8, 8, 8);
                for (int y = 1; y <= borderY; y++)
                {
                    Texture.Draw(RenderPosition, Origin - new Vector2(0.0f, y * 8.0f), Color, Vector2.One, 0.0f, borderLeft);
                    Texture.Draw(RenderPosition, Origin - new Vector2(RenderWidth - 8, y * 8.0f), Color, Vector2.One, 0.0f, borderRight);
                }
                if (Mode == RenderMode.Fill)
                {
                    Rectangle center = new Rectangle(8, 8, 8, 8);
                    for (int x = 1; x <= borderX; x++)
                    {
                        for (int y = 1; y <= borderY; y++)
                        {
                            Texture.Draw(RenderPosition, Origin - new Vector2(x * 8.0f, y * 8.0f), Color, Vector2.UnitY, 0.0f, center);
                        }
                    }
                }
            }
        }
    }
}
