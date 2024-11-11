using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Reflection;
using static Celeste.BounceBlock;

namespace Celeste.Mod.Aqua.Core
{
    public static class BounceBlockExtensions
    {
        static BounceBlockExtensions()
        {
            _solidUpdateMethod = typeof(Solid).GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            //_matchMethod = typeof(Entity).GetGenericMethod("CollideCheck", Array.Empty<Type>()).MakeGenericMethod(new Type[] { typeof(Solid), });
            //_checkHookMethod = typeof(Entity).GetGenericMethod("CollideCheck", Array.Empty<Type>()).MakeGenericMethod(new Type[] { typeof(GrapplingHook), });
            //_checkRopeMethod = typeof(EntityExtensions).GetMethod("IntersectsWithRope", BindingFlags.Public | BindingFlags.Static);
            //_hookAttachedMethod = typeof(EntityExtensions).GetMethod("IsHookAttached", BindingFlags.Public | BindingFlags.Static);
            //_bounceDirField = typeof(BounceBlock).GetField("bounceDir", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static void Initialize()
        {
            //IL.Celeste.BounceBlock.Update += BounceBlock_ILUpdate;
            On.Celeste.BounceBlock.Update += BounceBlock_Update;
        }

        public static void Uninitialize()
        {
            //IL.Celeste.BounceBlock.Update -= BounceBlock_ILUpdate;
            On.Celeste.BounceBlock.Update -= BounceBlock_Update;
        }

        //private static void BounceBlock_ILUpdate(MonoMod.Cil.ILContext il)
        //{
        //    AquaDebugger.LogInfo("{0} {1} {2} {3}", _matchMethod, _checkHookMethod, _checkRopeMethod, _bounceDirField);
        //    ILCursor cursor = new ILCursor(il);
        //    ILLabel label = null;

        //    if (cursor.TryGotoNext(ins => ins.MatchLdloc2(), ins => ins.MatchBrfalse(out label)))
        //    {
        //        cursor.Index++;
        //        cursor.EmitLdarg0();
        //        cursor.EmitCall(_hookAttachedMethod);
        //        cursor.EmitOr();
        //    }

        //    if (cursor.TryGotoNext(ins => ins.MatchBr(out label)))
        //    {
        //        AquaDebugger.LogInfo("ILHook Success {0}", label.Target);
        //        cursor.Index++;
        //        cursor.EmitDelegate(CalculateBounceDirection);
        //        cursor.EmitBr(label);
        //    }

        //    if (cursor.TryGotoNext(ins => ins.MatchLdarg0(), ins => ins.MatchCall(_matchMethod), ins => ins.MatchBrtrue(out label)))
        //    {
        //        cursor.EmitLdarg0();
        //        cursor.EmitCall(_checkHookMethod);
        //        cursor.EmitBrtrue(label);

        //        cursor.EmitLdarg0();
        //        cursor.EmitCall(_checkRopeMethod);
        //        cursor.EmitBrtrue(label);
        //    }
        //}

        private static void BounceBlock_Update(On.Celeste.BounceBlock.orig_Update orig, BounceBlock self)
        {
            var funcPtr = _solidUpdateMethod.MethodHandle.GetFunctionPointer();
            Action func = Activator.CreateInstance(typeof(Action), self, funcPtr) as Action;
            func();
            self.reappearFlash = Calc.Approach(self.reappearFlash, 0f, Engine.DeltaTime * 8f);
            if (self.state == States.Waiting)
            {
                self.CheckModeChange();
                self.moveSpeed = Calc.Approach(self.moveSpeed, 100f, 400f * Engine.DeltaTime);
                Vector2 vector = Calc.Approach(self.ExactPosition, self.startPos, self.moveSpeed * Engine.DeltaTime);
                Vector2 liftSpeed = (vector - self.ExactPosition).SafeNormalize(self.moveSpeed);
                liftSpeed.X *= 0.75f;
                self.MoveTo(vector, liftSpeed);
                self.windUpProgress = Calc.Approach(self.windUpProgress, 0f, 1f * Engine.DeltaTime);
                Player player = self.WindUpPlayerCheck();
                if (player != null || self.IsHookAttached())
                {
                    self.moveSpeed = 80f;
                    self.windUpStartTimer = 0f;
                    if (self.iceMode)
                    {
                        self.bounceDir = -Vector2.UnitY;
                    }
                    else
                    {
                        GrapplingHook hook = self.Scene.Tracker.GetEntity<GrapplingHook>();
                        Vector2 pt = player != null ? player.Center : hook.Position;
                        self.bounceDir = (pt - self.Center).SafeNormalize();
                    }

                    self.state = States.WindingUp;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    if (self.iceMode)
                    {
                        self.StartShaking(0.2f);
                        Audio.Play("event:/game/09_core/iceblock_touch", self.Center);
                    }
                    else
                    {
                        Audio.Play("event:/game/09_core/bounceblock_touch", self.Center);
                    }
                }
            }
            else if (self.state == States.WindingUp)
            {
                Player player2 = self.WindUpPlayerCheck();
                if (player2 != null)
                {
                    if (self.iceMode)
                    {
                        self.bounceDir = -Vector2.UnitY;
                    }
                    else
                    {
                        self.bounceDir = (player2.Center - self.Center).SafeNormalize();
                    }
                }

                if (self.windUpStartTimer > 0f)
                {
                    self.windUpStartTimer -= Engine.DeltaTime;
                    self.windUpProgress = Calc.Approach(self.windUpProgress, 0f, 1f * Engine.DeltaTime);
                    return;
                }

                self.moveSpeed = Calc.Approach(self.moveSpeed, self.iceMode ? 35f : 40f, 600f * Engine.DeltaTime);
                float num = (self.iceMode ? 0.333f : 1f);
                Vector2 vector2 = self.startPos - self.bounceDir * (self.iceMode ? 16f : 10f);
                Vector2 vector3 = Calc.Approach(self.ExactPosition, vector2, self.moveSpeed * num * Engine.DeltaTime);
                Vector2 liftSpeed2 = (vector3 - self.ExactPosition).SafeNormalize(self.moveSpeed * num);
                liftSpeed2.X *= 0.75f;
                self.MoveTo(vector3, liftSpeed2);
                self.windUpProgress = Calc.ClampedMap(Vector2.Distance(self.ExactPosition, vector2), 16f, 2f);
                if (self.iceMode && Vector2.DistanceSquared(self.ExactPosition, vector2) <= 12f)
                {
                    self.StartShaking(0.1f);
                }
                else if (!self.iceMode && self.windUpProgress >= 0.5f)
                {
                    self.StartShaking(0.1f);
                }

                if (Vector2.DistanceSquared(self.ExactPosition, vector2) <= 2f)
                {
                    if (self.iceMode)
                    {
                        self.Break();
                    }
                    else
                    {
                        self.state = States.Bouncing;
                    }

                    self.moveSpeed = 0f;
                }
            }
            else if (self.state == States.Bouncing)
            {
                self.moveSpeed = Calc.Approach(self.moveSpeed, 140f, 800f * Engine.DeltaTime);
                Vector2 vector4 = self.startPos + self.bounceDir * 24f;
                Vector2 vector5 = Calc.Approach(self.ExactPosition, vector4, self.moveSpeed * Engine.DeltaTime);
                self.bounceLift = (vector5 - self.ExactPosition).SafeNormalize(Math.Min(self.moveSpeed * 3f, 200f));
                self.bounceLift.X *= 0.75f;
                self.MoveTo(vector5, self.bounceLift);
                self.windUpProgress = 1f;
                if (self.ExactPosition == vector4 || (!self.iceMode && self.WindUpPlayerCheck() == null && !self.IsHookAttached()))
                {
                    self.debrisDirection = (vector4 - self.startPos).SafeNormalize();
                    self.state = States.BounceEnd;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    self.moveSpeed = 0f;
                    self.bounceEndTimer = 0.05f;
                    self.ShakeOffPlayer(self.bounceLift);
                }
            }
            else if (self.state == States.BounceEnd)
            {
                self.bounceEndTimer -= Engine.DeltaTime;
                if (self.bounceEndTimer <= 0f)
                {
                    self.Break();
                }
            }
            else
            {
                if (self.state != States.Broken)
                {
                    return;
                }

                self.Depth = 8990;
                self.reformed = false;
                if (self.respawnTimer > 0f)
                {
                    self.respawnTimer -= Engine.DeltaTime;
                    return;
                }

                Vector2 position = self.Position;
                self.Position = self.startPos;
                if (!self.CollideCheck<Actor>() && !self.CollideCheck<Solid>() && !self.CollideCheck<GrapplingHook>() && !self.IntersectsWithRope())
                {
                    self.CheckModeChange();
                    Audio.Play(self.iceMode ? "event:/game/09_core/iceblock_reappear" : "event:/game/09_core/bounceblock_reappear", self.Center);
                    float duration = 0.35f;
                    for (int i = 0; (float)i < self.Width; i += 8)
                    {
                        for (int j = 0; (float)j < self.Height; j += 8)
                        {
                            Vector2 vector6 = new Vector2(self.X + (float)i + 4f, self.Y + (float)j + 4f);
                            self.Scene.Add(Engine.Pooler.Create<RespawnDebris>().Init(vector6 + (vector6 - self.Center).SafeNormalize() * 12f, vector6, self.iceMode, duration));
                        }
                    }

                    Alarm.Set(self, duration, delegate
                    {
                        self.reformed = true;
                        self.reappearFlash = 0.6f;
                        self.EnableStaticMovers();
                        self.ReformParticles();
                    });
                    self.Depth = -9000;
                    self.MoveStaticMovers(self.Position - position);
                    self.Collidable = true;
                    self.state = States.Waiting;
                }
                else
                {
                    self.Position = position;
                }
            }
        }

        private static MethodInfo _solidUpdateMethod;
        //private static Methodself _matchMethod;
        //private static Methodself _checkHookMethod;
        //private static Methodself _checkRopeMethod;
        //private static Methodself _hookAttachedMethod;
        //private static FieldInfo _bounceDirField;
    }
}
