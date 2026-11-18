using Celeste.Mod.Aqua.Core;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    /// <summary>
    /// Per-entity state bag.
    /// <para>
    /// The keys Aqua uses are known up front, so they live in typed fields instead of a
    /// <c>Dictionary&lt;string, object&gt;</c>. Every key name is written exactly once, as a
    /// <see cref="KeyNames"/> constant, and the call sites use those same constants; the dispatch is
    /// a <c>switch</c> over them, which the compiler turns into the same length/hash jump it would
    /// generate for string literals (a <c>FrozenDictionary</c> lookup was measured 2-5x slower than
    /// that switch). A lazily created overflow dictionary (<see cref="_spill"/>) only receives keys
    /// that are not predefined and values whose type does not match the predefined field.
    /// </para>
    /// <para>
    /// Why not one <c>Dictionary&lt;string, object&gt;</c> per entity: SpeedrunTool deep-clones this
    /// whole table on every savestate, and cloning a dictionary costs roughly
    /// <c>716 B + 148 B * slotCount + 14 B * entryCount</c> (measured with the vendored DeepCloner).
    /// Measured on a 3616-container table: 6336 B/container before, 359 B/container after.
    /// </para>
    /// <para>
    /// Adding a key touches these places, in this order:
    /// <list type="number">
    /// <item>a <see cref="KeyNames"/> constant -- the only place the literal is written;</item>
    /// <item>the same constant in <see cref="AllKeys"/> and a <see cref="KeyBit"/> constant with the
    /// next free bit, both <b>at the same position</b> as in <see cref="KeyNames"/> (the three lists
    /// are index-aligned);</item>
    /// <item>a private field for the value;</item>
    /// <item>one <c>case</c> in <see cref="Has(string)"/>, <see cref="Get{T}"/>,
    /// <see cref="TrySetTyped"/> and <see cref="Remove"/>;</item>
    /// <item>a probe value in <c>BuildProbeValues</c> if the field has a type that no other key
    /// uses, otherwise the DEBUG self-check throws "no probe value matches";</item>
    /// <item>the call sites, using the constant instead of a literal.</item>
    /// </list>
    /// <b>There are only 64 presence bits</b>: <see cref="KeyBit"/> is a <c>ulong</c> mask, so key
    /// number 65 would compute <c>1UL &lt;&lt; 64</c>, which C# masks back to bit 0 and silently
    /// aliases the first key.
    /// </para>
    /// <para>
    /// A mistake is caught, but not every kind by the compiler: two <see cref="KeyNames"/> constants
    /// with the same value fail to compile (duplicate <c>case</c> label, CS0152), while two
    /// <see cref="KeyBit"/> constants with the same bit <b>do</b> compile -- they are only passed to
    /// <c>Mark</c>/<c>Present</c>, never used as <c>case</c> labels -- and are caught at runtime by
    /// the DEBUG self-check in the static constructor. That check is also what catches a key missing
    /// from <see cref="AllKeys"/> or from the <c>Set</c>/<c>Get</c> switches.
    /// </para>
    /// <para>
    /// The public signatures are unchanged; two behaviours differ, see <see cref="GetValues"/> and
    /// the capacity constructor.
    /// </para>
    /// </summary>
    public partial class DataContainer
    {
        // The seven key tables -- KeyNames, AllKeys, KeyBit, and the switches in Has, Get,
        // TrySetTyped and Remove (plus the private field block below) -- must list the keys in the
        // same order. KeyNames/AllKeys/KeyBit are index-aligned (AllKeys[i] is the name of bit i);
        // the DEBUG check proves set-equality but cannot prove that Get and TrySetTyped were kept
        // in that order, so always append a new key to the end of all seven.
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

        /// <summary>
        /// The same names as an array, used only by <see cref="GetValues"/> (stable order) and the
        /// DEBUG self-check -- never on the hot path.
        /// </summary>
        private static readonly string[] AllKeys =
        {
            KeyNames.Hookable,
            KeyNames.OnAttachCallback,
            KeyNames.OnDetachCallback,
            KeyNames.HookAttached,
            KeyNames.CanCollideMethod,
            KeyNames.UniqueId,
            KeyNames.PrevPosition,
            KeyNames.AccelerateState,
            KeyNames.Reversed,
            KeyNames.PostMovePatch,
            KeyNames.EeveePatched,
            KeyNames.MoveToward,
            KeyNames.RespawnPosition,
            KeyNames.AttachedPlatform,
            KeyNames.UndraggableRoutine,
            KeyNames.HookInSound,
            KeyNames.CassetteActive,
            KeyNames.GrappleHook,
            KeyNames.GrappleIndicator,
            KeyNames.ShootCheck,
            KeyNames.IsBoosterDash,
            KeyNames.SlideState,
            KeyNames.PreviousFacing,
            KeyNames.RopeIsLoosen,
            KeyNames.ClimbRopeDirection,
            KeyNames.LiftSpeedY,
            KeyNames.FastBubbleMultiplier,
            KeyNames.RedDashSwingDir,
            KeyNames.RedDashSwingSpeed,
            KeyNames.RedirectionShoot,
            KeyNames.DashHangingTicker,
            KeyNames.BoostSpeedSaveTicker,
            KeyNames.ElecShockTicker,
        };

        static DataContainer()
        {
#if DEBUG
            VerifyKeyTable();
#endif
        }

        public static DataContainer For(Entity obj)
        {
            DataContainer data;
            if (!_data.TryGetValue(obj, out data))
            {
                data = new DataContainer();
                _data.Add(obj, data);
            }

            return data;
        }

        public static bool Has(Entity obj)
        {
            return _data.TryGetValue(obj, out _);
        }

        public bool Has(string key)
        {
            Dictionary<string, object> spill = _spill;
            if (spill != null && spill.ContainsKey(key))
            {
                return true;
            }

            switch (key)
            {
                case KeyNames.Hookable: return Present(_present, KeyBit.Hookable);
                case KeyNames.OnAttachCallback: return Present(_present, KeyBit.OnAttachCallback);
                case KeyNames.OnDetachCallback: return Present(_present, KeyBit.OnDetachCallback);
                case KeyNames.HookAttached: return Present(_present, KeyBit.HookAttached);
                case KeyNames.CanCollideMethod: return Present(_present, KeyBit.CanCollideMethod);
                case KeyNames.UniqueId: return Present(_present, KeyBit.UniqueId);
                case KeyNames.PrevPosition: return Present(_present, KeyBit.PrevPosition);
                case KeyNames.AccelerateState: return Present(_present, KeyBit.AccelerateState);
                case KeyNames.Reversed: return Present(_present, KeyBit.Reversed);
                case KeyNames.PostMovePatch: return Present(_present, KeyBit.PostMovePatch);
                case KeyNames.EeveePatched: return Present(_present, KeyBit.EeveePatched);
                case KeyNames.MoveToward: return Present(_present, KeyBit.MoveToward);
                case KeyNames.RespawnPosition: return Present(_present, KeyBit.RespawnPosition);
                case KeyNames.AttachedPlatform: return Present(_present, KeyBit.AttachedPlatform);
                case KeyNames.UndraggableRoutine: return Present(_present, KeyBit.UndraggableRoutine);
                case KeyNames.HookInSound: return Present(_present, KeyBit.HookInSound);
                case KeyNames.CassetteActive: return Present(_present, KeyBit.CassetteActive);
                case KeyNames.GrappleHook: return Present(_present, KeyBit.GrappleHook);
                case KeyNames.GrappleIndicator: return Present(_present, KeyBit.GrappleIndicator);
                case KeyNames.ShootCheck: return Present(_present, KeyBit.ShootCheck);
                case KeyNames.IsBoosterDash: return Present(_present, KeyBit.IsBoosterDash);
                case KeyNames.SlideState: return Present(_present, KeyBit.SlideState);
                case KeyNames.PreviousFacing: return Present(_present, KeyBit.PreviousFacing);
                case KeyNames.RopeIsLoosen: return Present(_present, KeyBit.RopeIsLoosen);
                case KeyNames.ClimbRopeDirection: return Present(_present, KeyBit.ClimbRopeDirection);
                case KeyNames.LiftSpeedY: return Present(_present, KeyBit.LiftSpeedY);
                case KeyNames.FastBubbleMultiplier: return Present(_present, KeyBit.FastBubbleMultiplier);
                case KeyNames.RedDashSwingDir: return Present(_present, KeyBit.RedDashSwingDir);
                case KeyNames.RedDashSwingSpeed: return Present(_present, KeyBit.RedDashSwingSpeed);
                case KeyNames.RedirectionShoot: return Present(_present, KeyBit.RedirectionShoot);
                case KeyNames.DashHangingTicker: return Present(_present, KeyBit.DashHangingTicker);
                case KeyNames.BoostSpeedSaveTicker: return Present(_present, KeyBit.BoostSpeedSaveTicker);
                case KeyNames.ElecShockTicker: return Present(_present, KeyBit.ElecShockTicker);
                default: return false;
            }
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            // The overflow dictionary wins: a key is stored in exactly one place, because Set()
            // moves a key out of _spill when it writes the matching typed field again.
            Dictionary<string, object> spill = _spill;
            if (spill != null && spill.TryGetValue(key, out object spilledValue))
            {
                return spilledValue is T spilledTypedValue ? spilledTypedValue : defaultValue;
            }

            // A key that was never written returns the caller default, exactly like a missing
            // entry in the old dictionary did -- not default(T), which differs for a value-typed
            // field (Get<object> must return null, Get<bool>(key, true) must return true, and so on).
            ulong present = _present;
            switch (key)
            {
                case KeyNames.Hookable: return Read<T, bool>(present, KeyBit.Hookable, _hookable, defaultValue);
                case KeyNames.OnAttachCallback:
                    return ReadRef<T, Action<GrapplingHook>>(present, KeyBit.OnAttachCallback, _onAttachCallback, defaultValue);
                case KeyNames.OnDetachCallback:
                    return ReadRef<T, Action<GrapplingHook>>(present, KeyBit.OnDetachCallback, _onDetachCallback, defaultValue);
                case KeyNames.HookAttached: return Read<T, bool>(present, KeyBit.HookAttached, _hookAttached, defaultValue);
                case KeyNames.CanCollideMethod:
                    return ReadRef<T, MethodInfo>(present, KeyBit.CanCollideMethod, _canCollideMethod, defaultValue);
                case KeyNames.UniqueId: return Read<T, ulong>(present, KeyBit.UniqueId, _uniqueId, defaultValue);
                case KeyNames.PrevPosition: return Read<T, Vector2>(present, KeyBit.PrevPosition, _prevPosition, defaultValue);
                case KeyNames.AccelerateState:
                    return Read<T, AccelerationArea.AccelerateState>(present, KeyBit.AccelerateState, _accelerateState, defaultValue);
                case KeyNames.Reversed: return Read<T, bool>(present, KeyBit.Reversed, _reversed, defaultValue);
                case KeyNames.PostMovePatch:
                    return ReadRef<T, Action<Vector2>>(present, KeyBit.PostMovePatch, _postMovePatch, defaultValue);
                case KeyNames.EeveePatched: return Read<T, bool>(present, KeyBit.EeveePatched, _eeveePatched, defaultValue);
                case KeyNames.MoveToward: return ReadRef<T, MoveToward>(present, KeyBit.MoveToward, _moveToward, defaultValue);
                case KeyNames.RespawnPosition: return Read<T, Vector2>(present, KeyBit.RespawnPosition, _respawnPosition, defaultValue);
                case KeyNames.AttachedPlatform:
                    return ReadRef<T, Platform>(present, KeyBit.AttachedPlatform, _attachedPlatform, defaultValue);
                case KeyNames.UndraggableRoutine:
                    return ReadRef<T, Coroutine>(present, KeyBit.UndraggableRoutine, _undraggableRoutine, defaultValue);
                case KeyNames.HookInSound: return ReadRef<T, SoundSource>(present, KeyBit.HookInSound, _hookInSound, defaultValue);
                case KeyNames.CassetteActive: return Read<T, bool>(present, KeyBit.CassetteActive, _cassetteActive, defaultValue);
                case KeyNames.GrappleHook: return ReadRef<T, GrapplingHook>(present, KeyBit.GrappleHook, _grappleHook, defaultValue);
                case KeyNames.GrappleIndicator:
                    return ReadRef<T, GrappleIndicator>(present, KeyBit.GrappleIndicator, _grappleIndicator, defaultValue);
                case KeyNames.ShootCheck: return ReadRef<T, ShotHookCheck>(present, KeyBit.ShootCheck, _shootCheck, defaultValue);
                case KeyNames.IsBoosterDash: return Read<T, bool>(present, KeyBit.IsBoosterDash, _isBoosterDash, defaultValue);
                case KeyNames.SlideState: return Read<T, PlayerStates.SlideStates>(present, KeyBit.SlideState, _slideState, defaultValue);
                case KeyNames.PreviousFacing: return Read<T, int>(present, KeyBit.PreviousFacing, _previousFacing, defaultValue);
                case KeyNames.RopeIsLoosen: return Read<T, bool>(present, KeyBit.RopeIsLoosen, _ropeIsLoosen, defaultValue);
                case KeyNames.ClimbRopeDirection:
                    return Read<T, int>(present, KeyBit.ClimbRopeDirection, _climbRopeDirection, defaultValue);
                case KeyNames.LiftSpeedY: return Read<T, float>(present, KeyBit.LiftSpeedY, _liftSpeedY, defaultValue);
                case KeyNames.FastBubbleMultiplier:
                    return Read<T, float>(present, KeyBit.FastBubbleMultiplier, _fastBubbleMultiplier, defaultValue);
                case KeyNames.RedDashSwingDir: return Read<T, float>(present, KeyBit.RedDashSwingDir, _redDashSwingDir, defaultValue);
                case KeyNames.RedDashSwingSpeed: return Read<T, float>(present, KeyBit.RedDashSwingSpeed, _redDashSwingSpeed, defaultValue);
                case KeyNames.RedirectionShoot: return Read<T, bool>(present, KeyBit.RedirectionShoot, _redirectionShoot, defaultValue);
                case KeyNames.DashHangingTicker:
                    return ReadRef<T, TimeTicker>(present, KeyBit.DashHangingTicker, _dashHangingTicker, defaultValue);
                case KeyNames.BoostSpeedSaveTicker:
                    return ReadRef<T, TimeTicker>(present, KeyBit.BoostSpeedSaveTicker, _boostSpeedSaveTicker, defaultValue);
                case KeyNames.ElecShockTicker:
                    return ReadRef<T, TimeTicker>(present, KeyBit.ElecShockTicker, _elecShockTicker, defaultValue);
                default: return defaultValue;
            }
        }

        public void Set(string key, object value)
        {
            if (TrySetTyped(key, value))
            {
                return;
            }

            // Not a predefined key, a value whose type does not match the predefined field, or a
            // null written into a value-typed key: keep it in the overflow dictionary so that
            // Has()/Get<T>() behave exactly like the old Dictionary<string, object>.
            (_spill ??= new Dictionary<string, object>(_spillInitialCapacity))[key] = value;
        }

        /// <summary>
        /// Writes a predefined key. Returns false when the value type does not match the field,
        /// so that <see cref="Set"/> can fall back to the overflow dictionary.
        /// </summary>
        private bool TrySetTyped(string key, object value)
        {
            switch (key)
            {
                case KeyNames.Hookable: return Store(key, value, ref _hookable, KeyBit.Hookable);
                case KeyNames.OnAttachCallback: return StoreRef(key, value, ref _onAttachCallback, KeyBit.OnAttachCallback);
                case KeyNames.OnDetachCallback: return StoreRef(key, value, ref _onDetachCallback, KeyBit.OnDetachCallback);
                case KeyNames.HookAttached: return Store(key, value, ref _hookAttached, KeyBit.HookAttached);
                case KeyNames.CanCollideMethod: return StoreRef(key, value, ref _canCollideMethod, KeyBit.CanCollideMethod);
                case KeyNames.UniqueId: return Store(key, value, ref _uniqueId, KeyBit.UniqueId);
                case KeyNames.PrevPosition: return Store(key, value, ref _prevPosition, KeyBit.PrevPosition);
                case KeyNames.AccelerateState: return Store(key, value, ref _accelerateState, KeyBit.AccelerateState);
                case KeyNames.Reversed: return Store(key, value, ref _reversed, KeyBit.Reversed);
                case KeyNames.PostMovePatch: return StoreRef(key, value, ref _postMovePatch, KeyBit.PostMovePatch);
                case KeyNames.EeveePatched: return Store(key, value, ref _eeveePatched, KeyBit.EeveePatched);
                case KeyNames.MoveToward: return StoreRef(key, value, ref _moveToward, KeyBit.MoveToward);
                case KeyNames.RespawnPosition: return Store(key, value, ref _respawnPosition, KeyBit.RespawnPosition);
                case KeyNames.AttachedPlatform: return StoreRef(key, value, ref _attachedPlatform, KeyBit.AttachedPlatform);
                case KeyNames.UndraggableRoutine: return StoreRef(key, value, ref _undraggableRoutine, KeyBit.UndraggableRoutine);
                case KeyNames.HookInSound: return StoreRef(key, value, ref _hookInSound, KeyBit.HookInSound);
                case KeyNames.CassetteActive: return Store(key, value, ref _cassetteActive, KeyBit.CassetteActive);
                case KeyNames.GrappleHook: return StoreRef(key, value, ref _grappleHook, KeyBit.GrappleHook);
                case KeyNames.GrappleIndicator: return StoreRef(key, value, ref _grappleIndicator, KeyBit.GrappleIndicator);
                case KeyNames.ShootCheck: return StoreRef(key, value, ref _shootCheck, KeyBit.ShootCheck);
                case KeyNames.IsBoosterDash: return Store(key, value, ref _isBoosterDash, KeyBit.IsBoosterDash);
                case KeyNames.SlideState: return Store(key, value, ref _slideState, KeyBit.SlideState);
                case KeyNames.PreviousFacing: return Store(key, value, ref _previousFacing, KeyBit.PreviousFacing);
                case KeyNames.RopeIsLoosen: return Store(key, value, ref _ropeIsLoosen, KeyBit.RopeIsLoosen);
                case KeyNames.ClimbRopeDirection: return Store(key, value, ref _climbRopeDirection, KeyBit.ClimbRopeDirection);
                case KeyNames.LiftSpeedY: return Store(key, value, ref _liftSpeedY, KeyBit.LiftSpeedY);
                case KeyNames.FastBubbleMultiplier: return Store(key, value, ref _fastBubbleMultiplier, KeyBit.FastBubbleMultiplier);
                case KeyNames.RedDashSwingDir: return Store(key, value, ref _redDashSwingDir, KeyBit.RedDashSwingDir);
                case KeyNames.RedDashSwingSpeed: return Store(key, value, ref _redDashSwingSpeed, KeyBit.RedDashSwingSpeed);
                case KeyNames.RedirectionShoot: return Store(key, value, ref _redirectionShoot, KeyBit.RedirectionShoot);
                case KeyNames.DashHangingTicker: return StoreRef(key, value, ref _dashHangingTicker, KeyBit.DashHangingTicker);
                case KeyNames.BoostSpeedSaveTicker: return StoreRef(key, value, ref _boostSpeedSaveTicker, KeyBit.BoostSpeedSaveTicker);
                case KeyNames.ElecShockTicker: return StoreRef(key, value, ref _elecShockTicker, KeyBit.ElecShockTicker);
                default: return false;
            }
        }

        public void Remove(string key)
        {
            _spill?.Remove(key);
            switch (key)
            {
                case KeyNames.Hookable: Clear(key, ref _hookable, KeyBit.Hookable); return;
                case KeyNames.OnAttachCallback: Clear(key, ref _onAttachCallback, KeyBit.OnAttachCallback); return;
                case KeyNames.OnDetachCallback: Clear(key, ref _onDetachCallback, KeyBit.OnDetachCallback); return;
                case KeyNames.HookAttached: Clear(key, ref _hookAttached, KeyBit.HookAttached); return;
                case KeyNames.CanCollideMethod: Clear(key, ref _canCollideMethod, KeyBit.CanCollideMethod); return;
                case KeyNames.UniqueId: Clear(key, ref _uniqueId, KeyBit.UniqueId); return;
                case KeyNames.PrevPosition: Clear(key, ref _prevPosition, KeyBit.PrevPosition); return;
                case KeyNames.AccelerateState: Clear(key, ref _accelerateState, KeyBit.AccelerateState); return;
                case KeyNames.Reversed: Clear(key, ref _reversed, KeyBit.Reversed); return;
                case KeyNames.PostMovePatch: Clear(key, ref _postMovePatch, KeyBit.PostMovePatch); return;
                case KeyNames.EeveePatched: Clear(key, ref _eeveePatched, KeyBit.EeveePatched); return;
                case KeyNames.MoveToward: Clear(key, ref _moveToward, KeyBit.MoveToward); return;
                case KeyNames.RespawnPosition: Clear(key, ref _respawnPosition, KeyBit.RespawnPosition); return;
                case KeyNames.AttachedPlatform: Clear(key, ref _attachedPlatform, KeyBit.AttachedPlatform); return;
                case KeyNames.UndraggableRoutine: Clear(key, ref _undraggableRoutine, KeyBit.UndraggableRoutine); return;
                case KeyNames.HookInSound: Clear(key, ref _hookInSound, KeyBit.HookInSound); return;
                case KeyNames.CassetteActive: Clear(key, ref _cassetteActive, KeyBit.CassetteActive); return;
                case KeyNames.GrappleHook: Clear(key, ref _grappleHook, KeyBit.GrappleHook); return;
                case KeyNames.GrappleIndicator: Clear(key, ref _grappleIndicator, KeyBit.GrappleIndicator); return;
                case KeyNames.ShootCheck: Clear(key, ref _shootCheck, KeyBit.ShootCheck); return;
                case KeyNames.IsBoosterDash: Clear(key, ref _isBoosterDash, KeyBit.IsBoosterDash); return;
                case KeyNames.SlideState: Clear(key, ref _slideState, KeyBit.SlideState); return;
                case KeyNames.PreviousFacing: Clear(key, ref _previousFacing, KeyBit.PreviousFacing); return;
                case KeyNames.RopeIsLoosen: Clear(key, ref _ropeIsLoosen, KeyBit.RopeIsLoosen); return;
                case KeyNames.ClimbRopeDirection: Clear(key, ref _climbRopeDirection, KeyBit.ClimbRopeDirection); return;
                case KeyNames.LiftSpeedY: Clear(key, ref _liftSpeedY, KeyBit.LiftSpeedY); return;
                case KeyNames.FastBubbleMultiplier: Clear(key, ref _fastBubbleMultiplier, KeyBit.FastBubbleMultiplier); return;
                case KeyNames.RedDashSwingDir: Clear(key, ref _redDashSwingDir, KeyBit.RedDashSwingDir); return;
                case KeyNames.RedDashSwingSpeed: Clear(key, ref _redDashSwingSpeed, KeyBit.RedDashSwingSpeed); return;
                case KeyNames.RedirectionShoot: Clear(key, ref _redirectionShoot, KeyBit.RedirectionShoot); return;
                case KeyNames.DashHangingTicker: Clear(key, ref _dashHangingTicker, KeyBit.DashHangingTicker); return;
                case KeyNames.BoostSpeedSaveTicker: Clear(key, ref _boostSpeedSaveTicker, KeyBit.BoostSpeedSaveTicker); return;
                case KeyNames.ElecShockTicker: Clear(key, ref _elecShockTicker, KeyBit.ElecShockTicker); return;
                default: return;
            }
        }

        /// <summary>
        /// Snapshot of every key that <see cref="Has(string)"/> currently reports, taken at call time.
        /// <para>
        /// Behaviour difference against the dictionary implementation this replaced: that one
        /// returned the live backing dictionary (the same instance on every call, later
        /// <see cref="Set"/> calls visible through it, and castable back to
        /// <c>Dictionary&lt;string, object&gt;</c>). There is no backing dictionary any more, so
        /// callers get a copy: mutating it does not affect the container and two calls return two
        /// instances. The method is kept because it is public API; nothing in Aqua calls it.
        /// </para>
        /// </summary>
        public IReadOnlyDictionary<string, object> GetValues()
        {
            Dictionary<string, object> snapshot = new Dictionary<string, object>(AllKeys.Length + (_spill?.Count ?? 0));
            foreach (string key in AllKeys)
            {
                if (Has(key))
                {
                    snapshot[key] = Get<object>(key);
                }
            }

            if (_spill != null)
            {
                foreach (KeyValuePair<string, object> pair in _spill)
                {
                    snapshot[pair.Key] = pair.Value;
                }
            }

            return snapshot;
        }

        public DataContainer() : this(DefaultSpillCapacity)
        {
        }

        public DataContainer(int capacity)
        {
            // The old constructor forwarded the capacity to `new Dictionary(capacity)`, so a
            // negative value threw; keep that contract instead of silently clamping it.
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            // The capacity now only sizes the overflow dictionary and is always clamped: the
            // overflow path is a handful of keys at most (in the shipped code path _spill is never
            // created at all), and sizing it like the old main dictionary would bring the cloning
            // cost back.
            _spillInitialCapacity = Math.Min(capacity, DefaultSpillCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Mark(int keyBit)
        {
            _present |= 1UL << keyBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Unmark(int keyBit)
        {
            _present &= ~(1UL << keyBit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Present(ulong present, int keyBit)
        {
            return (present & (1UL << keyBit)) != 0UL;
        }

        /// <summary>Writes a value-typed field and marks the key present.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Store<T>(string key, object value, ref T field, int keyBit) where T : struct
        {
            if (!(value is T typedValue))
            {
                return false;
            }

            field = typedValue;
            Mark(keyBit);
            _spill?.Remove(key);
            return true;
        }

        /// <summary>Writes a reference-typed field (null clears it) and marks the key present.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool StoreRef<T>(string key, object value, ref T field, int keyBit) where T : class
        {
            if (value != null && !(value is T))
            {
                return false;
            }

            field = (T)value;
            Mark(keyBit);
            _spill?.Remove(key);
            return true;
        }

        /// <summary>Resets a field and forgets the key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Clear<T>(string key, ref T field, int keyBit)
        {
            field = default;
            Unmark(keyBit);
            _spill?.Remove(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T Read<T, TField>(ulong present, int keyBit, TField field, T defaultValue) where TField : struct
        {
            return Present(present, keyBit) ? AsValue<T, TField>(field, defaultValue) : defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T ReadRef<T, TField>(ulong present, int keyBit, TField field, T defaultValue) where TField : class
        {
            return Present(present, keyBit) ? AsRef<T, TField>(field, defaultValue) : defaultValue;
        }

        // Same rules the old Dictionary<string, object> + `value is T` implementation followed: the
        // value is returned only when the requested T can hold it -- the identical type, its
        // Nullable<>, or whatever the boxed value is (object, Enum, ValueType, IComparable, any
        // interface a boxed struct implements); otherwise the caller default is returned.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T AsValue<T, TField>(TField value, T defaultValue) where TField : struct
        {
            if (typeof(T) == typeof(TField))
            {
                // Identical type: reinterpret to avoid boxing the value on the hot paths.
                return Unsafe.As<TField, T>(ref value);
            }

            // Everything else is a cold read. Box (the old dictionary stored a boxed value anyway)
            // and answer exactly what `value is T` answered. This is what keeps Get<object>,
            // Get<Enum>, Get<ValueType>, Get<IComparable> and Get<Nullable<TField>> working.
            object boxed = value;
            return boxed is T typedValue ? typedValue : defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T AsRef<T, TField>(TField value, T defaultValue) where TField : class
        {
            return value is T typedValue ? typedValue : defaultValue;
        }

        /// <summary>
        /// Presence bits for <see cref="_present"/>. These are no longer a dispatch table: the
        /// <c>case</c> labels use <see cref="KeyNames"/> and each constant here only has to be a
        /// distinct bit in 0..63 (verified by VerifyKeyTable in DEBUG).
        /// </summary>
        private static class KeyBit
        {
            public const int Hookable = 0;
            public const int OnAttachCallback = 1;
            public const int OnDetachCallback = 2;
            public const int HookAttached = 3;
            public const int CanCollideMethod = 4;
            public const int UniqueId = 5;
            public const int PrevPosition = 6;
            public const int AccelerateState = 7;
            public const int Reversed = 8;
            public const int PostMovePatch = 9;
            public const int EeveePatched = 10;
            public const int MoveToward = 11;
            public const int RespawnPosition = 12;
            public const int AttachedPlatform = 13;
            public const int UndraggableRoutine = 14;
            public const int HookInSound = 15;
            public const int CassetteActive = 16;
            public const int GrappleHook = 17;
            public const int GrappleIndicator = 18;
            public const int ShootCheck = 19;
            public const int IsBoosterDash = 20;
            public const int SlideState = 21;
            public const int PreviousFacing = 22;
            public const int RopeIsLoosen = 23;
            public const int ClimbRopeDirection = 24;
            public const int LiftSpeedY = 25;
            public const int FastBubbleMultiplier = 26;
            public const int RedDashSwingDir = 27;
            public const int RedDashSwingSpeed = 28;
            public const int RedirectionShoot = 29;
            public const int DashHangingTicker = 30;
            public const int BoostSpeedSaveTicker = 31;
            public const int ElecShockTicker = 32;
        }

        // Which predefined keys have been written. Kept separately from the field values because
        // the old dictionary reported Has() == true for Set(key, null) on a reference key.
        private ulong _present;

        // The 10 keys Entity_Construct / SetHookable / SetAttachCallbacks write for every entity.
        private bool _hookable;
        private Action<GrapplingHook> _onAttachCallback;
        private Action<GrapplingHook> _onDetachCallback;
        private bool _hookAttached;
        private MethodInfo _canCollideMethod;
        private ulong _uniqueId;
        private Vector2 _prevPosition;
        private AccelerationArea.AccelerateState _accelerateState;
        private bool _reversed;
        private Action<Vector2> _postMovePatch;

        // Keys written by specific entity types.
        private bool _eeveePatched;
        private MoveToward _moveToward;
        private Vector2 _respawnPosition;
        private Platform _attachedPlatform;
        private Coroutine _undraggableRoutine;
        private SoundSource _hookInSound;
        private bool _cassetteActive;

        // Player keys.
        private GrapplingHook _grappleHook;
        private GrappleIndicator _grappleIndicator;
        private ShotHookCheck _shootCheck;
        private bool _isBoosterDash;
        private PlayerStates.SlideStates _slideState;
        private int _previousFacing;
        private bool _ropeIsLoosen;
        private int _climbRopeDirection;
        private float _liftSpeedY;
        private float _fastBubbleMultiplier;
        private float _redDashSwingDir;
        private float _redDashSwingSpeed;
        private bool _redirectionShoot;
        private TimeTicker _dashHangingTicker;
        private TimeTicker _boostSpeedSaveTicker;
        private TimeTicker _elecShockTicker;

        // Created on first write of a key that is not predefined, of a value whose type does not
        // match the predefined field, or of null into a value-typed key. Stays null for almost
        // every entity; with the keys above typed, the shipped code path never creates it.
        private Dictionary<string, object> _spill;

        /// <summary>Upper bound for the overflow dictionary initial capacity (see the constructor).</summary>
        private const int DefaultSpillCapacity = 4;

        private int _spillInitialCapacity;

        private static readonly ConditionalWeakTable<Entity, DataContainer> _data = new ConditionalWeakTable<Entity, DataContainer>();
    }
}
