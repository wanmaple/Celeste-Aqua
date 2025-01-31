using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Change Gameplay Mode Trigger")]
    public class ChangeGameplayModeTrigger : Trigger
    {
        public GrapplingHook.GameplayMode GameplayMode { get; set; }
        public int BeginCounter { get; set; }
        public int MaxCounter { get; set; }
        public bool ResetCountInTransition { get; set; }

        public ChangeGameplayModeTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            BeginCounter = data.Int("begin_counter", 0);
            MaxCounter = data.Int("max_counter", -1);
            ResetCountInTransition = data.Bool("reset_count_in_transition", true);
            switch (data.Attr("mode"))
            {
                case "ShootCounter":
                    GameplayMode = GrapplingHook.GameplayMode.ShootCounter;
                    break;
                default:
                    GameplayMode = GrapplingHook.GameplayMode.Default;
                    break;
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            LevelStates.LevelState state = player.level.GetState();
            if (state != null)
            {
                state.GameplayMode = GameplayMode;
                state.InitialShootCount = state.RestShootCount = BeginCounter;
                state.MaxShootCount = MaxCounter;
                state.ResetCountInTransition = ResetCountInTransition;
                var hook = player.GetGrappleHook();
                hook.ChangeGameplayMode(GameplayMode, player.level, BeginCounter);
            }
        }
    }
}
