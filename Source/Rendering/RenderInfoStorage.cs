using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Rendering
{
    public class RenderInfoStorage
    {
        public class RenderInfo : IDisposable
        {
            public RenderTarget2D RenderTarget;
            public Effect Effect;

            public void Dispose()
            {
                if (RenderTarget != null)
                    RenderTarget.Dispose();
                if (Effect != null)
                    Effect.Dispose();
            }
        }

        public static RenderInfoStorage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RenderInfoStorage();
                return _instance;
            }
        }

        private static RenderInfoStorage _instance;

        private RenderInfoStorage()
        {
            _cache = new Dictionary<ulong, RenderInfo>(32);
        }

        public RenderTarget2D GetSimpleRenderTarget(ulong id)
        {
            if (!_cache.TryGetValue(id, out var info))
            {
                return null;
            }
            return info.RenderTarget;
        }

        public RenderTarget2D CreateSimpleRenderTarget(ulong id, int width, int height)
        {
            if (!_cache.TryGetValue(id, out RenderInfo info))
            {
                var rt = new RenderTarget2D(Draw.SpriteBatch.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
                info = new RenderInfo { RenderTarget = rt, };
                _cache.Add(id, info);
                return rt;
            }
            else if (info.RenderTarget == null)
            {
                var rt = new RenderTarget2D(Draw.SpriteBatch.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
                info.RenderTarget = rt;
                return rt;
            }
            else if (info.RenderTarget.Width != width || info.RenderTarget.Height != height)
            {
                info.RenderTarget.Dispose();
                var rt = new RenderTarget2D(Draw.SpriteBatch.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
                info.RenderTarget = rt;
                return rt;
            }
            return info.RenderTarget;
        }

        public void ReleaseSimpleRenderTarget(ulong id)
        {
            if (_cache.TryGetValue(id, out RenderInfo info))
            {
                if (info.RenderTarget != null)
                    info.RenderTarget.Dispose();
                if (info.Effect == null)
                    _cache.Remove(id);
            }
        }

        public Effect GetEffect(ulong id)
        {
            if (!_cache.TryGetValue(id, out RenderInfo info))
            {
                return null;
            }
            return info.Effect;
        }

        public Effect CreateEffect(ulong id, string effName)
        {
            if (!_cache.TryGetValue(id, out RenderInfo info))
            {
                var eff = FXCenter.Instance.GetFX(effName);
                if (eff != null)
                {
                    info = new RenderInfo { Effect = eff, };
                    _cache.Add(id, info);
                }
                return eff;
            }
            else
            {
                if (info.Effect != null)
                    info.Effect.Dispose();
                var eff = FXCenter.Instance.GetFX(effName);
                info.Effect = eff;
                return eff;
            }
        }

        public void ReleaseEffect(ulong id)
        {
            if (_cache.TryGetValue(id, out RenderInfo info))
            {
                if (info.Effect != null)
                    info.Effect.Dispose();
                if (info.RenderTarget == null)
                    _cache.Remove(id);
            }
        }

        public void ReleaseAll(ulong id)
        {
            if (_cache.TryGetValue(id, out RenderInfo info))
            {
                info.Dispose();
                _cache.Remove(id);
            }
        }

        public void Clear()
        {
            foreach (var pair in _cache)
            {
                pair.Value.Dispose();
            }
            _cache.Clear();
        }

        private Dictionary<ulong, RenderInfo> _cache;
    }
}
