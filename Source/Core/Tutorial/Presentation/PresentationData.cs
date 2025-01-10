using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace Celeste.Mod.Aqua.Core
{
    public class PresentationData
    {
        public List<PresentationFrameData> Frames;

        public PresentationData()
        {
        }

        public void Trim()
        {
            if (Frames.Count > 1)
            {
                PresentationFrameData first = Frames[0];
                Vector2 playerPos = first.PlayerFrame.Position;
                Vector2 playerScale = first.PlayerFrame.Scale;
                bool hookActive = first.HookFrame.Active;
                Vector2 hookPos = first.HookFrame.Position;
                int trimSize = 0;
                for (int i = 1; i < Frames.Count; i++)
                {
                    PlayerFrameData playerFrame = Frames[i].PlayerFrame;
                    HookFrameData hookFrame = Frames[i].HookFrame;
                    if (playerFrame.Position == playerPos && playerFrame.Scale == playerScale && hookActive == hookFrame.Active && hookPos == hookFrame.Position)
                    {
                        ++trimSize;
                    }
                    else
                        break;
                }
                if (trimSize > 0)
                {
                    Frames.RemoveRange(1, trimSize);
                }
            }
            if (Frames.Count > 1)
            {
                PresentationFrameData last = Frames[Frames.Count - 1];
                Vector2 playerPos = last.PlayerFrame.Position;
                Vector2 playerScale = last.PlayerFrame.Scale;
                bool hookActive = last.HookFrame.Active;
                Vector2 hookPos = last.HookFrame.Position;
                int trimSize = 0;
                for (int i = Frames.Count - 2; i >= 0; i--)
                {
                    PlayerFrameData playerFrame = Frames[i].PlayerFrame;
                    HookFrameData hookFrame = Frames[i].HookFrame;
                    if (playerFrame.Position == playerPos && playerFrame.Scale == playerScale && hookActive == hookFrame.Active && hookPos == hookFrame.Position)
                    {
                        ++trimSize;
                    }
                    else
                        break;
                }
                if (trimSize > 0)
                {
                    Frames.RemoveRange(Frames.Count - 1 - trimSize, trimSize);
                }
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Frames.Count);
            foreach (PresentationFrameData frame in Frames)
            {
                frame.Write(writer);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            int cnt = reader.ReadInt32();
            if (Frames == null)
                Frames = new List<PresentationFrameData>(cnt);
            else
                Frames.Clear();
            for (int i = 0; i < cnt; i++)
            {
                Frames.Add(PresentationFrameData.Read(reader));
            }
        }
    }
}
