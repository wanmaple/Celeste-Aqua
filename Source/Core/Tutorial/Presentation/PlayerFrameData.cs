using Microsoft.Xna.Framework;
using System.IO;

namespace Celeste.Mod.Aqua.Core
{
    public struct PlayerFrameData
    {
        public Vector2 Position;
        public int Facing;
        public int Gravity;
        public int HairCount;
        public int HairFacing;
        public Vector2 Scale;
        public string AnimationID;
        public int AnimationFrame;
        public Vector2 RenderPosition;

        public void Write(BinaryWriter writer)
        {
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Facing);
            writer.Write(Gravity);
            writer.Write(HairCount);
            writer.Write(HairFacing);
            writer.Write(Scale.X);
            writer.Write(Scale.Y);
            writer.Write(AnimationID);
            writer.Write(AnimationFrame);
            writer.Write(RenderPosition.X);
            writer.Write(RenderPosition.Y);
        }

        public static PlayerFrameData Read(BinaryReader reader)
        {
            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            int facing = reader.ReadInt32();
            int gravity = reader.ReadInt32();
            int hairCount = reader.ReadInt32();
            int hairFacing = reader.ReadInt32();
            float scaleX = reader.ReadSingle();
            float scaleY = reader.ReadSingle();
            string animationID = reader.ReadString();
            int animationFrame = reader.ReadInt32();
            float renderPosX = reader.ReadSingle();
            float renderPosY = reader.ReadSingle();
            return new PlayerFrameData
            {
                Position = new Vector2(posX, posY),
                Facing = facing,
                Gravity = gravity,
                HairCount = hairCount,
                HairFacing = hairFacing,
                Scale = new Vector2(scaleX, scaleY),
                AnimationID = animationID,
                AnimationFrame = animationFrame,
                RenderPosition = new Vector2(renderPosX, renderPosY),
            };
        }
    }
}
