using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.IO;

namespace Celeste.Mod.Aqua.Core
{
    public static class PresentationHook
    {
        public static void Initialize()
        {
            On.Celeste.Level.UnloadLevel += Level_UnloadLevel;
            On.Monocle.Engine.Update += Engine_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.Level.UnloadLevel -= Level_UnloadLevel;
            On.Monocle.Engine.Update -= Engine_Update;
        }

        private static void Level_UnloadLevel(On.Celeste.Level.orig_UnloadLevel orig, Level self)
        {
            orig(self);
            PresentationRecorder.Instance.CancelRecording();
        }

        private static void Engine_Update(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime)
        {
            orig(self, gameTime);
#if DEBUG
            if (self.scene is Level level)
            {
                if (MInput.Keyboard.Pressed(Keys.Insert))
                {
                    if (!PresentationRecorder.Instance.IsRecording)
                        PresentationRecorder.Instance.BeginRecording(level);
                    else
                    {
                        _tempData = PresentationRecorder.Instance.EndRecording();
                        _tempData.Trim();
                    }
                }
                else if (MInput.Keyboard.Pressed(Keys.Delete))
                {
                    PresentationRecorder.Instance.CancelRecording();
                }
                if (MInput.Keyboard.Pressed(Keys.Home) && _tempData != null)
                {
                    level.Add(new PresentationController(_tempData, Calc.HexToColor("a42d2d"), Calc.HexToColor("ffff00"), true, 0.1f, 0.45f, 1.0f));
                }
                else if (MInput.Keyboard.Pressed(Keys.End))
                {
                    PresentationController controller = level.Tracker.GetEntity<PresentationController>();
                    if (controller != null)
                    {
                        controller.RemoveSelf();
                    }
                }
                if (_tempData != null)
                {
                    if (MInput.Keyboard.Pressed(Keys.Add) || MInput.Keyboard.Pressed(Keys.Subtract))
                    {
                        PresentationController controller = level.Tracker.GetEntity<PresentationController>();
                        if (controller != null)
                        {
                            controller.RemoveSelf();
                        }
                        if (_tempData.Frames.Count > 0)
                        {
                            if (MInput.Keyboard.Pressed(Keys.Add))
                                _tempData.Frames.RemoveAt(0);
                            if (MInput.Keyboard.Pressed(Keys.Subtract))
                                _tempData.Frames.RemoveAt(_tempData.Frames.Count - 1);
                        }
                    }
                    else if (MInput.Keyboard.Pressed(Keys.P))
                    {
                        string folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string filename = DateTime.Now.ToString("yyyyMMddhhmmss") + ".bin";
                        string path = Path.Combine(folder, filename);
                        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                        {
                            using (var bw = new BinaryWriter(fs))
                            {
                                _tempData.Serialize(bw);
                            }
                        }
                    }
                }

                PresentationRecorder.Instance.Update(level);
            }
#endif
        }

        private static PresentationData _tempData;
    }
}
