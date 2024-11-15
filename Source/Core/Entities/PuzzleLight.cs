using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Puzzle Light")]
    [Tracked(true)]
    public class PuzzleLight : Entity
    {
        public string PuzzleID { get; private set; }
        public int SequenceID { get; private set; } = 0;
        public bool SwitchOn { get; set; } = false;
        public Color LitColor { get; private set; } = Color.White;

        public PuzzleLight(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            PuzzleID = data.Attr("puzzleId");
            SequenceID = data.Int("sequence");
            SwitchOn = data.Bool("on");
            LitColor = data.HexColor("color");

            Add(new PlayerInOut(OnPlayerIn, null));
            _texLamp = GFX.Game["objects/common/puzzle_light"];
            _texLampMask = GFX.Game["objects/common/puzzle_light_mask"];
            _texLit = GFX.Game["objects/common/lit"];

            Collider = new Hitbox(_texLamp.Width, _texLamp.Height);
        }

        public override void Awake(Scene scene)
        {
            if (!string.IsNullOrEmpty(PuzzleID))
            {
                List<Entity> puzzleCenters = scene.Tracker.GetEntities<PuzzleEntity>();
                foreach (PuzzleEntity center in puzzleCenters)
                {
                    if (center.PuzzleID == PuzzleID)
                    {
                        _relatedCenter = center;
                        _relatedCenter.RelatedLights.Add(this);
                        break;
                    }
                }
            }
        }

        private void OnPlayerIn(Player player)
        {
            if (_relatedCenter == null)
                return;

            if (!SwitchOn && _relatedCenter.CanLitOn(this))
            {
                SwitchOn = true;
            }
            else if (SwitchOn && _relatedCenter.CanLitOff(this))
            {
                SwitchOn = false;
            }
        }

        public override void Render()
        {
            if (_texLamp != null && _texLampMask != null && _texLit != null)
            {
                Vector2 center = new Vector2(_texLamp.Width * 0.5f, _texLamp.Height * 0.5f);
                _texLamp.Draw(Position, center, Color.White);
                _texLampMask.Draw(Position, center, LitColor);
                if (SwitchOn)
                {
                    _texLit.Draw(Position + new Vector2(0.0f, -2.0f), new Vector2(_texLit.Width * 0.5f, _texLit.Height * 0.5f), LitColor);
                }
            }
        }

        private PuzzleEntity _relatedCenter;
        private MTexture _texLamp;
        private MTexture _texLampMask;
        private MTexture _texLit;
    }
}
