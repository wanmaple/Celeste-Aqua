using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity(
        "Aqua/Gravity Spike Up = LoadUp",
        "Aqua/Gravity Spike Right = LoadRight",
        "Aqua/Gravity Spike Down = LoadDown",
        "Aqua/Gravity Spike Left = LoadLeft"
        )]
    public class GravitySpikes : Spikes
    {
        public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData data) => new GravitySpikes(data, offset, Directions.Up);
        public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData data) => new GravitySpikes(data, offset, Directions.Right);
        public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData data) => new GravitySpikes(data, offset, Directions.Down);
        public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData data) => new GravitySpikes(data, offset, Directions.Left);

        public bool EnableOnGravityInverted { get; private set; }
        public Color SpikeColor { get; private set; }
        public string DisableType { get; private set; }

        public GravitySpikes(EntityData data, Vector2 offset, Directions dir)
            : base(data, offset, dir)
        {
            SpikeColor = data.HexColor("color", Color.White);
            switch (data.Attr("gravity", "Normal"))
            {
                case "Inverted":
                    EnableOnGravityInverted = true;
                    break;
                default:
                    EnableOnGravityInverted = false;
                    break;
            }
            DisableType = data.Attr("disable_type");
            StaticMover staticMover = Get<StaticMover>();
            bool attach = data.Bool("attach");
            if (!attach)
            {
                Remove(staticMover);
            }
            else
            {
                if (staticMover != null)
                {
                    staticMover.OnEnable = OnEnabled;
                    staticMover.OnDisable = OnDisabled;
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            string text = Direction.ToString().ToLower();
            _defaultTexture = GFX.Game.GetAtlasSubtexturesAt("danger/spikes/" + spikeType + "_" + text, 0);  // Random choose not supported yet, TODO
            _disableTexture = GFX.Game.GetAtlasSubtexturesAt("danger/spikes/" + DisableType + "_" + text, 0);
            foreach (Component com in Components)
            {
                if (com is Image image)
                {
                    image.Texture = _defaultTexture;
                    image.Color = SpikeColor;
                    _images.Add(image);
                }
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            bool currentGravityInverted = ModInterop.GravityHelper.IsPlayerGravityInverted();
            bool enabled = currentGravityInverted == EnableOnGravityInverted;
            SetEnabled(enabled);
        }

        public override void Update()
        {
            base.Update();
            bool currentGravityInverted = ModInterop.GravityHelper.IsPlayerGravityInverted();
            bool enabled = currentGravityInverted == EnableOnGravityInverted;
            SetEnabled(enabled);
        }

        private void SetEnabled(bool enabled)
        {
            if (enabled != _enabled)
            {
                _enabled = enabled;
                Collidable = _enabled;
                foreach (Image image in _images)
                {
                    image.Texture = _enabled ? _defaultTexture : _disableTexture;
                }
            }
        }

        private void OnEnabled()
        {
            base.OnEnable();
            SetSpikeColor(SpikeColor);
        }

        private void OnDisabled()
        {
            base.OnDisable();
            SetSpikeColor(SpikeColor);
        }

        private List<Image> _images = new List<Image>();
        private MTexture _defaultTexture;
        private MTexture _disableTexture;
        private bool _enabled = true;
    }
}
