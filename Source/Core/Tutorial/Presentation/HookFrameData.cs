using Microsoft.Xna.Framework;
using System.IO;

namespace Celeste.Mod.Aqua.Core
{
    public struct HookFrameData
    {
        public bool Active;
        public Vector2 Position;
        public float Rotation;
        public Vector2[] Pivots;
        public string AnimationID;
        public int AnimationFrame;

        public void Write(BinaryWriter writer)
        {
            writer.Write(Active);
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Rotation);
            writer.Write(Pivots.Length);
            for (int i = 0; i < Pivots.Length; i++)
            {
                writer.Write(Pivots[i].X);
                writer.Write(Pivots[i].Y);
            }
            writer.Write(AnimationID);
            writer.Write(AnimationFrame);
        }

        public static HookFrameData Read(BinaryReader reader)
        {
            bool active = reader.ReadBoolean();
            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            float rotation = reader.ReadSingle();
            int len = reader.ReadInt32();
            Vector2[] pivots = new Vector2[len];
            for (int i = 0; i < len; i++)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                pivots[i] = new Vector2(x, y);
            }
            string animationID = reader.ReadString();
            int animationFrame = reader.ReadInt32();
            return new HookFrameData
            {
                Active = active,
                Position = new Vector2(posX, posY),
                Rotation = rotation,
                Pivots = pivots,
                AnimationID = animationID,
                AnimationFrame = animationFrame,
            };
        }
    }
}
