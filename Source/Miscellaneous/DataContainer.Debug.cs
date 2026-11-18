using Celeste.Mod.Aqua.Core;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public partial class DataContainer
    {
#if DEBUG
        /// <summary>
        /// DEBUG-only self check, run from the static constructor (i.e. on the first entity of the
        /// session). It throws on any mismatch and verifies:
        /// <list type="bullet">
        /// <item>every <see cref="KeyNames"/> constant is in <see cref="AllKeys"/> and every
        /// <see cref="AllKeys"/> entry is a constant, with no duplicates on either side (so a key
        /// missing from one of the two lists is caught);</item>
        /// <item><see cref="KeyBit"/> gives every key a distinct bit inside 0..63;</item>
        /// <item>for <b>every</b> key: a value of the field's own type is accepted by
        /// <see cref="Set"/> without touching <see cref="_spill"/>, comes back out of
        /// <c>Get&lt;object&gt;</c> unchanged, and <see cref="Remove"/> clears
        /// <see cref="Has(string)"/>;</item>
        /// <item>for every key: <c>Set(key, null)</c> makes <see cref="Has(string)"/> true;</item>
        /// <item>for the hand-listed <c>CheckValue</c> keys (all 19 value-typed keys and 7 of the 14
        /// reference-typed ones): <c>Get&lt;T&gt;</c> with the exact T returns the stored value and
        /// the default after Remove, and a correctly typed write never reaches
        /// <see cref="_spill"/>;</item>
        /// <item>the overflow-dictionary rules (unknown key, type mismatch, null into a value-typed
        /// key).</item>
        /// </list>
        /// <para>
        /// Not covered, by design of the above: a <c>case</c> that returns the wrong field of the
        /// <b>same</b> type (e.g. two <c>float</c> keys swapped) -- only field names in the switch
        /// can prove that; <c>Get&lt;T&gt;</c> for a T other than the exact type and <c>object</c>
        /// on the 14 keys outside the <c>CheckValue</c> list (they are exercised through
        /// <c>Get&lt;object&gt;</c> only); and a new key whose field type is not in
        /// <c>BuildProbeValues</c> fails loudly instead of being silently uncovered.
        /// </para>
        /// </summary>
        private static void VerifyKeyTable()
        {
            // The constants are the single source of truth; AllKeys is the table that GetValues()
            // walks. Reflection is the only way to prove the two lists did not drift apart.
            List<string> constantNames = new List<string>();
            foreach (FieldInfo field in typeof(KeyNames).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.IsLiteral && field.FieldType == typeof(string))
                {
                    constantNames.Add((string)field.GetRawConstantValue());
                }
            }

            if (constantNames.Count != AllKeys.Length)
            {
                throw new InvalidOperationException(
                    $"DataContainer: KeyNames declares {constantNames.Count} constants but AllKeys has {AllKeys.Length} entries " +
                    "(every key needs both).");
            }

            HashSet<string> constantSet = new HashSet<string>(constantNames, StringComparer.Ordinal);
            if (constantSet.Count != constantNames.Count)
            {
                throw new InvalidOperationException("DataContainer: two KeyNames constants share the same value.");
            }

            HashSet<string> tableSet = new HashSet<string>(AllKeys, StringComparer.Ordinal);
            if (tableSet.Count != AllKeys.Length)
            {
                throw new InvalidOperationException("DataContainer: AllKeys contains the same key twice.");
            }

            foreach (string name in constantNames)
            {
                if (!tableSet.Contains(name))
                {
                    throw new InvalidOperationException($"DataContainer: KeyNames constant \"{name}\" is missing from AllKeys.");
                }
            }

            foreach (string name in AllKeys)
            {
                if (!constantSet.Contains(name))
                {
                    throw new InvalidOperationException($"DataContainer: AllKeys entry \"{name}\" is not a KeyNames constant.");
                }
            }

            // KeyBit no longer dispatches anything, it only assigns presence bits, so the only way
            // it can be wrong is a duplicated or out-of-range value.
            HashSet<int> bits = new HashSet<int>();
            foreach (FieldInfo field in typeof(KeyBit).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.IsLiteral && field.FieldType == typeof(int))
                {
                    int bit = (int)field.GetRawConstantValue();
                    if (bit < 0 || bit >= 64)
                    {
                        throw new InvalidOperationException($"DataContainer: KeyBit.{field.Name} = {bit} is outside the 64-bit presence mask.");
                    }

                    if (!bits.Add(bit))
                    {
                        throw new InvalidOperationException($"DataContainer: two KeyBit constants share bit {bit}.");
                    }
                }
            }

            if (bits.Count != AllKeys.Length)
            {
                throw new InvalidOperationException(
                    $"DataContainer: {bits.Count} distinct presence bits for {AllKeys.Length} keys (every key needs its own bit).");
            }

            DataContainer data = new DataContainer(0);

            // Representatives first, while _spill is still null: CheckValue asserts that a
            // correctly typed write never reaches the overflow dictionary.
            MethodInfo method = typeof(DataContainer).GetMethod(nameof(VerifyKeyTable), BindingFlags.Static | BindingFlags.NonPublic);
            CheckValue(data, KeyNames.Hookable, true, false);
            CheckValue(data, KeyNames.HookAttached, true, false);
            CheckValue(data, KeyNames.Reversed, true, false);
            CheckValue(data, KeyNames.EeveePatched, true, false);
            CheckValue(data, KeyNames.IsBoosterDash, true, false);
            CheckValue(data, KeyNames.RopeIsLoosen, true, false);
            CheckValue(data, KeyNames.CassetteActive, true, false);
            CheckValue(data, KeyNames.RedirectionShoot, true, false);
            CheckValue(data, KeyNames.UniqueId, 42UL, 0UL);
            CheckValue(data, KeyNames.PrevPosition, new Vector2(1f, 2f), default(Vector2));
            CheckValue(data, KeyNames.RespawnPosition, new Vector2(3f, 4f), default(Vector2));
            CheckValue(data, KeyNames.AccelerateState, AccelerationArea.AccelerateState.Deaccelerate, AccelerationArea.AccelerateState.None);
            CheckValue(data, KeyNames.SlideState, PlayerStates.SlideStates.HighSpeed, PlayerStates.SlideStates.None);
            CheckValue(data, KeyNames.PreviousFacing, -1, 0);
            CheckValue(data, KeyNames.ClimbRopeDirection, 3, 0);
            CheckValue(data, KeyNames.RedDashSwingDir, 1.5f, 0f);
            CheckValue(data, KeyNames.RedDashSwingSpeed, 210f, 0f);
            CheckValue(data, KeyNames.FastBubbleMultiplier, 1.25f, 0f);
            CheckValue(data, KeyNames.LiftSpeedY, -2f, 0f);

            // Reference-typed representatives (identity must survive).
            CheckValue(data, KeyNames.OnAttachCallback, (Action<GrapplingHook>)(_ => { }), null);
            CheckValue(data, KeyNames.OnDetachCallback, (Action<GrapplingHook>)(_ => { }), null);
            CheckValue(data, KeyNames.PostMovePatch, (Action<Vector2>)(_ => { }), null);
            CheckValue(data, KeyNames.CanCollideMethod, method, null);
            CheckValue(data, KeyNames.DashHangingTicker, new TimeTicker(0.05f), null);
            CheckValue(data, KeyNames.BoostSpeedSaveTicker, new TimeTicker(0.2f), null);
            CheckValue(data, KeyNames.ElecShockTicker, new TimeTicker(0.5f), null);

            // Every key: null is always accepted (reference fields clear, value-typed keys fall
            // through to _spill), Has follows Set/Remove.
            foreach (string key in AllKeys)
            {
                data.Set(key, null);
                if (!data.Has(key))
                {
                    throw new InvalidOperationException($"DataContainer: Has(\"{key}\") is false after Set.");
                }

                data.Remove(key);
                if (data.Has(key))
                {
                    throw new InvalidOperationException($"DataContainer: Has(\"{key}\") is true after Remove.");
                }
            }

            // Every key, with a value of its own field type: Set must take the typed path (no
            // _spill), Get<object> must return that value (a missing Get case returns the default,
            // i.e. null), and Has/Remove must follow. Unlike the CheckValue list above this loop is
            // driven by AllKeys alone, so a newly added key is covered without adding a line here --
            // provided its field type is in the probe list (otherwise this throws and says so).
            object[] probes = BuildProbeValues(method);
            foreach (string key in AllKeys)
            {
                object probe = FindAcceptedProbe(data, key, probes);
                if (probe == null)
                {
                    throw new InvalidOperationException(
                        $"DataContainer: no probe value matches the field type of \"{key}\"; add its type to BuildProbeValues.");
                }

                object roundTripped = data.Get<object>(key);
                if (!SameValue(probe, roundTripped))
                {
                    throw new InvalidOperationException(
                        $"DataContainer: Get<object>(\"{key}\") returned " +
                        $"{(roundTripped == null ? "null" : roundTripped.GetType().Name)} instead of the stored " +
                        $"{probe.GetType().Name}; the key probably has no Get case.");
                }

                data.Remove(key);
                if (data.Has(key))
                {
                    throw new InvalidOperationException($"DataContainer: Has(\"{key}\") is true after Remove.");
                }
            }

            // Overflow dictionary semantics: unknown key, type mismatch, null into a value-typed key.
            data.Set("d2_self_check_unknown", 5);
            if (!data.Has("d2_self_check_unknown") || data.Get<int>("d2_self_check_unknown") != 5)
            {
                throw new InvalidOperationException("DataContainer: unknown keys must round-trip through _spill.");
            }

            data.Remove("d2_self_check_unknown");
            if (data.Has("d2_self_check_unknown"))
            {
                throw new InvalidOperationException("DataContainer: Remove(unknown key) must clear Has().");
            }

            data.Set(KeyNames.Hookable, 5);
            if (!data.Has(KeyNames.Hookable) || data.Get<bool>(KeyNames.Hookable))
            {
                throw new InvalidOperationException("DataContainer: a type-mismatched value must fall through to _spill.");
            }

            data.Remove(KeyNames.Hookable);

            data.Set(KeyNames.PrevPosition, null);
            if (!data.Has(KeyNames.PrevPosition) || data.Get<Vector2>(KeyNames.PrevPosition) != default(Vector2))
            {
                throw new InvalidOperationException("DataContainer: a null written into a value-typed key must fall through to _spill.");
            }

            data.Remove(KeyNames.PrevPosition);
        }

        private static void CheckValue<T>(DataContainer data, string key, T value, T defaultValue)
        {
            data.Remove(key);
            data.Set(key, value);

            // A correctly typed value for a predefined key must reach its typed field. If a case is
            // missing (name or constant out of sync), Set silently falls through to the overflow
            // dictionary -- still correct, but it would bring the per-entity dictionary cost back,
            // so reject that here.
            if (data._spill != null && data._spill.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"DataContainer: Set(\"{key}\", {typeof(T).Name}) fell through to _spill; " +
                    "a switch case is probably missing for this key.");
            }

            if (!data.Has(key))
            {
                throw new InvalidOperationException($"DataContainer: Has(\"{key}\") is false after Set(value).");
            }

            if (!EqualityComparer<T>.Default.Equals(data.Get<T>(key), value))
            {
                throw new InvalidOperationException($"DataContainer: Get<{typeof(T).Name}>(\"{key}\") did not return the stored value.");
            }

            data.Remove(key);
            if (data.Has(key))
            {
                throw new InvalidOperationException($"DataContainer: Has(\"{key}\") is true after Remove.");
            }

            if (!EqualityComparer<T>.Default.Equals(data.Get<T>(key), defaultValue))
            {
                throw new InvalidOperationException($"DataContainer: Get<{typeof(T).Name}>(\"{key}\") after Remove must be the default.");
            }
        }

        /// <summary>
        /// One value per field type used by a predefined key. <see cref="FindAcceptedProbe"/> takes
        /// the first probe the key's field accepts, so a key whose <c>Set</c> case is missing ends up
        /// with "no probe matched" instead of passing silently.
        /// </summary>
        private static object[] BuildProbeValues(MethodInfo method)
        {
            return new object[]
            {
                true,                                        // bool keys
                7,                                           // int keys
                1.5f,                                        // float keys
                42UL,                                        // unique_id
                new Vector2(1f, 2f),                         // prev_position / respawn_position
                AccelerationArea.AccelerateState.Deaccelerate,
                PlayerStates.SlideStates.HighSpeed,
                (Action<GrapplingHook>)(_ => { }),           // on_attach_callback / on_detach_callback
                (Action<Vector2>)(_ => { }),                 // post_move_patch
                method,                                      // can_collide_method
                new TimeTicker(0.05f),                       // the three Player tickers
                CreateUninitialized<MoveToward>(),
                CreateUninitialized<Platform>(),
                CreateUninitialized<Coroutine>(),
                CreateUninitialized<SoundSource>(),
                CreateUninitialized<GrapplingHook>(),
                CreateUninitialized<GrappleIndicator>(),
                CreateUninitialized<ShotHookCheck>(),
            };
        }

        /// <summary>
        /// Allocates an instance without running a constructor: the self-check only needs identity,
        /// and constructing a real Entity / GrapplingHook during type initialisation would run game
        /// code.
        /// </summary>
        private static T CreateUninitialized<T>() where T : class
        {
            return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        }

        private static object FindAcceptedProbe(DataContainer data, string key, object[] probes)
        {
            foreach (object probe in probes)
            {
                data.Remove(key);
                data.Set(key, probe);
                if (data._spill == null || !data._spill.ContainsKey(key))
                {
                    // Set took the typed path: this probe has the field's type and is now stored.
                    return probe;
                }
            }

            data.Remove(key);
            return null;
        }

        private static bool SameValue(object expected, object actual)
        {
            if (expected == null || actual == null)
            {
                return expected == null && actual == null;
            }

            if (expected.GetType() != actual.GetType())
            {
                return false;
            }

            return expected.GetType().IsValueType ? expected.Equals(actual) : ReferenceEquals(expected, actual);
        }
#endif
    }
}
