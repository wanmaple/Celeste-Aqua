using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.Aqua.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Rod Move Block")]
    [Tracked(false)]
    public class RodMoveBlock : AquaMoveBlock, IRodControllable, ICustomRenderEntity
    {
        public string Flag { get; private set; }
        public bool IsRunning { get; private set; }
        public float HueOffset { get; private set; }
        public float SaturationOffset { get; private set; }

        public RodMoveBlock(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Flag = data.Attr("flag");
            HueOffset = data.Float("hue_offset");
            SaturationOffset = data.Float("saturation_offset");
            Coroutine coroutine = Get<Coroutine>();
            Remove(coroutine);
            Add(new Coroutine(RodController()));
            _fx = FXCenter.Instance.GetFX("hue_offset");
            if (_fx != null)
            {
                _fx.Parameters["HueOffset"].SetValue(HueOffset);
                _fx.Parameters["SaturationOffset"].SetValue(SaturationOffset);
            }
            AddTag(RenderTags.CustomEntity);
        }

        public Effect GetEffect()
        {
            return _fx;
        }

        public void OnReload()
        {
            _fx = FXCenter.Instance.GetFX("hue_offset");
            if (_fx != null)
            {
                _fx.Parameters["HueOffset"].SetValue(HueOffset);
                _fx.Parameters["SaturationOffset"].SetValue(SaturationOffset);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            RodEntityManager.Instance.Add(this);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            RodEntityManager.Instance.Remove(this);
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            RodEntityManager.Instance.Remove(this);
        }

        public override void Update()
        {
            if (SceneAs<Level>().Session.GetFlag(Flag))
            {
                triggered = true;
            }
            base.Update();
        }

        private IEnumerator RodController()
        {
            while (true)
            {
                triggered = false;
                state = MovementState.Idling;
                while (!triggered)
                {
                    yield return null;
                }

                IsRunning = true;
                yield return RunUntilBreak();
                SceneAs<Level>().Session.SetFlag(Flag, false);
                IsRunning = false;
            }
        }

        protected Effect _fx;
    }
}
