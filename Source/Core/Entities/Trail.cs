using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class Trail : Entity
    {
        public Trail(Vector2 position, Vector2 origin, string spriteName, string animName, Color color, Vector2 scale)
            : base(position)
        {
            Add(_trailSprite = new Sprite());
            GFX.SpriteBank.CreateOn(_trailSprite, spriteName);
            _trailSprite.Origin = origin;
            _trailSprite.Color = color;
            _trailSprite.Scale = scale;
            _trailSprite.Play(animName);
        }

        public override void Update()
        {
            base.Update();
            if (!_trailSprite.Animating)
            {
                Scene.Remove(this);
            }
        }

        private Sprite _trailSprite;
    }
}
