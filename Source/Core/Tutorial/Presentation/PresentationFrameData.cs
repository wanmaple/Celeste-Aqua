using System.IO;

namespace Celeste.Mod.Aqua.Core
{
    public struct PresentationFrameData
    {
        public PlayerFrameData PlayerFrame;
        public HookFrameData HookFrame;

        public void Write(BinaryWriter writer)
        {
            PlayerFrame.Write(writer);
            HookFrame.Write(writer);
        }

        public static PresentationFrameData Read(BinaryReader reader)
        {
            PresentationFrameData ret = new PresentationFrameData();
            ret.PlayerFrame = PlayerFrameData.Read(reader);
            ret.HookFrame = HookFrameData.Read(reader);
            return ret;
        }
    }
}
