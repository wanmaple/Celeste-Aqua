using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Puzzle Light")]
    [Tracked(false)]
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
            Add(_spriteBulb = new Sprite());
            GFX.SpriteBank.CreateOn(_spriteBulb, "Aqua_PuzzleLight");
            _spriteBulb.Justify = new Vector2(0.5f, 0.5f);
            _spriteBulb.SetColor(LitColor);
            _spriteBulb.Play("idle");
            Add(_imgPedestal = new Image(GFX.Game["objects/puzzle_light/pedestal"]));
            _imgPedestal.JustifyOrigin(0.5f, 0.5f);
            Add(_imgLit = new Image(GFX.Game["objects/puzzle_light/lit"]));
            _imgLit.JustifyOrigin(0.5f, 0.5f);
            _imgLit.SetColor(LitColor);
            _imgLit.Visible = SwitchOn;

            Collider = new Hitbox(_spriteBulb.Width, _spriteBulb.Height, -_spriteBulb.Width * 0.5f, -_spriteBulb.Height * 0.5f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            PuzzleEntity center = FindRelatedCenter(scene);
            if (center != null)
            {
                center.RelatedLights.Add(this);
            }
        }

        public override void Update()
        {
            base.Update();
            _imgLit.Visible = SwitchOn;
        }

        private void OnPlayerIn(Player player)
        {
            if (_relatedCenter == null)
                _relatedCenter = FindRelatedCenter(Scene);
            if (_relatedCenter == null)
                return;

            if (!SwitchOn && _relatedCenter.CanLitOn(this))
            {
                SwitchOn = true;
                _imgLit.Visible = true;
                Audio.Play("event:/game/05_mirror_temple/torch_activate", Position);
            }
            else if (SwitchOn && _relatedCenter.CanLitOff(this))
            {
                SwitchOn = false;
                _imgLit.Visible = false;
            }
        }

        private PuzzleEntity FindRelatedCenter(Scene scene)
        {
            if (!string.IsNullOrEmpty(PuzzleID))
            {
                List<Entity> puzzleCenters = scene.Tracker.GetEntities<PuzzleEntity>();
                foreach (PuzzleEntity center in puzzleCenters)
                {
                    if (center.PuzzleID == PuzzleID)
                    {
                        return center;
                    }
                }
            }
            return null;
        }

        private PuzzleEntity _relatedCenter;
        private Sprite _spriteBulb;
        private Image _imgPedestal;
        private Image _imgLit;
    }
}
