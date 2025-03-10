using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using FMOD.Studio;
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
        public bool GrappleTrigger { get; private set; }
        public bool FlagTrigger { get; private set; }

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
            MoveSpeed = data.Float("move_speed", 60.0f);
            OneUse = data.Bool("one_use", false);
            MoveFlag = data.Attr("move_flag", string.Empty);
            GrappleTrigger = data.Bool("grapple_trigger", true);
            FlagTrigger = data.Bool("flag_trigger", false);
            _startPosition = Position;
            _hitCollider = new Hitbox(16.0f, 16.0f, -8.0f, -8.0f);
            Add(new Coroutine(Sequence()));
            Add(new AccelerationAreaInOut(OnKeepInAccelerationArea, null, OnExitAccelerationArea));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(_imgFlash = new Image(GFX.Game["objects/hook_magnet/recover_flash"]));
            _imgFlash.JustifyOrigin(0.5f, 0.5f);
        }

        public override void Update()
        {
            base.Update();
            _flash = Calc.Approach(_flash, 0.0f, Engine.DeltaTime * 5.0f);
            _imgFlash.SetColor(Color.White * _flash);
        }

        protected override string GetDefaultSpriteName()
        {
            switch (Direction)
            {
                case MoveBlock.Directions.Up:
                    return "Aqua_MoveGrappleMagnetUp";
                case MoveBlock.Directions.Down:
                    return "Aqua_MoveGrappleMagnetDown";
                case MoveBlock.Directions.Left:
                    return "Aqua_MoveGrappleMagnetLeft";
                default:
                    return "Aqua_MoveGrappleMagnetRight";
            }
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

        private void OnExitAccelerationArea(AccelerationArea area)
        {
            this.SetAccelerateState(AccelerationArea.AccelerateState.None);
        }

        private void OnKeepInAccelerationArea(AccelerationArea area)
        {
            int oldSign = MathF.Sign(TargetSpeed);
            this.SetAccelerateState(area.TryAccelerate(this));
            int newSign = MathF.Sign(TargetSpeed);
            if (oldSign != newSign)
            {
                this.SetReversed(!this.IsReversed());
            }
        }

        private IEnumerator Sequence()
        {
            while (true)
            {
                while (!(GrappleTrigger && this.IsHookAttached()) && !(FlagTrigger && SceneAs<Level>().Session.GetFlag(MoveFlag)))
                {
                    yield return null;
                }
                if (FlagTrigger)
                {
                    Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => SceneAs<Level>().Session.SetFlag(MoveFlag, false), 0.0f, true));
                }

                StartShaking(0.2f);
                Audio.Play("event:/game/04_cliffside/arrowblock_activate", Position);
                yield return 0.2f;
                TargetSpeed = MoveSpeed;
                _moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
                _moveSfx.Param("arrow_stop", 0.0f);
                float speed = 0.0f;
                TimeTicker crashTicker = new TimeTicker(0.15f);
                float dt = Engine.DeltaTime;
                while (true)
                {
                    if (Scene.OnInterval(0.02f))
                    {
                        EmitParticles(GetMoveDirection() * (this.IsReversed() ? -1.0f : 1.0f));
                    }
                    _moveSfx.Position = Position;
                    speed = Calc.Approach(speed, TargetSpeed, Acceleration * dt);
                    Vector2 movement = GetMoveDirection() * speed * dt;
                    _collideSolid = false;
                    MoveH(movement.X, SolidsCollideCheck);
                    MoveV(movement.Y, SolidsCollideCheck);
                    if (_collideSolid)
                    {
                        _moveSfx.Param("arrow_stop", 1.0f);
                        crashTicker.Tick(dt);
                        if (crashTicker.Check())
                            break;
                    }
                    else
                    {
                        _moveSfx.Param("arrow_stop", 0.0f);
                        crashTicker.Reset();
                    }
                    Level level = Scene as Level;
                    float left = Position.X - 8.0f;
                    float right = Position.X + 8.0f;
                    float top = Position.Y - 8.0f;
                    if (left < level.Bounds.Left || top < level.Bounds.Top || right > level.Bounds.Right)
                    {
                        break;
                    }
                    yield return null;
                }

                Audio.Play("event:/game/04_cliffside/arrowblock_break", Position);
                _moveSfx.Stop();
                StartShaking(0.2f);
                yield return 0.2f;
                Vector2 offset = new Vector2(-8.0f, -8.0f);
                List<CustomDebris> debrisList = new List<CustomDebris>();
                int index = 0;
                int randomSeq = Calc.Random.Next(0, 4);
                for (int i = 0; i < 16.0f; i += 8)
                {
                    for (int j = 0; j < 16.0f; j += 8)
                    {
                        Vector2 vec = new Vector2((float)i + 4.0f, (float)j + 4.0f);
                        CustomDebris debris = Engine.Pooler.Create<CustomDebris>().Init("debris/magnet_debris", Position + offset + vec, Center, _startPosition + offset + vec, index + randomSeq);
                        debrisList.Add(debris);
                        Scene.Add(debris);
                        ++index;
                    }
                }
                Position = _startPosition;
                Visible = false;
                this.SetHookable(false);
                yield return 2.2f;
                foreach (CustomDebris deb in debrisList)
                {
                    deb.StopMoving();
                }
                Collider old = Collider;
                Collider = _hitCollider;
                while (CollideCheck<Solid>())
                {
                    yield return null;
                }
                Collider = old;
                EventInstance instance = Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", debrisList[0].Position);
                Coroutine routine = new Coroutine(SoundFollowsDebrisCenter(instance, debrisList));
                Add(routine);
                foreach (CustomDebris deb in debrisList)
                {
                    deb.StartShaking();
                }
                yield return 0.2f;
                if (!OneUse)
                {
                    foreach (CustomDebris deb in debrisList)
                    {
                        deb.ReturnHome(0.65f);
                    }
                    yield return 0.6f;
                }
                routine.RemoveSelf();
                foreach (CustomDebris deb in debrisList)
                {
                    deb.RemoveSelf();
                }
                if (OneUse)
                {
                    RemoveSelf();
                    yield break;
                }
                Audio.Play("event:/game/04_cliffside/arrowblock_reappear", Position);
                this.SetHookable(true);
                Visible = true;
                _flash = 1.0f;
            }
        }

        private IEnumerator SoundFollowsDebrisCenter(EventInstance instance, List<CustomDebris> debrisList)
        {
            while (true)
            {
                instance.getPlaybackState(out var playbackState);
                if (playbackState == PLAYBACK_STATE.STOPPED)
                {
                    break;
                }

                Vector2 zero = Vector2.Zero;
                foreach (CustomDebris debri in debrisList)
                {
                    zero += debri.Position;
                }

                zero /= (float)debrisList.Count;
                Audio.Position(instance, zero);
                yield return null;
            }
        }

        private bool SolidsCollideCheck(Vector2 movement)
        {
            Entity collided = null;
            Collider old = Collider;
            Collider = _hitCollider;
            float extrusionError = 3.0f;
            float left = Collider.AbsoluteLeft;
            float right = Collider.AbsoluteRight;
            float top = Collider.AbsoluteTop;
            float bottom = Collider.AbsoluteBottom;
            bool ret = false;
            if ((Direction == MoveBlock.Directions.Left || Direction == MoveBlock.Directions.Right) && this.CheckCollidePlatformsAtXDirection(movement.X, out collided))
            {
                float error = 0.0f;
                if ((error = collided.Bottom - top) <= extrusionError || (error = collided.Top - bottom) >= -extrusionError)
                {
                    Position.Y += error;
                }
                else
                {
                    _collideSolid = true;
                    ret = true;
                }
            }
            if ((Direction == MoveBlock.Directions.Up || Direction == MoveBlock.Directions.Down) && this.CheckCollidePlatformsAtYDirection(movement.Y, out collided))
            {
                float error = 0.0f;
                if ((error = collided.Right - left) <= extrusionError || (error = collided.Left - right) >= -extrusionError)
                {
                    Position.X += error;
                }
                else
                {
                    _collideSolid = true;
                    ret = true;
                }
            }
            Collider = old;
            return ret;
        }

        private Vector2 _startPosition;
        private Collider _hitCollider;
        private bool _collideSolid;
        private float _flash;
        private Image _imgFlash;
        private SoundSource _moveSfx = new SoundSource();
    }
}
