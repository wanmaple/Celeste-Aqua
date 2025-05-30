﻿using Celeste.Mod.Aqua.Miscellaneous;
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
        public Color DashColor { get; set; }
        public bool Loop { get; set; }
        public float TrailInterval { get; set; }
        public float TrailLifetime { get; set; }
        public float LoopInterval { get; set; }

        public PresentationController(PresentationData presentation, Color playerColor, Color hookColor, Color dashColor, bool loop, float trailInterval, float trailLifetime, float loopInterval)
        {
            _presentation = presentation;
            PlayerColor = playerColor;
            HookColor = hookColor;
            DashColor = dashColor;
            Loop = loop;
            TrailInterval = trailInterval;
            TrailLifetime = trailLifetime;
            LoopInterval = loopInterval;
            _loopIntervalTicker = new TimeTicker(LoopInterval);
            _scheduler = new Scheduler(TrailInterval);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            _playbackPlayer = new PresentationPlayer(0.0f);
            _playbackPlayer.PlayerColor = PlayerColor;
            _playbackPlayer.DashColor = DashColor;
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

            _scheduler.Update(Engine.DeltaTime);
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
                            _scheduler.Reset();
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
            _playbackPlayer.Apply(_presentation.Frames[_currentFrame].PlayerFrame);
            _playbackHook.Apply(_presentation.Frames[_currentFrame].HookFrame);
            if (_scheduler.OnInterval)
            {
                var trail = _playbackPlayer.CreateTrail(TrailLifetime);
                Scene.Add(trail);
                _trails.Add(trail);
            }
            ++_currentFrame;
        }

        private PresentationData _presentation;
        private PresentationPlayer _playbackPlayer;
        private PresentationGrapplingHook _playbackHook;
        private List<PresentationPlayer> _trails = new List<PresentationPlayer>();
        private int _currentFrame;
        private TimeTicker _loopIntervalTicker;
        private Scheduler _scheduler;
        private bool _waiting = false;
    }
}
