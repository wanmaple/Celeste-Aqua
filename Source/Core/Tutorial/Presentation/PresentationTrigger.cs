using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.IO;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Presentation Trigger")]
    public class PresentationTrigger : Trigger
    {
        public string PresentationName { get; set; }
        public Color PlayerColor { get; set; }
        public Color HookColor { get; set; }
        public Color DashColor { get; set; }
        public bool Loop { get; set; }
        public float LoopInterval { get; set; }
        public float TrailInterval { get; set; }
        public float TrailLifetime { get; set; }

        public PresentationTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            PresentationName = data.Attr("presentation");
            PlayerColor = data.HexColor("player_color", Calc.HexToColor("a42d2d"));
            HookColor = data.HexColor("hook_color", Calc.HexToColor("ffff00"));
            DashColor = data.HexColor("dash_color", Calc.HexToColor("ffffff"));
            Loop = data.Bool("loop");
            LoopInterval = data.Float("loop_interval", 1.0f);
            TrailInterval = data.Float("trail_interval", 0.1f);
            TrailLifetime = data.Float("trail_lifetime", 0.45f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            string presentationPath = "Maps/Presentations/" + PresentationName;
            if (Everest.Content.TryGet(presentationPath, out ModAsset metadata))
            {
                try
                {
                    _presentation = new PresentationData();
                    var stream = new MemoryStream(metadata.Data);
                    using (var br = new BinaryReader(stream))
                    {
                        _presentation.Deserialize(br);
                    }
                }
                catch (Exception)
                {
                    _presentation = null;
                    AquaDebugger.LogWarning("Presentation '{0}' deserialization failed.", presentationPath);
                }
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            _presentation = null;
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            _presentation = null;
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (_presentation != null)
            {
                PresentationController controller = Scene.Tracker.GetEntity<PresentationController>();
                if (controller != null)
                {
                    controller.RemoveSelf();
                }
                Scene.Add(new PresentationController(_presentation, PlayerColor, HookColor, DashColor, Loop, TrailInterval, TrailLifetime, LoopInterval));
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (_presentation != null)
            {
                PresentationController controller = Scene.Tracker.GetEntity<PresentationController>();
                if (controller != null)
                {
                    controller.RemoveSelf();
                }
            }
        }

        PresentationData _presentation;
    }
}
