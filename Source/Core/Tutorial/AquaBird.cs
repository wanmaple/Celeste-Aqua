using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Aqua Bird")]
    [Tracked(true)]
    public class AquaBird : BirdNPC
    {
        public string BirdID { get; set; }
        public new bool Caw { get; set; }
        public int StartupIndex { get; set; }
        public bool TriggerOnce { get; set; } = true;

        private static readonly Dictionary<string, Vector2> DIRECTIONS = new Dictionary<string, Vector2>
        {
            {
                "Left",
                new Vector2(-1f, 0f)
            },
            {
                "Right",
                new Vector2(1f, 0f)
            },
            {
                "Up",
                new Vector2(0f, -1f)
            },
            {
                "Down",
                new Vector2(0f, 1f)
            },
            {
                "UpLeft",
                new Vector2(-1f, -1f)
            },
            {
                "UpRight",
                new Vector2(1f, -1f)
            },
            {
                "DownLeft",
                new Vector2(-1f, 1f)
            },
            {
                "DownRight",
                new Vector2(1f, 1f)
            }
        };

        static AquaBird()
        {
            _parseCommandMethod = typeof(Everest.Events.CustomBirdTutorial).GetMethod("ParseCommand", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public AquaBird(EntityData data, Vector2 offset) 
            : base(data, offset)
        {
            BirdID = data.Attr("birdId");
            StartupIndex = data.Int("startupIndex");
            TriggerOnce = data.Bool("triggerOnce");
            Caw = data.Bool("caw");
            Facing = ((!data.Bool("faceLeft")) ? Facings.Right : Facings.Left);
            onlyOnce = data.Bool("onlyOnce");
            Sprite.Scale.X = (float)Facing;
            string dialogsText = data.Attr("dialogs");
            string controlsText = data.Attr("controls");
            if (!string.IsNullOrEmpty(dialogsText) && !string.IsNullOrEmpty(controlsText))
            {
                string[] dialogIds = dialogsText.Split(';');
                string[] controls = controlsText.Split(';');
                for (int i = 0; i < Math.Min(dialogIds.Length, controls.Length); i++)
                {
                    BuildGUI(dialogIds[i], controls[i]);
                }
            }
            AddTag(Tags.FrozenUpdate);
        }

        public bool IsTutorialTrig(int index)
        {
            if (index > 0 && index < _trigs.Count)
            {
                return _trigs[index];
            }
            return false;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Enumerable.All(Enumerable.OfType<AquaBirdTrigger>(scene.Tracker.GetEntities<AquaBirdTrigger>()), (AquaBirdTrigger trigger) => trigger.TutorialIndex < 0 || trigger.BirdID != BirdID))
            {
                TriggerShowTutorial(StartupIndex);
            }
        }

        public void TriggerShowTutorial(int index)
        {
            if (_currentActiveIndex == index)
                return;

            if (index >= 0 && index < _guis.Count && (!TriggerOnce || !_trigs[index]))
            {
                _trigs[index] = true;
                _currentActiveIndex = index;
                Add(new Coroutine(ChangeTutorial(_guis[index], Caw), true));
            }
            else
            {
                _currentActiveIndex = -1;
                Add(new Coroutine(HideTutorial()));
            }
        }

        public void TriggerCloseTutorial()
        {
            if (!_flewAway)
            {
                _flewAway = true;
                if (_currentActiveIndex >= 0)
                {
                    Add(new Coroutine(HideTutorial()));
                }

                Add(new Coroutine(StartleAndFlyAway()));
                if (onlyOnce)
                {
                    SceneAs<Level>().Session.DoNotLoad.Add(EntityID);
                }
            }
        }

        private void BuildGUI(string dialogId, string control)
        {
            object info = ((!GFX.Gui.Has(dialogId)) ? ((object)Dialog.Clean(dialogId)) : ((object)GFX.Gui[dialogId]));
            int num = 0;
            string[] identifiers = control.Split(',');
            object[] translations = new object[identifiers.Length];
            for (int i = 0; i < translations.Length; i++)
            {
                string identifier = identifiers[i];
                object obj = _parseCommandMethod.Invoke(null, new object[] { identifier, });
                ButtonBinding binding = obj as ButtonBinding;
                if (binding != null)
                {
                    obj = binding.Button;
                }

                if (obj != null)
                {
                    translations[i] = obj;
                }
                else if (identifier.StartsWith("mod:"))
                {
                    string[] autoBinding = identifier.Substring(4).Split('/');
                    EverestModule everestModule = Enumerable.FirstOrDefault(Everest.Modules, (EverestModule m) => m.Metadata.Name == autoBinding[0]);
                    if (everestModule?.SettingsType != null)
                    {
                        ButtonBinding modBinding = everestModule.SettingsType.GetProperty(autoBinding[1])?.GetGetMethod()?.Invoke(everestModule._Settings, null) as ButtonBinding;
                        if (modBinding?.Button != null)
                        {
                            translations[i] = modBinding.Button;
                        }
                        else
                        {
                            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 2);
                            defaultInterpolatedStringHandler.AppendLiteral("Public ButtonBinding property not found in ");
                            defaultInterpolatedStringHandler.AppendFormatted(everestModule.SettingsType);
                            defaultInterpolatedStringHandler.AppendLiteral(". ControlString: ");
                            defaultInterpolatedStringHandler.AppendFormatted(identifier);
                            AquaDebugger.LogWarning(defaultInterpolatedStringHandler.ToStringAndClear());
                        }
                    }
                    else
                    {
                        AquaDebugger.LogWarning("EverestModule or EverestModule.SettingsType not found. ControlString: " + identifier);
                    }
                }
                else if (GFX.Gui.Has(identifier))
                {
                    translations[i] = GFX.Gui[identifier];
                }
                else if (DIRECTIONS.ContainsKey(identifier))
                {
                    translations[i] = DIRECTIONS[identifier];
                }
                else
                {
                    FieldInfo field = typeof(Input)!.GetField(identifier, BindingFlags.Static | BindingFlags.Public);
                    if (field?.GetValue(null)?.GetType() == typeof(VirtualButton))
                    {
                        translations[i] = field.GetValue(null);
                    }
                    else if (identifier.StartsWith("dialog:"))
                    {
                        translations[i] = Dialog.Clean(identifier.Substring("dialog:".Length));
                    }
                    else
                    {
                        translations[i] = identifier;
                    }
                }

                if (translations[i] is string)
                {
                    num++;
                    if (i == 0)
                    {
                        num++;
                    }
                }
            }

            var gui = new BirdTutorialGui(this, new Vector2(0f, -16f), info, translations);
            DynData<BirdTutorialGui> dynData = new DynData<BirdTutorialGui>(gui);
            if (string.IsNullOrEmpty(dialogId))
            {
                dynData["infoHeight"] = 0f;
            }

            dynData["controlsWidth"] = dynData.Get<float>("controlsWidth") + (float)num;
            _guis.Add(gui);
            _trigs.Add(false);
        }

        private IEnumerator ChangeTutorial(BirdTutorialGui gui, bool caw = false)
        {
            yield return HideTutorial();

            if (caw)
            {
                yield return Caw();
            }

            if (this.gui != null)
                this.gui.Open = false;
            this.gui = gui;
            gui.Open = true;
            Scene.Add(gui);
            while (gui.Scale < 1f)
            {
                yield return null;
            }
        }

        private new IEnumerator HideTutorial()
        {
            if (gui != null)
            {
                gui.Open = false;
                while (gui.Scale > 0f)
                {
                    yield return null;
                }

                Scene.Remove(gui);
                gui = null;
            }
        }

        private List<BirdTutorialGui> _guis = new List<BirdTutorialGui>(4);
        private List<bool> _trigs = new List<bool>(4);
        private int _currentActiveIndex = -1;
        private bool _flewAway = false;

        private static MethodInfo _parseCommandMethod;
    }
}
