using Celeste.Mod.Aqua.Module;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.Aqua.Core
{
    public class PresentationRecorder
    {
        public static PresentationRecorder Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PresentationRecorder();
                return _instance;
            }
        }

        private static PresentationRecorder _instance;

        private PresentationRecorder()
        {
        }

        public bool IsRecording => _recording;

        public bool BeginRecording(Level level)
        {
            if (_recording)
                return false;

            Player player = level.Tracker.GetEntity<Player>();
            if (player != null)
            {
                GrapplingHook hook = player.GetGrappleHook();
                if (hook != null)
                {
                    _recordingData = new PresentationData();
                    _recording = true;
                    return true;
                }
            }
            return false;
        }

        public void Update(Level level)
        {
            if (_recording)
            {
                Player player = level.Tracker.GetEntity<Player>();
                if (player == null)
                {
                    CancelRecording();
                    return;
                }
                GrapplingHook hook = player.GetGrappleHook();
                var playerFrame = new PlayerFrameData
                {
                    Position = player.Position,
                    Facing = (int)player.Facing,
                    Gravity = ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1 : 1,
                    AnimationID = player.Sprite.CurrentAnimationID,
                    AnimationFrame = player.Sprite.CurrentAnimationFrame,
                    HairCount = player.Sprite.HairCount,
                    HairFacing = (int)player.Hair.Facing,
                    Scale = player.Sprite.Scale,
                    RenderPosition = player.Sprite.RenderPosition,
                };
                if (string.IsNullOrEmpty(playerFrame.AnimationID))
                {
                    playerFrame.AnimationID = player.Sprite.LastAnimationID;
                }
                var hookFrame = new HookFrameData
                {
                    Active = hook.Active,
                    Position = hook.Position,
                    Rotation = hook.Sprite.Rotation,
                    Pivots = hook.Get<HookRope>().AllPivots.Select(pivot => pivot.point).Concat(new[] { player.Center, }).ToArray(),
                    AnimationID = hook.Sprite.CurrentAnimationID,
                    AnimationFrame = hook.Sprite.CurrentAnimationFrame,
                };
                PresentationFrameData frame = new PresentationFrameData
                {
                    PlayerFrame = playerFrame,
                    HookFrame = hookFrame,
                };
                _frames.Add(frame);
            }
        }

        public PresentationData EndRecording()
        {
            if (_recording)
            {
                _recordingData.Frames = new List<PresentationFrameData>(_frames);
                _frames.Clear();
                _recording = false;
                return _recordingData;
            }
            return null;
        }

        public void CancelRecording()
        {
            if (_recording)
            {
                _recording = false;
                _frames.Clear();
                _recordingData = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private PresentationData _recordingData;
        private List<PresentationFrameData> _frames = new List<PresentationFrameData>(512);
        private bool _recording = false;
    }
}
