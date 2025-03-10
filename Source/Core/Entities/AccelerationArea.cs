using Celeste.Mod.Entities;
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
        public enum AccelerateState
        {
            None,
            Accelerate,
            Deaccelerate,
        }

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
            GFX.SpriteBank.CreateOn(_arrow, "Aqua_AccelerationArrow");
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

        public bool EnterEntity(Entity entity)
        {
            return _inEntities.Add(entity);
        }

        public bool ExitEntity(Entity entity)
        {
            return _inEntities.Remove(entity);
        }

        public AccelerateState TryAccelerate(MoveBlock block)
        {
            float acc = (block is AquaMoveBlock amb) ? amb.Acceleration : 300.0f;
            if (IsIdenticalDirection(block.direction))
            {
                int oldSign = MathF.Sign(block.targetSpeed);
                float oldSpeedAbs = MathF.Abs(block.targetSpeed);
                block.targetSpeed += acc * Engine.DeltaTime;
                int newSign = MathF.Sign(block.targetSpeed);
                float newSpeedAbs = MathF.Abs(block.targetSpeed);
                if (oldSign == newSign && newSpeedAbs > oldSpeedAbs)
                    return AccelerateState.Accelerate;
                else
                    return AccelerateState.Deaccelerate;
            }
            else if (IsOppositeDirection(block.direction))
            {
                int oldSign = MathF.Sign(block.targetSpeed);
                float oldSpeedAbs = MathF.Abs(block.targetSpeed);
                block.targetSpeed -= acc * Engine.DeltaTime;
                int newSign = MathF.Sign(block.targetSpeed);
                float newSpeedAbs = MathF.Abs(block.targetSpeed);
                if (oldSign == newSign && newSpeedAbs > oldSpeedAbs)
                    return AccelerateState.Accelerate;
                else
                    return AccelerateState.Deaccelerate;
            }
            return AccelerateState.None;
        }

        public AccelerateState TryAccelerate(MoveGrappleMagnet magnet)
        {
            float acc = magnet.Acceleration;
            if (IsIdenticalDirection(magnet.Direction))
            {
                int oldSign = MathF.Sign(magnet.TargetSpeed);
                float oldSpeedAbs = MathF.Abs(magnet.TargetSpeed);
                magnet.TargetSpeed += acc * Engine.DeltaTime;
                int newSign = MathF.Sign(magnet.TargetSpeed);
                float newSpeedAbs = MathF.Abs(magnet.TargetSpeed);
                if (oldSign == newSign && newSpeedAbs > oldSpeedAbs)
                    return AccelerateState.Accelerate;
                else
                    return AccelerateState.Deaccelerate;
            }
            else if (IsOppositeDirection(magnet.Direction))
            {
                int oldSign = MathF.Sign(magnet.TargetSpeed);
                float oldSpeedAbs = MathF.Abs(magnet.TargetSpeed);
                magnet.TargetSpeed -= acc * Engine.DeltaTime;
                int newSign = MathF.Sign(magnet.TargetSpeed);
                float newSpeedAbs = MathF.Abs(magnet.TargetSpeed);
                if (oldSign == newSign && newSpeedAbs > oldSpeedAbs)
                    return AccelerateState.Accelerate;
                else
                    return AccelerateState.Deaccelerate;
            }
            return AccelerateState.None;
        }

        public AccelerateState TryAccelerate(CrushBlock block)
        {
            float acc = CrushBlock.CrushAccel;
            float sign = block.crushDir.X == 0.0f ? block.crushDir.Y : block.crushDir.X;
            Vector2 movement = block.Position - block.GetPreviousPosition();
            float movementSign = movement.X == 0.0f ? movement.Y : movement.X;
            if (IsIdenticalDirection(block.crushDir))
            {
                block.AddExtraSpeed(acc * Engine.DeltaTime);
                if (movementSign * sign > 0.0f)
                    return AccelerateState.Accelerate;
                else
                    return AccelerateState.Deaccelerate;
            }
            else if (IsOppositeDirection(block.crushDir))
            {
                block.AddExtraSpeed(-acc * Engine.DeltaTime);
                if (movementSign * sign < 0.0f)
                    return AccelerateState.Accelerate;
                else
                    return AccelerateState.Deaccelerate;
            }
            return AccelerateState.None;
        }

        public void CommonAccelerate(Solid solid, float acceleration)
        {
            Vector2 acc = GetAccelerateDirection() * acceleration * Engine.DeltaTime;
            solid.MoveH(0.5f * acc.X * Engine.DeltaTime * Engine.DeltaTime);
            solid.MoveV(0.5f * acc.Y * Engine.DeltaTime * Engine.DeltaTime);
        }

        private bool IsIdenticalDirection(MoveBlock.Directions direction)
        {
            return Direction == direction;
        }

        private bool IsIdenticalDirection(Vector2 direction)
        {
            return Vector2.Dot(GetAccelerateDirection(), direction) > 0.0f;
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

        private bool IsOppositeDirection(Vector2 direction)
        {
            return Vector2.Dot(GetAccelerateDirection(), direction) < 0.0f;
        }

        private Vector2 GetAccelerateDirection()
        {
            switch (Direction)
            {
                case MoveBlock.Directions.Left:
                    return -Vector2.UnitX;
                case MoveBlock.Directions.Right:
                    return Vector2.UnitX;
                case MoveBlock.Directions.Up:
                    return -Vector2.UnitY;
                case MoveBlock.Directions.Down:
                    return Vector2.UnitY;
            }
            return Vector2.Zero;
        }

        private IEnumerator Blink()
        {
            while (true)
            {
                while (AcceleratingEntityCount() == 0)
                {
                    yield return null;
                }

                float elapsed = 0.0f;
                while (AcceleratingEntityCount() > 0 || elapsed < BlinkDuration)
                {
                    elapsed += Engine.DeltaTime;
                    if (elapsed >= BlinkDuration && AcceleratingEntityCount() > 0)
                    {
                        elapsed -= BlinkDuration;
                    }
                    float t = Calc.Clamp(MathF.Sin(MathF.PI / BlinkDuration * elapsed), 0.0f, 1.0f);
                    Color curBorderColor = Color.Lerp(BorderColor, BlinkBorderColor, t);
                    Color curArrowColor = Color.Lerp(ArrowColor, BlinkArrowColor, t);
                    _border.SetColor(curBorderColor);
                    _arrow.SetColor(curArrowColor);
                    yield return null;
                }
            }
        }

        private int AcceleratingEntityCount()
        {
            int accCnt = 0;
            foreach (Entity entity in _inEntities)
            {
                if (entity.Active)
                {
                    if (entity is Solid solid)
                    {
                        if (solid.GetAccelerateState() != AccelerateState.None)
                            ++accCnt;
                    }
                    else
                    {
                        ++accCnt;
                    }
                }
            }
            return accCnt;
        }

        private Image9Slice _border;
        private Sprite _arrow;
        private HashSet<Entity> _inEntities = new HashSet<Entity>(4);
    }
}
