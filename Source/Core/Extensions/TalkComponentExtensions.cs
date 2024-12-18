namespace Celeste.Mod.Aqua.Core
{
    public static class TalkComponentExtensions
    {
        public static void Initialize()
        {
            On.Celeste.TalkComponent.TalkComponentUI.Update += TalkComponentUI_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.TalkComponent.TalkComponentUI.Update -= TalkComponentUI_Update;
        }

        private static void TalkComponentUI_Update(On.Celeste.TalkComponent.TalkComponentUI.orig_Update orig, TalkComponent.TalkComponentUI self)
        {
            orig(self);
            self.Visible = self.Handler.Visible;
        }
    }
}
