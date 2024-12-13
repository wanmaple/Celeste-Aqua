using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Tie Rod")]
    [Tracked(false)]
    public class TieRod : Entity
    {
        public string Flag { get; private set; }

        public TieRod(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Flag = data.Attr("flag");
            float w = 16.0f;
            float h = 16.0f;
            Collider = new Hitbox(w, h, -w * 0.5f, -h * 0.5f);
            Vector2 indicatorOffset = new Vector2(0.0f, -2.0f);
            _talker = new TalkComponent(new Rectangle(-(int)w / 2, -(int)h / 2, (int)w, (int)h), new Vector2(indicatorOffset.X, indicatorOffset.Y - h * 0.5f), OnPlayerInteract);
            Add(_sprite = new Sprite());
            GFX.SpriteBank.CreateOn(_sprite, "TieRod");
            _sprite.Play("idle");
            Add(new HookInteractable(OnHookInteract));
            Depth = -10000;
            this.SetHookable(true);
        }

        public override void Update()
        {
            base.Update();
            IReadOnlyList<IRodControllable> entities = RodEntityManager.Instance.GetEntitiesOfFlag(Flag);
            if (entities == null || entities.Any(e => e.IsRunning))
            {
                Remove(_talker);
            }
            else
            {
                Add(_talker);
            }
        }

        public void ChangeState()
        {
            if (!_state)
            {
                _sprite.Play("turn_left", true);
            }
            else
            {
                _sprite.Play("turn_right", true);
            }
            _state = !_state;
        }

        private void OnPlayerInteract(Player player)
        {
            if (Interact())
            {
                Audio.Play("event:/game/general/tie_rod", Position);
                ChangeStates();
            }
            else
            {
                Audio.Play("event:/game/general/tie_rod_stuck", Position);
            }
        }

        private bool OnHookInteract(GrapplingHook hook, Vector2 at)
        {
            hook.Revoke();
            IReadOnlyList<IRodControllable> entities = RodEntityManager.Instance.GetEntitiesOfFlag(Flag);
            {
                if (entities != null && entities.All(e => !e.IsRunning))
                {
                    if (Interact())
                    {
                        Audio.Play("event:/game/general/tie_rod", Position);
                        ChangeStates();
                    }
                    else
                    {
                        Audio.Play("event:/game/general/tie_rod_stuck", Position);
                    }
                }
                else
                {
                    Audio.Play("event:/game/general/tie_rod_stuck", Position);
                }
            }
            return true;
        }

        private void ChangeStates()
        {
            List<Entity> rods = Scene.Tracker.GetEntities<TieRod>();
            foreach (TieRod rod in rods)
            {
                if (rod.Flag == Flag)
                {
                    rod.ChangeState();
                }
            }
        }

        private bool Interact()
        {
            if (_pending)
                return false;
            _pending = true;
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, Execute, 0.5f, true));
            return true;
        }

        private void Execute()
        {
            IReadOnlyList<IRodControllable> entities = RodEntityManager.Instance.GetEntitiesOfFlag(Flag);
            SceneAs<Level>().Session.SetFlag(Flag, true);
            _pending = false;
        }

        private Sprite _sprite;
        private TalkComponent _talker;
        private bool _pending;
        private bool _state;
    }
}
