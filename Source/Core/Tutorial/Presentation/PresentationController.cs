using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(false)]
    public class PresentationController : Entity
    {
        public Color PlayerColor { get; set; }
        public Color HookColor { get; set; }
        public bool Loop { get; set; }
        public float TrailInterval { get; set; }
        public float TrailLifetime { get; set; }
        public float LoopInterval { get; set; }

        public PresentationController(PresentationData presentation, Color playerColor, Color hookColor, bool loop, float trailInterval, float trailLifetime, float loopInterval)
        {
            _presentation = presentation;
            PlayerColor = playerColor;
            HookColor = hookColor;
            Loop = loop;
            TrailInterval = trailInterval;
            TrailLifetime = trailLifetime;
            LoopInterval = loopInterval;
            _loopIntervalTicker = new TimeTicker(LoopInterval);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            _playbackPlayer = new PresentationPlayer(0.0f);
            _playbackPlayer.SetColor(PlayerColor);
            scene.Add(_playbackPlayer);
            _playbackHook = new PresentationGrapplingHook();
            _playbackHook.SetColor(HookColor);
            _playbackHook.Visible = false;
            scene.Add(_playbackHook);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (var trail in _trails)
            {
                scene.Remove(trail);
            }
            scene.Remove(_playbackPlayer);
            scene.Remove(_playbackHook);
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            foreach (var trail in _trails)
            {
                scene.Remove(trail);
            }
            scene.Remove(_playbackPlayer);
            scene.Remove(_playbackHook);
        }

        public override void Update()
        {
            base.Update();

            if (_currentFrame == _presentation.Frames.Count)
            {
                if (Loop)
                {
                    if (!_waiting)
                    {
                        _loopIntervalTicker.Reset();
                        _playbackPlayer.Visible = _playbackHook.Visible = false;
                        _waiting = true;
                        return;
                    }
                    else
                    {
                        _loopIntervalTicker.Tick(Engine.DeltaTime);
                        if (_loopIntervalTicker.Check())
                        {
                            _waiting = false;
                            _playbackPlayer.Visible = _playbackHook.Visible = true;
                            _currentFrame = 0;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    RemoveSelf();
                    return;
                }
            }
            if (Scene.OnInterval(TrailInterval))
            {
                var trail = new PresentationPlayer(TrailLifetime);
                trail.SetColor(PlayerColor);
                Scene.Add(trail);
                trail.Apply(_presentation.Frames[_currentFrame].PlayerFrame);
                _trails.Add(trail);
            }
            _playbackPlayer.Apply(_presentation.Frames[_currentFrame].PlayerFrame);
            _playbackHook.Apply(_presentation.Frames[_currentFrame].HookFrame);
            ++_currentFrame;
        }

        private PresentationData _presentation;
        private PresentationPlayer _playbackPlayer;
        private PresentationGrapplingHook _playbackHook;
        private List<PresentationPlayer> _trails = new List<PresentationPlayer>();
        private int _currentFrame;
        private TimeTicker _loopIntervalTicker;
        private bool _waiting = false;
    }
}
