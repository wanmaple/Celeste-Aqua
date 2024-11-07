using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Moving Solid")]
    [Tracked(true)]
    public class MovingSolid : Solid
    {
        public float MovingSpeed { get; private set; }

        public MovingSolid(Vector2 position, Vector2 size, float speed, float duration)
            : base(position, size.X, size.Y, false)
        {
            MovingSpeed = speed;

            Tween tween = Tween.Create(Tween.TweenMode.Looping, Ease.BackInOut, duration);
            tween.OnStart = OnMovingStart;
            tween.OnUpdate = OnMovingUpdate;
            Add(tween);
        }

        public MovingSolid(EntityData data, Vector2 offset)
            : this(data.Position + offset, new Vector2(data.Width, data.Height), data.Float("speed"), data.Float("duration"))
        { }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Tween tween = Get<Tween>();
            tween.Start();
        }

        public override void Update()
        {
            base.Update();
        }

        private void OnMovingStart(Tween tween)
        {
            Speed.X = -MovingSpeed;
        }

        private void OnMovingUpdate(Tween tween)
        {
            if (tween.Percent <= 0.5f)
            {
                Speed.X = MathHelper.Lerp(-MovingSpeed, MovingSpeed, tween.Percent / 0.5f);
            }
            else
            {
                Speed.X = MathHelper.Lerp(MovingSpeed, -MovingSpeed, (tween.Percent - 0.5f) / 0.5f);
            }
        }

        public override void Render()
        {
            base.Render();
            Draw.Rect(Collider, new Color(0.5f, 0.5f, 0.5f, 0.7f));

        }

        private Tween _tween;
    }
}
