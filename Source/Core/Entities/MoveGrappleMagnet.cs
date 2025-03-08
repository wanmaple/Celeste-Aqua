using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Move Grapple Magnet")]
    public class MoveGrappleMagnet : GrappleMagnet
    {
        public MoveBlock.Directions Direction { get; private set; }
        public float Acceleration { get; private set; }
        public float MoveSpeed { get; private set; }
        public bool OneUse { get; private set; }
        public string MoveFlag { get; private set; }
        public bool UseFlagToMove { get; private set; }
        public string ArrowTexture { get; private set; }

        public float TargetSpeed { get; set; }

        public MoveGrappleMagnet(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            string direction = data.Attr("direction", "Right");
            switch (direction)
            {
                case "Up":
                    Direction = MoveBlock.Directions.Up;
                    break;
                case "Down":
                    Direction = MoveBlock.Directions.Down;
                    break;
                case "Left":
                    Direction = MoveBlock.Directions.Left;
                    break;
                default:
                    Direction = MoveBlock.Directions.Right;
                    break;
            }
            Acceleration = data.Float("acceleration", 300.0f);
            MoveSpeed = data.Float("speed", 60.0f);
            MoveFlag = data.Attr("move_flag", string.Empty);
            UseFlagToMove = data.Bool("use_flag_to_move", false);
            Add(new Coroutine(Sequence()));
        }

        private Vector2 GetMoveDirection()
        {
            switch (Direction)
            {
                case MoveBlock.Directions.Up:
                    return -Vector2.UnitY;
                case MoveBlock.Directions.Down:
                    return Vector2.UnitY;
                case MoveBlock.Directions.Left:
                    return -Vector2.UnitX;
                default:
                    return Vector2.UnitX;
            }
        }

        private IEnumerator Sequence()
        {
            while (true)
            {
                while ((!UseFlagToMove && !this.IsHookAttached()) || (UseFlagToMove && !SceneAs<Level>().Session.GetFlag(MoveFlag)))
                {
                    yield return null;
                }

                Audio.Play("event:/game/04_cliffside/arrowblock_activate", Position);
                yield return 0.2f;
                TargetSpeed = MoveSpeed;
                _moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
                float speed = 0.0f;
                while (true)
                {
                    if (Scene.OnInterval(0.02f))
                    {
                        EmitParticles(GetMoveDirection());
                    }
                    speed = Calc.Approach(speed, TargetSpeed, Acceleration * Engine.DeltaTime);
                    Vector2 movement = GetMoveDirection() * speed * Engine.DeltaTime;
                    MoveH(movement.X, SolidsCollideCheck);
                    MoveV(movement.Y, SolidsCollideCheck);
                    Level level = Scene as Level;
                    if (Left < level.Bounds.Left || Top < level.Bounds.Top || Right > level.Bounds.Right)
                    {
                        break;
                    }
                    yield return null;
                }

                Audio.Play("event:/game/04_cliffside/arrowblock_break", Position);
                _moveSfx.Stop();
                //state = MovementState.Breaking;
                //speed = (targetSpeed = 0f);
                //angle = (targetAngle = homeAngle);
                //StartShaking(0.2f);
                //StopPlayerRunIntoAnimation = true;
                //yield return 0.2f;
                //BreakParticles();
                //List<Debris> debris = new List<Debris>();
                //for (int i = 0; (float)i < Width; i += 8)
                //{
                //    for (int j = 0; (float)j < Height; j += 8)
                //    {
                //        Vector2 vector2 = new Vector2((float)i + 4f, (float)j + 4f);
                //        Debris debris2 = Engine.Pooler.Create<Debris>().Init(Position + vector2, Center, startPosition + vector2);
                //        debris.Add(debris2);
                //        Scene.Add(debris2);
                //    }
                //}

                //MoveBlock moveBlock = this;
                //Vector2 amount = startPosition - Position;
                //DisableStaticMovers();
                //moveBlock.MoveStaticMovers(amount);
                //Position = startPosition;
                //MoveBlock moveBlock2 = this;
                //MoveBlock moveBlock3 = this;
                //bool visible = false;
                //moveBlock3.Collidable = false;
                //moveBlock2.Visible = visible;
                //yield return 2.2f;
                //foreach (Debris item in debris)
                //{
                //    item.StopMoving();
                //}

                //while (CollideCheck<Actor>() || CollideCheck<Solid>())
                //{
                //    yield return null;
                //}

                //Collidable = true;
                //EventInstance instance = Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", debris[0].Position);
                //MoveBlock moveBlock4 = this;
                //Coroutine component;
                //Coroutine routine = (component = new Coroutine(SoundFollowsDebrisCenter(instance, debris)));
                //moveBlock4.Add(component);
                //foreach (Debris item2 in debris)
                //{
                //    item2.StartShaking();
                //}

                //yield return 0.2f;
                //foreach (Debris item3 in debris)
                //{
                //    item3.ReturnHome(0.65f);
                //}

                //yield return 0.6f;
                //routine.RemoveSelf();
                //foreach (Debris item4 in debris)
                //{
                //    item4.RemoveSelf();
                //}

                //Audio.Play("event:/game/04_cliffside/arrowblock_reappear", Position);
                //Visible = true;
                //EnableStaticMovers();
                //speed = (targetSpeed = 0f);
                //angle = (targetAngle = homeAngle);
                //noSquish = null;
                //fillColor = idleBgFill;
                //UpdateColors();
                //flash = 1f;
            }
        }

        private bool SolidsCollideCheck(Vector2 movement)
        {
            Entity collided = null;
            float extrusionError = 2.0f;
            float left = Center.X - 8.0f;
            float right = Center.X + 8.0f;
            float top = Center.Y - 8.0f;
            float bottom = Center.Y + 8.0f;
            if (this.CheckCollidePlatformsAtXDirection(movement.X, out collided))
            {
                float error = 0.0f;
                if ((error = collided.Bottom - top) <= extrusionError || (error = collided.Top - bottom) >= -extrusionError)
                {
                    Position.Y += error;
                }
                else
                {
                    return true;
                }
            }
            if (this.CheckCollidePlatformsAtYDirection(movement.Y, out collided))
            {
                float error = 0.0f;
                if ((error = collided.Right - left) <= extrusionError || (error = collided.Left - right) >= -extrusionError)
                {
                    Position.X += error;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        private SoundSource _moveSfx;
    }
}
