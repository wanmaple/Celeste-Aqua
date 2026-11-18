using Monocle;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public class DataContainer
    {
        /// <summary>
        /// The names of the keys this bag holds. Written once here so the call sites do not repeat
        /// the literals.
        /// </summary>
        internal static class KeyNames
        {
            public const string Hookable = "hookable";
            public const string OnAttachCallback = "on_attach_callback";
            public const string OnDetachCallback = "on_detach_callback";
            public const string HookAttached = "hook_attached";
            public const string CanCollideMethod = "can_collide_method";
            public const string UniqueId = "unique_id";
            public const string PrevPosition = "prev_position";
            public const string AccelerateState = "accelerate_state";
            public const string Reversed = "reversed";
            public const string PostMovePatch = "post_move_patch";
            public const string EeveePatched = "eevee_patched";
            public const string MoveToward = "move_toward";
            public const string RespawnPosition = "respawn_position";
            public const string AttachedPlatform = "attached_platform";
            public const string UndraggableRoutine = "undraggable_routine";
            public const string HookInSound = "hook_in_sound";
            public const string CassetteActive = "cassette_active";
            public const string GrappleHook = "grapple_hook";
            public const string GrappleIndicator = "grapple_indicator";
            public const string ShootCheck = "shoot_check";
            public const string IsBoosterDash = "is_booster_dash";
            public const string SlideState = "slide_state";
            public const string PreviousFacing = "previous_facing";
            public const string RopeIsLoosen = "rope_is_loosen";
            public const string ClimbRopeDirection = "climb_rope_direction";
            public const string LiftSpeedY = "lift_speed_y";
            public const string FastBubbleMultiplier = "fast_bubble_multiplier";
            public const string RedDashSwingDir = "red_dash_swing_dir";
            public const string RedDashSwingSpeed = "red_dash_swing_speed";
            public const string RedirectionShoot = "redirection_shoot";
            public const string DashHangingTicker = "dash_hanging_ticker";
            public const string BoostSpeedSaveTicker = "boost_speed_save_ticker";
            public const string ElecShockTicker = "elec_shock_ticker";
        }

        public static DataContainer For(Entity obj)
        {
            DataContainer data;
            if (!_data.TryGetValue(obj, out data))
            {
                data = new DataContainer(32);
                _data.Add(obj, data);
            }
            return data;
        }

        public static bool Has(Entity obj)
        {
            return _data.TryGetValue(obj, out _);
        }

        static ConditionalWeakTable<Entity, DataContainer> _data = new ConditionalWeakTable<Entity, DataContainer>();

        public bool Has(string key)
        {
            return _values.ContainsKey(key);
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (_values.TryGetValue(key, out object value))
            {
                if (value is T tValue)
                    return tValue;
                else
                    return defaultValue;
            }
            else
            {
                return defaultValue;
            }
        }

        public void Set(string key, object value)
        {
            _values[key] = value;
        }

        public void Remove(string key)
        {
            _values.Remove(key);
        }

        public IReadOnlyDictionary<string, object> GetValues()
        {
            return _values;
        }

        public DataContainer() : this(32)
        { }

        public DataContainer(int capacity)
        {
            _values = new Dictionary<string, object>(capacity);
        }

        Dictionary<string, object> _values;
    }
}
