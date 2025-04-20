using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity(
        "Aqua/Unhookable Spike Up = LoadUp",
        "Aqua/Unhookable Spike Right = LoadRight",
        "Aqua/Unhookable Spike Down = LoadDown",
        "Aqua/Unhookable Spike Left = LoadLeft"
        )]
    public class UnhookableSpikes : Spikes
    {
        public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData data) => new UnhookableSpikes(data, offset, Directions.Up);
        public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData data) => new UnhookableSpikes(data, offset, Directions.Right);
        public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData data) => new UnhookableSpikes(data, offset, Directions.Down);
        public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData data) => new UnhookableSpikes(data, offset, Directions.Left);

        public Color SpikeColor { get; private set; }
        public bool BlockUp { get; private set; }
        public bool BlockDown { get; private set; }
        public bool BlockLeft { get; private set; }
        public bool BlockRight { get; private set; }

        public UnhookableSpikes(EntityData data, Vector2 offset, Directions dir)
            : base(data, offset, dir)
        {
            SpikeColor = data.HexColor("color", Color.White);
            BlockUp = data.Bool("block_up", true);
            BlockDown = data.Bool("block_down", true);
            BlockLeft = data.Bool("block_left", true);
            BlockRight = data.Bool("block_right", true);
            StaticMover staticMover = Get<StaticMover>();
            if (staticMover != null)
            {
                staticMover.OnEnable = OnEnabled;
                staticMover.OnDisable = OnDisabled;
            }
            bool attach = data.Bool("attach");
            if (!attach)
            {
                Remove(staticMover);
            }
            this.SetHookable(true);
            Add(new HookInteractable(OnInteractHook));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            SetSpikeColor(SpikeColor);
        }

        private bool OnInteractHook(GrapplingHook grapple, Vector2 at)
        {
            Vector2 hookDir = grapple.ShootDirection;
            if (AquaMaths.BlockDirection(hookDir, this, grapple, BlockUp, BlockDown, BlockLeft, BlockRight))
            {
                Audio.Play("event:/char/madeline/unhookable", grapple.Position);
                grapple.Revoke();
                return true;
            }
            return false;
        }

        private void OnEnabled()
        {
            base.OnEnable();
            SetSpikeColor(SpikeColor);
        }

        private void OnDisabled()
        {
            base.OnDisable();
            SetSpikeColor(SpikeColor);
        }
    }
}
