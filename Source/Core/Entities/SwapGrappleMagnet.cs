using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Swap Grapple Magnet")]
    public class SwapGrappleMagnet : GrappleMagnet
    {
        public enum SwapStates
        {
            Backwarding,
            Forwarding,
        }

        public const float BLINK_DURATION = 1.6f;

        public string SwapFlag { get; private set; }
        public bool DashTrigger { get; private set; }
        public bool FlagTrigger { get; private set; }
        public string FrameTexture { get; private set; }

        public SwapGrappleMagnet(EntityData data, Vector2 offset)
             : base(data, offset)
        {
            SwapFlag = data.Attr("swap_flag");
            DashTrigger = data.Bool("dash_trigger", true);
            FlagTrigger = data.Bool("flag_trigger", false);
            FrameTexture = data.Attr("frame_texture", "objects/frames/frame1");
            if (!GFX.Game.Has(FrameTexture))
                FrameTexture = "objects/frames/frame1";
            _from = Position;
            _to = data.Nodes[0] + offset;
            _maxForwardSpeed = 360.0f / Vector2.Distance(_from, _to);
            _maxBackwardSpeed = _maxForwardSpeed * 0.4f;
            _returnTicker = new TimeTicker(0.8f);
            _returnTicker.Expire();
            Add(new DashListener(OnDash));
            Add(new Coroutine(Blink()));
            int width = (int)MathF.Ceiling(MathF.Abs(_to.X - _from.X) / 8.0f) * 8 + 16;
            int height = (int)MathF.Ceiling(MathF.Abs(_to.Y - _from.Y) / 8.0f) * 8 + 16;
            _backgroundRect = new Image9Slice(GFX.Game[FrameTexture], width, height, Image9Slice.RenderMode.Border);
            _backgroundRect.RenderPosition = new Vector2(MathF.Min(_from.X, _to.X) - 8.0f, MathF.Min(_from.Y, _to.Y) - 8.0f);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Audio.Stop(_returnSfx);
            Audio.Stop(_moveSfx);
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.Stop(_returnSfx);
            Audio.Stop(_moveSfx);
        }

        public override void Update()
        {
            base.Update();
            float dt = Engine.DeltaTime;
            _returnTicker.Tick(dt);
            if (_returnTicker.Check())
            {
                _state = 0;
                _speed = 0.0f;
                _returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return", Center);
                _returnTicker.Expire();
            }
            if (_burst != null)
            {
                _burst.Position = Center;
            }
            if (_state == SwapStates.Forwarding)
            {
                _speed = Calc.Approach(_speed, _maxForwardSpeed, _maxForwardSpeed / 0.2f * Engine.DeltaTime);
            }
            else
            {
                _speed = Calc.Approach(_speed, _maxBackwardSpeed, _maxBackwardSpeed / 1.5f * Engine.DeltaTime);
            }

            float num = _lerp;
            _lerp = Calc.Approach(_lerp, (int)_state, _speed * Engine.DeltaTime);
            if (_lerp != num)
            {
                Vector2 position = Position;
                if (_state == SwapStates.Forwarding && Scene.OnInterval(0.02f))
                {
                    EmitParticles(_to - _from);
                }
                Vector2 targetPos = Vector2.Lerp(_from, _to, _lerp);
                NaiveMove(targetPos - position);
                if (position != Position)
                {
                    Audio.Position(_moveSfx, Center);
                    Audio.Position(_returnSfx, Center);
                    if (Position == _from && _state == SwapStates.Backwarding)
                    {
                        Audio.SetParameter(_returnSfx, "end", 1.0f);
                        Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", Center);
                    }
                    else if (Position == _to && _state == SwapStates.Forwarding)
                    {
                        Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
                    }
                }
            }
            if (_swapping && _lerp >= 1.0f)
            {
                _swapping = false;
            }

            if (FlagTrigger && SceneAs<Level>().Session.GetFlag(SwapFlag))
            {
                TrigSwap();
                Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => SceneAs<Level>().Session.SetFlag(SwapFlag, false), 0.0f, true));
            }
        }

        public override void Render()
        {
            _backgroundRect.Render();
            base.Render();
        }

        private void OnDash(Vector2 direction)
        {
            if (!DashTrigger)
                return;
            TrigSwap();
        }

        private void TrigSwap()
        {
            _swapping = _lerp < 1f;
            _state = SwapStates.Forwarding;
            _returnTicker.Reset();
            _burst = (Scene as Level).Displacement.AddBurst(Center, 0.2f, 0.0f, 16.0f);
            if (_lerp >= 0.2f)
            {
                _speed = _maxForwardSpeed;
            }
            else
            {
                _speed = MathHelper.Lerp(_maxForwardSpeed * 0.333f, _maxForwardSpeed, _lerp / 0.2f);
            }

            Audio.Stop(_returnSfx);
            Audio.Stop(_moveSfx);
            if (!_swapping)
            {
                Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
            }
            else
            {
                _moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", Center);
            }
        }

        private IEnumerator Blink()
        {
            float elapsed = 0.0f;
            while (true)
            {
                elapsed += Engine.DeltaTime;
                if (elapsed >= BLINK_DURATION)
                {
                    elapsed -= BLINK_DURATION;
                }
                float t = Calc.Clamp(MathF.Sin(MathF.PI / BLINK_DURATION * elapsed), 0.0f, 1.0f);
                Color frameColor = Color.Lerp(Calc.HexToColor("cfcfcf"), Calc.HexToColor("7b7b7b"), t);
                _backgroundRect.Color = frameColor;
                yield return null;
            }
        }

        protected override void DoPostMove(Vector2 movement)
        {
            base.DoPostMove(movement);
            _from += movement;
            _to += movement;
            _backgroundRect.RenderPosition += movement;
        }

        private Vector2 _from;
        private Vector2 _to;
        private bool _swapping;
        private float _lerp;
        private float _speed;
        private float _maxForwardSpeed;
        private float _maxBackwardSpeed;
        private SwapStates _state = SwapStates.Backwarding;
        private TimeTicker _returnTicker;
        private EventInstance _returnSfx;
        private EventInstance _moveSfx;
        private Image9Slice _backgroundRect;
        private DisplacementRenderer.Burst _burst;
    }
}
