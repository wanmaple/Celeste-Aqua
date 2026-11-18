using Celeste.Mod.Aqua.Core;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.Aqua.Miscellaneous
{

    public class DataContainer : Component
    {
        public static DataContainer For(Entity obj)
        {
            DataContainer container = obj.Components.Get<DataContainer>();
            if (container is not null)
            {
                return container;
            }

            if (_queuedCount > 0 && _queue.TryGetValue(obj, out container))
            {
                return container;
            }

            container = new DataContainer(obj);
            _queue.Add(obj, container);
            _queuedCount++;
            obj.Add(container); // LockModes.Open 下会立刻触发 Added，并把 container 从 _queue 移除

            return container;
        }

        public static DataContainer OnlyGet(Entity obj)
        {
            DataContainer c = obj.Components.Get<DataContainer>();
            if (c is not null)
            {
                return c;
            }
            if (_queuedCount > 0 && _queue.TryGetValue(obj, out c))
            {
                return c;
            }
            return null;
        }

        private readonly Entity _owner;

        public DataContainer(Entity owner)
            : base(true, false)
        {
            _owner = owner;
            PrevPosition = owner.Position;
        }

        public override void Update()
        {
            base.Update();
            PrevPosition = _owner.Position;
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            if (_queue.Remove(entity))
            {
                _queuedCount--;
            }
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            if (_queue.Remove(entity))
            {
                _queuedCount--;
            }
        }

        public bool Hookable { get; set; } = false;
        public Action<GrapplingHook> OnAttachCallback { get; set; } = null;
        public Action<GrapplingHook> OnDetachCallback { get; set; } = null;
        public bool HookAttached { get; set; } = false;
        public MethodInfo CanCollideMethod { get; set; } = null;
        public Vector2 PrevPosition { get; set; }

        public ulong UniqueId { get; set; } = AUTO_ID++;

        public AccelerationArea.AccelerateState AccelerateState { get; set; } = AccelerationArea.AccelerateState.None;
        public bool Reversed { get; set; } = false;
        public Action<Vector2> PostMovePatch { get; set; } = null;
        public bool EeveePatched { get; set; } = false;
        public MoveToward MoveToward { get; set; }
        public Vector2? RespawnPosition { get; set; }
        public Platform AttachedPlatform { get; set; }
        public Coroutine UndraggableRoutine { get; set; }
        public SoundSource HookInSound { get; set; }
        public bool CassetteActive { get; set; }
        public GrapplingHook GrappleHook { get; set; }
        public GrappleIndicator GrappleIndicator { get; set; }
        public ShotHookCheck ShootCheck { get; set; }
        public bool IsBoosterDash { get; set; }
        public PlayerStates.SlideStates SlideState { get; set; }
        public int PreviousFacing { get; set; }
        public bool RopeIsLoosen { get; set; }
        public int ClimbRopeDirection { get; set; }
        public float LiftSpeedY { get; set; }
        public float FastBubbleMultiplier { get; set; }
        public float RedDashSwingDir { get; set; }
        public float RedDashSwingSpeed { get; set; }
        public bool RedirectionShoot { get; set; }
        public TimeTicker DashHangingTicker { get; set; }
        public TimeTicker BoostSpeedSaveTicker { get; set; }
        public TimeTicker ElectricShockTicker { get; set; }
        private static ulong AUTO_ID = 0u;

        private static ConditionalWeakTable<Entity, DataContainer> _queue = new ConditionalWeakTable<Entity, DataContainer>();
        private static int _queuedCount;
    }
}
