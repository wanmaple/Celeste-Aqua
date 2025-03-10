using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using static Celeste.GaussianBlur;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Falling Grapple Magnet")]
    public class FallingGrappleMagnet : GrappleMagnet
    {
        public float FallDelay { get; private set; }
        public string FallFlag { get; private set; }
        public bool GrappleTrigger { get; private set; }
        public bool FlagTrigger { get; private set; }

        public FallingGrappleMagnet(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            FallDelay = data.Float("fall_delay", 0.2f);
            FallFlag = data.Attr("fall_flag", string.Empty);
            GrappleTrigger = data.Bool("grapple_trigger", true);
            FlagTrigger = data.Bool("flag_trigger", false);
            Add(new Coroutine(Sequence()));
            _hitCollider = new Hitbox(16.0f, 16.0f, -8.0f, -8.0f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            StartShaking(0.0f);
        }

        private IEnumerator Sequence()
        {
            while (!(GrappleTrigger && this.IsHookAttached()) && !(FlagTrigger && SceneAs<Level>().Session.GetFlag(FallFlag)))
            {
                yield return null;
            }
            if (FlagTrigger)
            {
                Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => SceneAs<Level>().Session.SetFlag(FallFlag, false), 0.0f, true));
            }

            Audio.Play("event:/game/general/fallblock_shake", Center);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            yield return FallDelay;
            StopShaking();
            float speed = 0.0f;
            float maxSpeed = 160.0f;
            float dt = Engine.DeltaTime;
            bool breaking = false;
            while (true)
            {
                if (Scene.OnInterval(0.02f))
                {
                    EmitParticles(Vector2.UnitY);
                }
                _collideSolid = false;
                speed = Calc.Approach(speed, maxSpeed, 500.0f * dt);
                MoveV(speed * dt, SolidsCollideCheck);
                if (_collideSolid)
                {
                    breaking = true;
                    break;
                }
                Level level = SceneAs<Level>();
                if (Top > level.Bounds.Bottom)
                {
                    break;
                }
                yield return null;
            }

            if (breaking)
            {
                Audio.Play("event:/game/04_cliffside/arrowblock_break", Position);
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
                        CustomDebris debris = Engine.Pooler.Create<CustomDebris>().Init("debris/magnet_debris", Position + offset + vec, Center, Vector2.Zero, index + randomSeq);
                        debrisList.Add(debris);
                        Scene.Add(debris);
                        ++index;
                    }
                }
                Visible = false;
                this.SetHookable(false);
                yield return 2.2f;
                foreach (CustomDebris deb in debrisList)
                {
                    deb.RemoveSelf();
                }
            }
            RemoveSelf();
        }

        private bool SolidsCollideCheck(Vector2 movement)
        {
            Entity collided = null;
            Collider old = Collider;
            Collider = _hitCollider;
            bool ret = false;
            if (this.CheckCollidePlatformsAtYDirection(movement.Y, out collided))
            {
                _collideSolid = true;
                ret = true;
            }
            Collider = old;
            return ret;
        }

        private Collider _hitCollider;
        private bool _collideSolid;
    }
}
