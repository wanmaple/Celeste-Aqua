﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Acceleration Area")]
    [Tracked(false)]
    public class AccelerationArea : Entity
    {
        public MoveBlock.Directions Direction { get; private set; }
        public Color BorderColor { get; private set; }
        public Color ArrowColor { get; private set; }
        public Color BlinkBorderColor { get; private set; }
        public Color BlinkArrowColor { get; private set; }
        public float BlinkDuration { get; private set; }

        public AccelerationArea(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            BorderColor = data.HexColor("border_color");
            ArrowColor = data.HexColor("arrow_color");
            BlinkBorderColor = data.HexColor("blink_border_color");
            BlinkArrowColor = data.HexColor("blink_arrow_color");
            Collider = new Hitbox(data.Width, data.Height);
            Add(_border = new Image9Slice(GFX.Game["objects/acceleration_area/area"], data.Width, data.Height, Image9Slice.RenderMode.Border));
            _border.SetColor(data.HexColor("border_color"));
            Add(_arrow = new Sprite());
            GFX.SpriteBank.CreateOn(_arrow, "AccelerationArrow");
            _arrow.RenderPosition += new Vector2(data.Width * 0.5f, data.Height * 0.5f);
            _arrow.SetColor(data.HexColor("arrow_color"));
            _arrow.Play("idle");
            BlinkDuration = Calc.Max(data.Float("blink_duration"), 0.1f);
            Add(new Coroutine(Blink()));
            switch (data.Attr("direction"))
            {
                case "Up":
                    Direction = MoveBlock.Directions.Up;
                    _arrow.Rotation = -MathF.PI * 0.5f;
                    break;
                case "Down":
                    Direction = MoveBlock.Directions.Down;
                    _arrow.Rotation = MathF.PI * 0.5f;
                    break;
                case "Left":
                    Direction = MoveBlock.Directions.Left;
                    _arrow.Rotation = MathF.PI;
                    break;
                case "Right":
                default:
                    Direction = MoveBlock.Directions.Right;
                    break;
            }
            Depth = Depths.SolidsBelow;
        }

        public bool TryAccelerate(MoveBlock block)
        {
            float acc = (block is AquaMoveBlock amb) ? amb.Acceleration : 300.0f;
            if (IsIdenticalDirection(block.direction))
            {
                block.targetSpeed += acc * Engine.DeltaTime;
                return true;
            }
            else if (IsOppositeDirection(block.direction))
            {
                block.targetSpeed -= acc * Engine.DeltaTime;
                return true;
            }
            return false;
        }

        public void StartBlink()
        {
            _blink = true;
        }

        private bool IsIdenticalDirection(MoveBlock.Directions direction)
        {
            return Direction == direction;
        }

        private bool IsOppositeDirection(MoveBlock.Directions direction)
        {
            switch (Direction)
            {
                case MoveBlock.Directions.Left:
                    return direction == MoveBlock.Directions.Right;
                case MoveBlock.Directions.Right:
                    return direction == MoveBlock.Directions.Left;
                case MoveBlock.Directions.Up:
                    return direction == MoveBlock.Directions.Down;
                case MoveBlock.Directions.Down:
                    return direction == MoveBlock.Directions.Up;
            }
            return false;
        }

        private IEnumerator Blink()
        {
            while (true)
            {
                while (!_blink)
                {
                    yield return null;
                }

                _blink = false;
                float elapsed = 0.0f;
                while (elapsed < BlinkDuration)
                {
                    if (_blink)
                    {
                        if (elapsed > BlinkDuration * 0.5f)
                        {
                            elapsed = BlinkDuration - elapsed;
                        }
                        _blink = false;
                    }
                    elapsed += Engine.DeltaTime;
                    float t = Calc.Clamp(MathF.Sin(MathF.PI / BlinkDuration * elapsed), 0.0f, 1.0f);
                    Color curBorderColor = Color.Lerp(BorderColor, BlinkBorderColor, t);
                    Color curArrowColor = Color.Lerp(ArrowColor, BlinkArrowColor, t);
                    _border.SetColor(curBorderColor);
                    _arrow.SetColor(curArrowColor);
                    yield return null;
                }
            }
        }

        private Image9Slice _border;
        private Sprite _arrow;
        private bool _blink;
        private List<MoveBlock> _nowIn = new List<MoveBlock>(4);
    }
}