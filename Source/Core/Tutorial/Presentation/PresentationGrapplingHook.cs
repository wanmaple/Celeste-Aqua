using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public class PresentationGrapplingHook : Entity
    {
        public PresentationGrapplingHook()
        {
            Add(_sprite = new HookSprite());
            _ropeRenderer = new RopeRenderer(GFX.Game["objects/hook/rope_white"]);
        }

        public override void Render()
        {
            base.Render();
            _ropeRenderer.Render(_pivotSegments);
        }

        public void Apply(HookFrameData frame)
        {
            if (_sprite.Has(frame.AnimationID))
            {
                _sprite.Play(frame.AnimationID);
                _sprite.CurrentAnimationFrame = frame.AnimationFrame;
                _sprite.Stop();
            }
            else
            {
                AquaDebugger.LogInfo("ANIMATION ID: {0} NOT EXIST.", frame.AnimationID);
            }
            Position = frame.Position;
            _sprite.Rotation = frame.Rotation;
            _pivotSegments.Clear();
            for (int i = 0; i < frame.Pivots.Length - 1; i++)
            {
                Vector2 pt1 = frame.Pivots[i];
                Vector2 pt2 = frame.Pivots[i + 1];
                _pivotSegments.Add(new Segment(pt1, pt2));
            }
            Visible = frame.Active;
        }

        public void SetColor(Color color)
        {
            _sprite.Color = color;
            _ropeRenderer.RopeColor = color;
        }

        private HookSprite _sprite;
        private RopeRenderer _ropeRenderer;
        private List<Segment> _pivotSegments = new List<Segment>(8);
    }
}
