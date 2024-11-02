using Celeste.Mod.Aqua.Core;

namespace Celeste.Mod.Aqua
{
    public class HookCenter
    {
        public event On.Celeste.Level.orig_Begin AfterLevelBegin;
        public event On.Celeste.Level.orig_End AfterLevelEnd;
        public event On.Celeste.Level.orig_LoadLevel BeforeLoadLevel;
        public event On.Celeste.Level.orig_LoadLevel AfterLoadLevel;

        public void Hook()
        {
            On.Celeste.Level.Begin += Level_Begin;
            On.Celeste.Level.End += Level_End;
            On.Celeste.Level.LoadLevel += Level_LoadLevel;

            SolidMovements.Initialize();
            PlayerStates.Initialize();
        }

        public void Unhook()
        {
            On.Celeste.Level.Begin -= Level_Begin;
            On.Celeste.Level.End -= Level_End;
            On.Celeste.Level.LoadLevel -= Level_LoadLevel;

            SolidMovements.Uninitialize();
            PlayerStates.Uninitialize();
        }

        private void Level_Begin(On.Celeste.Level.orig_Begin orig, Level self)
        {
            orig(self);

            AfterLevelBegin?.Invoke(self);
        }

        private void Level_End(On.Celeste.Level.orig_End orig, Level self)
        {
            orig(self);

            AfterLevelEnd?.Invoke(self);
        }

        private void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            BeforeLoadLevel?.Invoke(self, playerIntro, isFromLoader);

            orig(self, playerIntro, isFromLoader);

            AfterLoadLevel?.Invoke(self, playerIntro, isFromLoader);
        }
    }
}
