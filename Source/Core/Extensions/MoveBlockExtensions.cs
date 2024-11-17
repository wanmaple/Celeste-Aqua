﻿using System.Collections.Generic;
using System;
using Monocle;
using Microsoft.Xna.Framework;
using static Celeste.MoveBlock;
using FMOD.Studio;

namespace Celeste.Mod.Aqua.Core
{
    public static class MoveBlockExtensions
    {
        public static void Initialize()
        {
            On.Celeste.MoveBlock.Controller += MoveBlock_Controller;
        }

        public static void Uninitialize()
        {
            On.Celeste.MoveBlock.Controller -= MoveBlock_Controller;
        }

        private static System.Collections.IEnumerator MoveBlock_Controller(On.Celeste.MoveBlock.orig_Controller orig, MoveBlock self)
        {
            while (true)
            {
                self.triggered = false;
                self.state = MovementState.Idling;
                while (!self.triggered && !self.HasPlayerRider() && !self.IsHookAttached())
                {
                    yield return null;
                }

                Audio.Play("event:/game/04_cliffside/arrowblock_activate", self.Position);
                self.state = MovementState.Moving;
                self.StartShaking(0.2f);
                self.ActivateParticles();
                yield return 0.2f;
                self.targetSpeed = (self.fast ? 75f : 60f);
                self.moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
                self.moveSfx.Param("arrow_stop", 0f);
                self.StopPlayerRunIntoAnimation = false;
                float crashTimer = 0.15f;
                float crashResetTimer = 0.1f;
                float noSteerTimer = 0.2f;
                while (true)
                {
                    if (self.canSteer)
                    {
                        self.targetAngle = self.homeAngle;
                        bool flag = ((self.direction != Directions.Right && self.direction != 0) ? self.HasPlayerClimbing() : self.HasPlayerOnTop());
                        if (flag && noSteerTimer > 0f)
                        {
                            noSteerTimer -= Engine.DeltaTime;
                        }

                        if (flag)
                        {
                            if (noSteerTimer <= 0f)
                            {
                                if (self.direction == Directions.Right || self.direction == Directions.Left)
                                {
                                    self.targetAngle = self.homeAngle + MathF.PI / 4f * (float)self.angleSteerSign * (float)Input.MoveY.Value;
                                }
                                else
                                {
                                    self.targetAngle = self.homeAngle + MathF.PI / 4f * (float)self.angleSteerSign * (float)Input.MoveX.Value;
                                }
                            }
                        }
                        else
                        {
                            noSteerTimer = 0.2f;
                        }
                    }
                    if (self.Scene.OnInterval(0.02f))
                    {
                        self.MoveParticles();
                    }

                    self.speed = Calc.Approach(self.speed, self.targetSpeed, 300f * Engine.DeltaTime);
                    self.angle = Calc.Approach(self.angle, self.targetAngle, MathF.PI * 16f * Engine.DeltaTime);
                    Vector2 vector = Calc.AngleToVector(self.angle, self.speed);
                    Vector2 vec = vector * Engine.DeltaTime;
                    bool flag2;
                    if (self.direction == Directions.Right || self.direction == Directions.Left)
                    {
                        flag2 = self.MoveCheck(vec.XComp());
                        self.noSquish = self.Scene.Tracker.GetEntity<Player>();
                        self.MoveVCollideSolids(vec.Y, thruDashBlocks: false);
                        self.noSquish = null;
                        self.LiftSpeed = vector;
                        if (self.Scene.OnInterval(0.03f))
                        {
                            if (vec.Y > 0f)
                            {
                                self.ScrapeParticles(Vector2.UnitY);
                            }
                            else if (vec.Y < 0f)
                            {
                                self.ScrapeParticles(-Vector2.UnitY);
                            }
                        }
                    }
                    else
                    {
                        flag2 = self.MoveCheck(vec.YComp());
                        self.noSquish = self.Scene.Tracker.GetEntity<Player>();
                        self.MoveHCollideSolids(vec.X, thruDashBlocks: false);
                        self.noSquish = null;
                        self.LiftSpeed = vector;
                        if (self.Scene.OnInterval(0.03f))
                        {
                            if (vec.X > 0f)
                            {
                                self.ScrapeParticles(Vector2.UnitX);
                            }
                            else if (vec.X < 0f)
                            {
                                self.ScrapeParticles(-Vector2.UnitX);
                            }
                        }

                        if (self.direction == Directions.Down && self.Top > (float)(self.SceneAs<Level>().Bounds.Bottom + 32))
                        {
                            flag2 = true;
                        }
                    }

                    if (flag2)
                    {
                        self.moveSfx.Param("arrow_stop", 1f);
                        crashResetTimer = 0.1f;
                        if (!(crashTimer > 0f))
                        {
                            break;
                        }

                        crashTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        self.moveSfx.Param("arrow_stop", 0f);
                        if (crashResetTimer > 0f)
                        {
                            crashResetTimer -= Engine.DeltaTime;
                        }
                        else
                        {
                            crashTimer = 0.15f;
                        }
                    }

                    Level level = self.Scene as Level;
                    if (self.Left < (float)level.Bounds.Left || self.Top < (float)level.Bounds.Top || self.Right > (float)level.Bounds.Right)
                    {
                        break;
                    }

                    yield return null;
                }

                Audio.Play("event:/game/04_cliffside/arrowblock_break", self.Position);
                self.moveSfx.Stop();
                self.state = MovementState.Breaking;
                self.speed = (self.targetSpeed = 0f);
                self.angle = (self.targetAngle = self.homeAngle);
                self.StartShaking(0.2f);
                self.StopPlayerRunIntoAnimation = true;
                yield return 0.2f;
                self.BreakParticles();
                List<MoveBlock.Debris> debris = new List<MoveBlock.Debris>();
                for (int i = 0; (float)i < self.Width; i += 8)
                {
                    for (int j = 0; (float)j < self.Height; j += 8)
                    {
                        Vector2 vector2 = new Vector2((float)i + 4f, (float)j + 4f);
                        MoveBlock.Debris debris2 = Engine.Pooler.Create<MoveBlock.Debris>().Init(self.Position + vector2, self.Center, self.startPosition + vector2);
                        debris.Add(debris2);
                        self.Scene.Add(debris2);
                    }
                }

                MoveBlock moveBlock = self;
                Vector2 amount = self.startPosition - self.Position;
                self.DisableStaticMovers();
                moveBlock.MoveStaticMovers(amount);
                self.Position = self.startPosition;
                self.Visible = (self.Collidable = false);
                yield return 2.2f;
                foreach (MoveBlock.Debris item in debris)
                {
                    item.StopMoving();
                }

                while (self.CollideCheck<Actor>() || self.CollideCheck<Solid>())
                {
                    yield return null;
                }

                self.Collidable = true;
                EventInstance instance = Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", debris[0].Position);
                MoveBlock moveBlock2 = self;
                Coroutine component;
                Coroutine routine = (component = new Coroutine(self.SoundFollowsDebrisCenter(instance, debris)));
                moveBlock2.Add(component);
                foreach (MoveBlock.Debris item2 in debris)
                {
                    item2.StartShaking();
                }

                yield return 0.2f;
                foreach (MoveBlock.Debris item3 in debris)
                {
                    item3.ReturnHome(0.65f);
                }

                yield return 0.6f;
                routine.RemoveSelf();
                foreach (MoveBlock.Debris item4 in debris)
                {
                    item4.RemoveSelf();
                }

                Audio.Play("event:/game/04_cliffside/arrowblock_reappear", self.Position);
                self.Visible = true;
                self.EnableStaticMovers();
                self.speed = (self.targetSpeed = 0f);
                self.angle = (self.targetAngle = self.homeAngle);
                self.noSquish = null;
                self.fillColor = idleBgFill;
                self.UpdateColors();
                self.flash = 1f;
            }
        }
    }
}