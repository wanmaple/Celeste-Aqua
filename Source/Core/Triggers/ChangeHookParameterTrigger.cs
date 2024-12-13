using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Change Hook Parameter Trigger")]
    public class ChangeHookParameterTrigger : Trigger
    {
        public string Parameter { get; set; }
        public string Value { get; set; }

        public ChangeHookParameterTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Parameter = data.Attr("parameter");
            Value = data.Attr("value");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            LevelStates.LevelState state = player.level.GetState();
            switch (Parameter)
            {
                case "FeatureEnabled":
                    if (bool.TryParse(Value, out bool val))
                    {
                        state.FeatureEnabled = val;
                    }
                    break;
                case "RopeMaterial":
                    if (int.TryParse(Value, out int material))
                    {
                        state.RopeMaterial = (GrapplingHook.RopeMaterial)material;
                        PlayerStates.MadelinesHook.Material = state.RopeMaterial;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
