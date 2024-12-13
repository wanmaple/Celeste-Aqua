using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Rendering
{
    public class FXCenter
    {
        public static FXCenter Instance
        {
            get
            {
                if (_instance ==null)
                    _instance = new FXCenter ();
                return _instance;
            }
        }

        private static FXCenter _instance;

        private FXCenter()
        {
        }

        public void PrepareLoad(string fxName, ModAsset asset)
        {
            _toLoad[fxName] = asset;
        }

        public Effect GetFX(string fxName)
        {
            if (_toLoad.TryGetValue(fxName, out ModAsset asset))
            {
                Effect eff = new Effect(Engine.Instance.GraphicsDevice, asset.Data);
                _loaded.Add(fxName, eff);
                _toLoad.Remove(fxName);
            }
            if (_loaded.TryGetValue(fxName, out Effect fx))
            {
                return fx;
            }
            return null;
        }

        private Dictionary<string, ModAsset> _toLoad = new Dictionary<string, ModAsset>(32);
        private Dictionary<string, Effect> _loaded = new Dictionary<string, Effect>(32);
    }
}
