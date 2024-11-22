using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Aqua Booster")]
    [Tracked(false)]
    public class AquaBooster : Booster
    {
        public bool Hookable { get; private set; }

        public AquaBooster(Vector2 position, bool red, bool hookable)
            : base(position, red)
        {
            Hookable = hookable;
            if (Hookable)
            {
                if (red)
                {
                    GFX.SpriteBank.CreateOn(sprite, "BoosterPurple");
                }
                else
                {
                    GFX.SpriteBank.CreateOn(sprite, "BoosterOrange");
                }
                HookInteractable com = Get<HookInteractable>();
                com.Interaction = OnHookGrab;
                PlayerCollider com2 = Get<PlayerCollider>();
                com2.OnCollide = OnPlayerEx;
                Add(_movingCoroutine = new Coroutine(MoveUpdate(), false));
            }
        }

        public AquaBooster(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("red"), data.Bool("hookable"))
        {
        }

        public void StartMove(Vector2 direction, float speed)
        {
            _movingDirection = direction;
            _speed = speed;
        }

        private bool OnHookGrab(GrapplingHook hook, Vector2 at)
        {
            hook.Revoke();
            Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
            _movingDirection = Vector2.Normalize(hook.HookDirection);
            _speed = 400.0f;
            return true;
        }

        private void OnPlayerEx(Player player)
        {
            if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer)
            {
                _movingCoroutine.Active = false;
                cannotUseTimer = 0.45f;
                if (red)
                {
                    player.RedBoost(this);
                }
                else
                {
                    player.Boost(this);
                }

                Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_enter" : "event:/game/04_cliffside/greenbooster_enter", Position);
                wiggler.Start();
                sprite.Play("inside");
                sprite.FlipX = player.Facing == Facings.Left;
            }
        }

        private IEnumerator MoveUpdate()
        {
            while (true)
            {
                if (!AquaMaths.IsApproximateZero(_movingDirection) && !AquaMaths.IsApproximateZero(_speed))
                {
                    float dt = Engine.DeltaTime;
                    float length = _speed * dt;
                    Vector2 movement = _movingDirection * length;
                    Segment trace = new Segment(Position, Position + movement);
                    float minMovement = float.MaxValue;
                    List<Entity> soldTiles = Scene.Tracker.GetEntities<SolidTiles>();
                    Rectangle levelBounds = (Scene as Level).Bounds;
                    while (length > 0.0f)
                    {
                        Vector2 sumNormal = Vector2.Zero;
                        bool collided = false;
                        foreach (Solid solid in soldTiles)
                        {
                            if (!solid.Collidable || solid.Collider == null)
                                continue;
                            Grid collider = solid.Collider as Grid;
                            IReadOnlyList<Edge> edges = collider.Edges();
                            if (edges != null)
                            {
                                foreach (Edge edge in edges)
                                {
                                    if (!levelBounds.Contains((int)edge.Segment.Point1.X, (int)edge.Segment.Point1.Y) && !levelBounds.Contains((int)edge.Segment.Point2.X, (int)edge.Segment.Point2.Y))
                                        continue;
                                    if (HitEdge(trace, edge, out Vector2 hit, out Vector2 normal))
                                    {
                                        sumNormal += normal;
                                        minMovement = MathF.Min(minMovement, (hit + normal * (Collider as Circle).Radius - Position).Length());
                                        collided = true;
                                    }
                                }
                            }
                        }
                        if (!collided)
                        {
                            Position += movement;
                            length = 0.0f;
                        }
                        else
                        {
                            sumNormal.Normalize();
                            AquaDebugger.LogInfo("BEFORE DIR: {0} NORMAL {1}", _movingDirection, sumNormal);
                            _movingDirection = AquaMaths.Reflect(_movingDirection, sumNormal);
                            AquaDebugger.LogInfo("AFTER DIR: {0} NORMAL {1}", _movingDirection, sumNormal);
                            Position += _movingDirection * minMovement;
                            trace.Point1 = Position;
                            length -= minMovement;
                            trace.Point2 = Position + length * _movingDirection;
                        }
                    }
                    _speed = MathF.Max(_speed - 40.0f, 0.0f);
                }
                yield return null;
            }
        }

        private bool HitEdge(Segment trace, Edge edge, out Vector2 hitPoint, out Vector2 normal)
        {
            hitPoint = Vector2.Zero;
            normal = edge.Normal;
            float moveAmountAlongNormal = Vector2.Dot(trace.Vector, edge.Normal);
            if (moveAmountAlongNormal >= 0.0f)
                return false;

            float sdf = SDFSegment(trace.Point2, edge.Segment.Point1, edge.Segment.Point2);
            if (sdf >= (Collider as Circle).Radius)
                return false;

            if (edge.Normal.X == 0.0f)
            {
                Vector2 minXPt = edge.Segment.Point1.X < edge.Segment.Point2.X ? edge.Segment.Point1 : edge.Segment.Point2;
                Vector2 maxXPt = minXPt == edge.Segment.Point1 ? edge.Segment.Point2 : edge.Segment.Point1;
                if (trace.Point2.X < minXPt.X)
                {
                    normal = Vector2.Normalize(trace.Point2 - minXPt);
                    hitPoint = minXPt;
                }
                else if (trace.Point2.X > maxXPt.X)
                {
                    normal = Vector2.Normalize(trace.Point2 - maxXPt);
                    hitPoint = maxXPt;
                }
                else
                {
                    float distanceToEdge = MathF.Abs(trace.Point1.Y - edge.Segment.Point1.Y);
                    float totalDistance = MathF.Abs(trace.Point2.Y - trace.Point1.Y);
                    float t = distanceToEdge / totalDistance;
                    hitPoint = AquaMaths.Lerp(trace.Point1, trace.Point2, t);
                }
            }
            else
            {
                Vector2 minYPt = edge.Segment.Point1.Y < edge.Segment.Point2.Y ? edge.Segment.Point1 : edge.Segment.Point2;
                Vector2 maxYPt = minYPt == edge.Segment.Point1 ? edge.Segment.Point2 : edge.Segment.Point1;
                if (trace.Point2.X < minYPt.Y)
                {
                    normal = Vector2.Normalize(trace.Point2 - minYPt);
                    hitPoint = minYPt;
                }
                else if (trace.Point2.X > maxYPt.X)
                {
                    normal = Vector2.Normalize(trace.Point2 - maxYPt);
                    hitPoint = maxYPt;
                }
                else
                {
                    float distanceToEdge = MathF.Abs(trace.Point1.X - edge.Segment.Point1.X);
                    float totalDistance = MathF.Abs(trace.Point2.X - trace.Point1.X);
                    float t = distanceToEdge / totalDistance;
                    hitPoint = AquaMaths.Lerp(trace.Point1, trace.Point2, t);
                }
            }
            return true;
        }

        private float SDFSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 pa = p - a, ba = b - a;
            float h = Calc.Clamp(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba), 0.0f, 1.0f);
            return (pa - ba * h).Length();
        }

        internal Vector2 _movingDirection = Vector2.Zero;
        internal float _speed = 0.0f;
        internal Coroutine _movingCoroutine;
    }
}
