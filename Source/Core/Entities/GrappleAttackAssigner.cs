using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.Aqua.Core.SimpleGrappleInteractionAssigner;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Grapple Attack Assigner")]
    public class GrappleAttackAssigner : GrappleInteractionAssigner
    {
        public bool CanDash { get; private set; }

        public GrappleAttackAssigner(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            CanDash = data.Bool("can_dash", true);
            this.MakeExtraCollideCondition();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            var platforms = scene.Tracker.GetEntities<Platform>();
            foreach (Platform platform in platforms)
            {
                if (CollideCheck(platform) && !IsInBlacklist(platform))
                {
                    AssignInteractionToEntity(platform);
                }
            }
            RemoveSelf();
        }

        private void AssignInteractionToEntity(Platform platform)
        {
            if (!CanDash)
            {
                platform.OnDashCollide = null;
            }
            if (platform is CrushBlock kevin)
            {
                kevin.Add(new HookInteractable((grapple, at) =>
                {
                    Vector2 direction = grapple.ConvertToHitDirection(kevin, grapple.ShootDirection);
                    if (kevin.CanActivate(-direction))
                    {
                        grapple.Revoke();
                        kevin.Attack(-direction);
                        return true;
                    }
                    return false;
                }));
            }
        }

        private bool CanCollide(Entity other)
        {
            return other is CrushBlock;
        }
    }
}
