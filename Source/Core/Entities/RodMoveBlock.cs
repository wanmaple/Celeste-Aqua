using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.Entities;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Rod Move Block")]
    [Tracked(false)]
    public class RodMoveBlock : AquaMoveBlock, IRodControllable
    {
        public string Flag { get; private set; }
        public bool IsRunning { get; private set; }

        public RodMoveBlock(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Flag = data.Attr("flag");
            idleBgFill = data.HexColor("idle_color");
            pressedBgFill = data.HexColor("moving_color");
            breakingBgFill = data.HexColor("break_color");
            Coroutine coroutine = Get<Coroutine>();
            Remove(coroutine);
            Add(new Coroutine(RodController()));
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
    }
}
