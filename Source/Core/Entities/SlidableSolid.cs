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
            Add(new ClimbBlocker(false));
        }

        public override void Update()
        {
            base.Update();
            if (this.IsHookAttached())
            {
                var grapples = Scene.Tracker.GetEntities<GrapplingHook>();
                foreach (GrapplingHook grapple in grapples)
                {
                    if (grapple.AlongRopeSpeed > 0.0f)
                    {
                        if (grapple.Bottom == Top || grapple.Top == Bottom)
                        {
                            Vector2 movement = Vector2.Dot(grapple.AlongRopeSpeed * grapple.HookDirection, Vector2.UnitX) * Vector2.UnitX * Engine.DeltaTime * HookSmoothCoefficient;
                            grapple.SetPositionRounded(grapple.Position + movement);
                            if (grapple.Right <= Left || grapple.Left >= Right)
                            {
                                grapple.Revoke();
                            }
                        }
                        else if (grapple.Left == Right || grapple.Right == Left)
                        {
                            Vector2 movement = Vector2.Dot(grapple.AlongRopeSpeed * grapple.HookDirection, Vector2.UnitY) * Vector2.UnitY * Engine.DeltaTime * HookSmoothCoefficient;
                            grapple.SetPositionRounded(grapple.Position + movement);
                            if (grapple.Top >= Bottom || grapple.Bottom <= Top)
                            {
                                grapple.Revoke();
                            }
                        }
                    }
                }
            }
        }
    }
}
