using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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

        public string CutsceneID { get; private set; }

        public CustomCutscene(string cutsceneId, bool fadeInOnSkip, bool endingChapter)
            : base(fadeInOnSkip, endingChapter)
        {
            CutsceneID = cutsceneId;
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
            string[] lines = content.Split("{break}");
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
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
                }
            }
            _behaviors = behaviors.ToArray();
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
                    if (spriteId != "default")
                    {
                        GFX.SpriteBank.CreateOn(player.Sprite, spriteId);
                    }
                    player.Sprite.Play(animId);
                }
                else if (int.TryParse(target, out int id))
                {
                    List<CustomNPC> npcs = level.Entities.FindAll<CustomNPC>();
                    CustomNPC targetNpc = npcs.FirstOrDefault(npc => ((EntityID)npc.GetType().GetField("id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(npc)).ID == id);
                    if (targetNpc != null)
                    {
                        if (spriteId != "default")
                        {
                            GFX.SpriteBank.CreateOn(targetNpc.Sprite, spriteId);
                        }
                        targetNpc.Sprite.Play(animId);
                    }
                }
            }
            yield return null;
        }

        private static IEnumerator DoFacing(CustomBehavior behavior, Level level, CustomCutscene cutscene)
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
                else if (int.TryParse(target, out int id))
                {
                    List<CustomNPC> npcs = level.Entities.FindAll<CustomNPC>();
                    CustomNPC targetNpc = npcs.FirstOrDefault(npc => ((EntityID)npc.GetType().GetField("id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(npc)).ID == id);
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
                    else if (int.TryParse(target, out int id))
                    {
                        List<CustomNPC> npcs = level.Entities.FindAll<CustomNPC>();
                        CustomNPC targetNpc = npcs.FirstOrDefault(npc => ((EntityID)npc.GetType().GetField("id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(npc)).ID == id);
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
                else if (int.TryParse(target, out int id))
                {
                    List<CustomNPC> npcs = level.Entities.FindAll<CustomNPC>();
                    CustomNPC targetNpc = npcs.FirstOrDefault(npc => ((EntityID)npc.GetType().GetField("id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(npc)).ID == id);
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
        private static readonly Func<CustomBehavior, Level, CustomCutscene, IEnumerator>[] BEHAVIOR_DELEGATES = new Func<CustomBehavior, Level, CustomCutscene, IEnumerator>[(int)CustomBehaviors.Count]
        {
            DoAnimation,
            DoFacing,
            DoWalk,
            DoCameraTo,
            DoCameraZoom,
            DoPlayAudio,
            DoDialogue,
            DoWaitFor,
        };
    }
}
