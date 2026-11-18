using Celeste.Mod.Aqua.Core;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Reflection;

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

            container = new DataContainer(obj);
            obj.Add(container);

            return container;
        }

        private readonly Entity _owner;

        public DataContainer(Entity owner)
            : base(false, false)
        {
            _owner = owner;
        }

        public bool Hookable { get; set; }
        public Action<GrapplingHook> OnAttachCallback { get; set; }
        public Action<GrapplingHook> OnDetachCallback { get; set; }
        public bool HookAttached { get; set; }
        public MethodInfo CanCollideMethod { get; set; }
        public Vector2 PrevPosition { get; set;}

        public ulong UniqueId { get; set; }

        public AccelerationArea.AccelerateState AccelerateState { get; set; }
        public bool Reversed { get; set; }
        public Action<Vector2> PostMovePatch { get; set; }
        public bool EeveePatched { get; set; }
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
    }
}
