using Celeste.Mod.Aqua.Debug;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(true)]
    public abstract class SlidableSolid : Solid
    {
        public float HookSmoothCoefficient { get; private set; }

        public SlidableSolid(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, true)
        {
            HookSmoothCoefficient = data.Float("hook_smooth", 2.5f);
        }

        public override void Update()
        {
            base.Update();
            if (this.IsHookAttached())
            {
                GrapplingHook hook = Scene.Tracker.GetEntity<GrapplingHook>();
                if (hook.AlongRopeSpeed > 0.0f)
                {
                    if (hook.Bottom == Top || hook.Top == Bottom)
                    {
                        Vector2 movement = Vector2.Dot(hook.AlongRopeSpeed * hook.HookDirection, Vector2.UnitX) * Vector2.UnitX * Engine.DeltaTime * HookSmoothCoefficient;
                        hook.SetPositionRounded(hook.Position + movement);
                        if (hook.Right <= Left || hook.Left >= Right)
                        {
                            hook.Revoke();
                        }
                    }
                    else if (hook.Left == Right || hook.Right == Left)
                    {
                        Vector2 movement = Vector2.Dot(hook.AlongRopeSpeed * hook.HookDirection, Vector2.UnitY) * Vector2.UnitY * Engine.DeltaTime * HookSmoothCoefficient;
                        hook.SetPositionRounded(hook.Position + movement);
                        if (hook.Top >= Bottom || hook.Bottom <= Top)
                        {
                            hook.Revoke();
                        }
                    }
                }
            }
        }
    }
}
