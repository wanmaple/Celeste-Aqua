using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Custom Cutscene Trigger")]
    public class CustomCutsceneTrigger : Trigger
    {
        public string CutsceneID { get; private set; }
        public bool FadeInOnSkip { get; private set; }
        public bool EndingChapter { get; private set; }

        public string CutsceneName => NAME_PREFIX + CutsceneID;

        public CustomCutsceneTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            CutsceneID = data.Attr("cutsceneId");
            FadeInOnSkip = data.Bool("fadeInOnSkip");
            EndingChapter = data.Bool("endingChapter");
        }

        public override void OnEnter(Player player)
        {
            if (AquaModule.Session.HasFlag(CutsceneName))
                return;

            if (!_trigged)
            {
                base.OnEnter(player);
                CustomCutscene cutscene = new CustomCutscene(CutsceneName, FadeInOnSkip, EndingChapter);
                Scene.Add(cutscene);
                _trigged = true;
            }
        }

        private bool _trigged = false;

        private const string NAME_PREFIX = "AquaCutscene_";
    }
}
