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

            if (_queue.TryGetValue(obj, out container))
            {
                return container;
            }

            container = new DataContainer(obj);
            container.PrevPosition = obj.Position;
            _queue.Add(obj, container);
            obj.Add(container); // 如果 obj.Components.LockMode = LockModes.Open, 这会立刻触发 Added 从而将 container 从 _queue 移除

            return container;
        }

        public static DataContainer OnlyGet(Entity obj)
        {
            return obj.Components.Get<DataContainer>();
        }

        private readonly Entity _owner;

        public DataContainer(Entity owner)
            : base(true, false)
        {
            _owner = owner;
        }

        public override void Update()
        {
            base.Update();
            PrevPosition = _owner.Position;
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            _queue.Remove(entity);
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            _queue.Remove(entity);
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
    }
}
