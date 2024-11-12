using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Aqua Bird Trigger")]
    [Tracked(true)]
    public class AquaBirdTrigger : Trigger
    {
        public string BirdID { get; set; }
        public int TutorialIndex { get; set; }
        public string ConditionFunction { get; set; }

        public AquaBirdTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            BirdID = data.Attr("birdId");
            TutorialIndex = data.Int("tutorialIndex");
            ConditionFunction = data.Attr("conditionFunction");
            if (ConditionFunction.StartsWith("mod:"))
            {
                string identifier = ConditionFunction.Substring(4);
                string[] path = identifier.Split('.');
                if (path.Length >= 3)
                {
                    string modName = path[0];
                    string className = path[1];
                    string methodName = path[2];
                    EverestModule everestModule = Enumerable.FirstOrDefault(Everest.Modules, (EverestModule m) => m.Metadata.Name == path[0]);
                    if (everestModule != null)
                    {
                        Assembly asm = everestModule.GetType().Assembly;
                        Type[] types = asm.GetTypes();
                        foreach (Type type in types)
                        {
                            if (type.Name == className)
                            {
                                MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                                if (method != null && method.ReturnType == typeof(bool))
                                {
                                    ParameterInfo[] args = method.GetParameters();
                                    if (args.Length == 1 && args[0].ParameterType == typeof(Level))
                                    {
                                        _conditionMethod = method;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (_conditionMethod == null && !string.IsNullOrEmpty(ConditionFunction))
            {
                AquaDebugger.LogWarning("The ConditionFunction Parameter may be incorrect. Make sure the format is 'mod:ModName/ClassName/FunctionName' and the function must be static, boolean return and accept a Level type parameter.");
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            TryTrig();
        }

        public override void OnStay(Player player)
        {
            if (!_trigged)
            {
                TryTrig();
            }
        }

        public override void OnLeave(Player player)
        {
            _trigged = false;
        }

        private void TryTrig()
        {
            AquaBird bird = FindBird(BirdID);
            if (bird != null)
            {
                bool cond = _conditionMethod == null || (bool)_conditionMethod.Invoke(null, new object[] { Scene });
                if (cond)
                {
                    if (TutorialIndex >= 0)
                    {
                        bird.TriggerShowTutorial(TutorialIndex);
                    }
                    else
                    {
                        bird.TriggerCloseTutorial();
                    }
                    _trigged = true;
                }
            }
        }

        private AquaBird FindBird(string birdId)
        {
            List<Entity> birds = Scene.Tracker.GetEntities<AquaBird>();
            foreach (AquaBird bird in birds)
            {
                if (bird.BirdID == birdId)
                {
                    return bird;
                }
            }
            return null;
        }

        private MethodInfo _conditionMethod;
        private bool _trigged = false;
    }
}
