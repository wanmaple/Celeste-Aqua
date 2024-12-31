using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Celeste.Mod.Aqua.Core
{
    public class CustomCutscene : CutsceneEntity
    {
        public enum CustomBehaviors
        {
            Anime,
            Face,
            Walk,
            CameraTo,
            CameraZoom,
            PlayAudio,
            Dialogue,
            WaitFor,

            Count,
        }

        public class CustomBehavior
        {
            public CustomBehaviors type;
            public string[] args;
        }

        private struct FinalPositionInfo
        {
            public Entity entity;
            public Vector2 startupPosition;
            public float movement;
            public int facing;
        }

        public string CutsceneID { get; private set; }

        public CustomCutscene(string cutsceneId, bool fadeInOnSkip, bool endingChapter)
            : base(fadeInOnSkip, endingChapter)
        {
            CutsceneID = cutsceneId;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            AnalyzeBehaviors();
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        public override void OnEnd(Level level)
        {
            Player player = level.Tracker.GetEntity<Player>();
            player.StateMachine.State = (int)AquaStates.StNormal;
            foreach (var pair in _finalPositions)
            {
                string name = pair.Key;
                FinalPositionInfo info = pair.Value;
                info.entity.Position = info.startupPosition + new Vector2(info.movement, 0.0f);
                if (info.facing != 0)
                {
                    if (name == "player")
                    {
                        player.Facing = (Facings)info.facing;
                    }
                    else
                    {
                        (info.entity as AquaNPC).Sprite.Scale.X = (float)info.facing;
                    }
                }
            }
            AquaModule.Session.MarkFlag(CutsceneID);
            level.ResetZoom();
        }

        private IEnumerator Cutscene(Level level)
        {
            Player player = level.Tracker.GetEntity<Player>();
            player.StateMachine.State = (int)AquaStates.StDummy;
            foreach (CustomBehavior behavior in _behaviors)
            {
                yield return BEHAVIOR_DELEGATES[(int)behavior.type].Invoke(behavior, level, this);
            }
            EndCutscene(level);
        }

        private void AnalyzeBehaviors()
        {
            string content = string.Empty;
            if (string.IsNullOrEmpty(CutsceneID))
            {
                _behaviors = Array.Empty<CustomBehavior>();
                return;
            }
            if (string.IsNullOrEmpty(content = Dialog.Get(CutsceneID)))
            {
                _behaviors = Array.Empty<CustomBehavior>();
                return;
            }

            List<CustomBehavior> behaviors = new List<CustomBehavior>(64);
            Dictionary<string, List<CustomBehavior>> moveBehaviors = new Dictionary<string, List<CustomBehavior>>(32);
            string[] lines = content.Split("{break}");
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] splits = line.Trim().Split(' ');
                if (splits.Length > 1)
                {
                    CustomBehaviors type = Enum.Parse<CustomBehaviors>(splits[0]);
                    string[] args = new string[splits.Length - 1];
                    Array.Copy(splits, 1, args, 0, splits.Length - 1);
                    CustomBehavior behavior = new CustomBehavior
                    {
                        type = type,
                        args = args,
                    };
                    behaviors.Add(behavior);

                    if (type == CustomBehaviors.Face || type == CustomBehaviors.Walk)
                    {
                        if (args.Length >= 2)
                        {
                            string target = args[0];
                            if (!moveBehaviors.TryGetValue(target, out List<CustomBehavior> moves))
                            {
                                moves = new List<CustomBehavior>(8);
                                moveBehaviors.Add(target, moves);
                            }
                            moves.Add(behavior);
                        }
                    }
                }
            }
            _behaviors = behaviors.ToArray();

            foreach (var pair in moveBehaviors)
            {
                string name = pair.Key;
                List<CustomBehavior> moves = pair.Value;
                Vector2 startupPos = Vector2.Zero;
                Entity entity = null;
                if (name == "player")
                {
                    Player player = Scene.Tracker.GetEntity<Player>();
                    startupPos = player.Position;
                    entity = player;
                }
                else
                {
                    List<Entity> npcs = Scene.Tracker.GetEntities<AquaNPC>();
                    AquaNPC targetNpc = npcs.FirstOrDefault(npc => (npc as AquaNPC).Name == moves[0].args[0]) as AquaNPC;
                    if (targetNpc != null)
                    {
                        entity = targetNpc;
                        startupPos = targetNpc.Position;
                    }
                    else
                    {
                        continue;
                    }
                }

                float movement = 0.0f;
                int facing = 0; // Means no change.
                foreach (CustomBehavior behavior in moves)
                {
                    if (behavior.type == CustomBehaviors.Walk && int.TryParse(behavior.args[1], out int distance) && distance != 0)
                    {
                        movement += distance;
                        facing = distance > 0 ? 1 : -1;
                    }
                    else if (behavior.type == CustomBehaviors.Face)
                    {
                        string dir = behavior.args[1];
                        if (dir == "left")
                            facing = -1;
                        else if (dir == "right")
                            facing = 1;
                    }
                }
                _finalPositions.Add(name, new FinalPositionInfo { entity = entity, startupPosition = startupPos, facing = facing, movement = movement, });
            }
        }

        private static IEnumerator DoAnimation(CustomBehavior behavior, Level level, CustomCutscene cutscene)
        {
            if (behavior.args.Length >= 3)
            {
                string target = behavior.args[0];
                string spriteId = behavior.args[1];
                string animId = behavior.args[2];
                if (target == "player")
                {
                    Player player = level.Tracker.GetEntity<Player>();
                    player.Sprite.Play(animId);
                }
                else if (!string.IsNullOrEmpty(target))
                {
                    List<Entity> npcs = level.Tracker.GetEntities<AquaNPC>();
                    AquaNPC targetNpc = npcs.FirstOrDefault(npc => (npc as AquaNPC).Name == target) as AquaNPC;
                    if (targetNpc != null)
                    {
                        if (spriteId != "default")
                        {
                            GFX.SpriteBank.CreateOn(targetNpc.Sprite, spriteId);
                        }
                        else
                        {
                            GFX.SpriteBank.CreateOn(targetNpc.Sprite, targetNpc.Name);
                        }
                        targetNpc.Sprite.Play(animId);
                    }
                }
            }
            yield return null;
        }

        private static IEnumerator DoFace(CustomBehavior behavior, Level level, CustomCutscene cutscene)
        {
            if (behavior.args.Length >= 2)
            {
                string target = behavior.args[0];
                string facing = behavior.args[1];
                if (target == "player")
                {
                    Player player = level.Tracker.GetEntity<Player>();
                    if (facing == "left")
                    {
                        player.Facing = Facings.Left;
                    }
                    else if (facing == "right")
                    {
                        player.Facing = Facings.Right;
                    }
                }
                else if (!string.IsNullOrEmpty(target))
                {
                    List<Entity> npcs = level.Tracker.GetEntities<AquaNPC>();
                    AquaNPC targetNpc = npcs.FirstOrDefault(npc => (npc as AquaNPC).Name == target) as AquaNPC;
                    if (targetNpc != null)
                    {
                        if (facing == "left")
                        {
                            targetNpc.Sprite.Scale = new Vector2(-1.0f, 1.0f);
                        }
                        else if (facing == "right")
                        {
                            targetNpc.Sprite.Scale = new Vector2(1.0f, 1.0f);
                        }
                    }
                }
            }
            yield return null;
        }

        private static IEnumerator DoWalk(CustomBehavior behavior, Level level, CustomCutscene cutscene)
        {
            if (behavior.args.Length >= 2)
            {
                string target = behavior.args[0];
                string text = behavior.args[1];
                if (int.TryParse(text, out int distance))
                {
                    if (target == "player")
                    {
                        Player player = level.Tracker.GetEntity<Player>();
                        yield return player.DummyWalkTo(player.X + distance);
                    }
                    else if (!string.IsNullOrEmpty(target))
                    {
                        List<Entity> npcs = level.Tracker.GetEntities<AquaNPC>();
                        AquaNPC targetNpc = npcs.FirstOrDefault(npc => (npc as AquaNPC).Name == target) as AquaNPC;
                        if (targetNpc != null)
                        {
                            yield return targetNpc.DummyWalkTo(targetNpc.X + distance);
                        }
                    }
                }
            }
            yield return null;
        }

        private static IEnumerator DoCameraTo(CustomBehavior behavior, Level level, CustomCutscene cutscene)
        {
            if (behavior.args.Length >= 3)
            {

            }
            yield return null;
        }

        private static IEnumerator DoCameraZoom(CustomBehavior behavior, Level level, CustomCutscene cutscene)
        {
            if (behavior.args.Length >= 3)
            {

            }
            yield return null;
        }

        private static IEnumerator DoPlayAudio(CustomBehavior behavior, Level level, CustomCutscene cutscene)
        {
            if (behavior.args.Length >= 2)
            {
                string audioId = behavior.args[0];
                string target = behavior.args[1];
                Vector2 position = Vector2.Zero;
                if (target == "player")
                {
                    Player player = level.Tracker.GetEntity<Player>();
                    position = player.Position;
                }
                else if (!string.IsNullOrEmpty(target))
                {
                    List<Entity> npcs = level.Tracker.GetEntities<AquaNPC>();
                    AquaNPC targetNpc = npcs.FirstOrDefault(npc => (npc as AquaNPC).Name == target) as AquaNPC;
                    if (targetNpc != null)
                    {
                        position = targetNpc.Position;
                    }
                }
                var instId = Audio.Play(audioId, position);
                if (behavior.args.Length >= 3)
                {
                    string text = behavior.args[2];
                    if (float.TryParse(text, out float duration))
                    {
                        cutscene.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () =>
                        {
                            Audio.Stop(instId, false);
                        }, duration, true));
                        yield return duration;
                    }
                }
            }
            yield return null;
        }

        private static IEnumerator DoDialogue(CustomBehavior behavior, Level level, CustomCutscene cutscene)
        {
            if (behavior.args.Length >= 1)
            {
                string dialogId = behavior.args[0];
                if (Dialog.Has(dialogId))
                {
                    yield return Textbox.Say(dialogId);
                }
            }
            yield return null;
        }

        private static IEnumerator DoWaitFor(CustomBehavior behavior, Level level, CustomCutscene cutscene)
        {
            if (behavior.args.Length >= 1)
            {
                if (float.TryParse(behavior.args[0], out float time))
                {
                    yield return time;
                }
            }
            yield return null;
        }

        private CustomBehavior[] _behaviors;
        private Dictionary<string, FinalPositionInfo> _finalPositions = new Dictionary<string, FinalPositionInfo>(8);

        private static readonly Func<CustomBehavior, Level, CustomCutscene, IEnumerator>[] BEHAVIOR_DELEGATES = new Func<CustomBehavior, Level, CustomCutscene, IEnumerator>[(int)CustomBehaviors.Count]
        {
            DoAnimation,
            DoFace,
            DoWalk,
            DoCameraTo,
            DoCameraZoom,
            DoPlayAudio,
            DoDialogue,
            DoWaitFor,
        };
    }
}
