using Celeste.Mod.Aqua.Rendering;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Rod Zip Mover")]
    [Tracked(false)]
    public class RodZipMover : ZipMover, IRodControllable, ICustomRenderEntity
    {
        public string Flag { get; private set; }
        public float Duration { get; private set; }
        public float HueOffset { get; private set; }
        public float SaturationOffset { get; private set; }
        public bool State { get; set; }
        public bool IsRunning { get; private set; }

        public RodZipMover(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Flag = data.Attr("flag");
            Duration = data.Has("duration") ? Calc.Max(data.Float("duration"), 0.1f) : 0.5f;
            HueOffset = data.Float("hue_offset");
            SaturationOffset = data.Float("saturation_offset");
            Coroutine coroutine = Get<Coroutine>();
            Remove(coroutine);
            Add(new Coroutine(RodSequence()));
            Add(new BeforeRenderHook(BeforeRender));
        }

        public Effect GetEffect()
        {
            return RenderInfoStorage.Instance.GetEffect(this.GetUniqueID());
        }

        public void OnReload()
        {
            var fx = RenderInfoStorage.Instance.CreateEffect(this.GetUniqueID(), "hue_offset");
            if (fx != null)
            {
                fx.Parameters["HueOffset"].SetValue(HueOffset);
                fx.Parameters["SaturationOffset"].SetValue(SaturationOffset);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            RodEntityManager.Instance.Add(this);
            var fx = RenderInfoStorage.Instance.CreateEffect(this.GetUniqueID(), "hue_offset");
            if (fx != null)
            {
                fx.Parameters["HueOffset"].SetValue(HueOffset);
                fx.Parameters["SaturationOffset"].SetValue(SaturationOffset);
            }
            RenderInfoStorage.Instance.CreateSimpleRenderTarget(this.GetUniqueID(), (int)Width, (int)Height);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            RodEntityManager.Instance.Remove(this);
            RenderInfoStorage.Instance.ReleaseAll(this.GetUniqueID());
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            RodEntityManager.Instance.Remove(this);
            RenderInfoStorage.Instance.ReleaseAll(this.GetUniqueID());
        }

        public override void Update()
        {
            _lastState = State;
            streetlight.SetAnimationFrame(IsRunning ? 3 : 1);
            if (!IsRunning && SceneAs<Level>().Session.GetFlag(Flag))
            {
                State = !State;
            }
            PatchSpeedRunner();
            base.Update();
        }

        public override void Render()
        {
            Vector2 position = Position;
            Position += Shake;
            var rt = RenderInfoStorage.Instance.GetSimpleRenderTarget(this.GetUniqueID());
            if (rt != null)
            {
                Draw.SpriteBatch.Draw(rt, Position, Color.White);
            }
            Position = position;
        }

        private void BeforeRender()
        {
            Effect fx = RenderInfoStorage.Instance.GetEffect(this.GetUniqueID());
            RenderTarget2D rt = RenderInfoStorage.Instance.GetSimpleRenderTarget(this.GetUniqueID());
            if (fx != null && rt != null)
            {
                var device = Draw.SpriteBatch.GraphicsDevice;
                device.SetRenderTarget(rt);
                device.Clear(Color.Black);
                Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, fx, Matrix.Identity);
                RenderRT();
                Draw.SpriteBatch.End();
            }
        }

        private void RenderRT()
        {
            Draw.Rect(1f, 1f, Width - 2f, Height - 2f, Color.Black);
            int num = 1;
            float num2 = 0f;
            int count = innerCogs.Count;
            for (int i = 4; (float)i <= Height - 4f; i += 8)
            {
                int num3 = num;
                for (int j = 4; (float)j <= Width - 4f; j += 8)
                {
                    int index = (int)(mod((num2 + (float)num * percent * MathF.PI * 4f) / (MathF.PI / 2f), 1f) * (float)count);
                    MTexture mTexture = innerCogs[index];
                    Rectangle rectangle = new Rectangle(0, 0, mTexture.Width, mTexture.Height);
                    Vector2 zero = Vector2.Zero;
                    if (j <= 4)
                    {
                        zero.X = 2f;
                        rectangle.X = 2;
                        rectangle.Width -= 2;
                    }
                    else if ((float)j >= Width - 4f)
                    {
                        zero.X = -2f;
                        rectangle.Width -= 2;
                    }

                    if (i <= 4)
                    {
                        zero.Y = 2f;
                        rectangle.Y = 2;
                        rectangle.Height -= 2;
                    }
                    else if ((float)i >= Height - 4f)
                    {
                        zero.Y = -2f;
                        rectangle.Height -= 2;
                    }

                    mTexture = mTexture.GetSubtexture(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, temp);
                    mTexture.DrawCentered(new Vector2(j, i) + zero, Color.White * ((num < 0) ? 0.5f : 1f));
                    num = -num;
                    num2 += MathF.PI / 3f;
                }

                if (num3 == num)
                {
                    num = -num;
                }
            }

            for (int k = 0; (float)k < Width / 8f; k++)
            {
                for (int l = 0; (float)l < Height / 8f; l++)
                {
                    int num4 = ((k != 0) ? (((float)k != Width / 8f - 1f) ? 1 : 2) : 0);
                    int num5 = ((l != 0) ? (((float)l != Height / 8f - 1f) ? 1 : 2) : 0);
                    if (num4 != 1 || num5 != 1)
                    {
                        edges[num4, num5].Draw(new Vector2((float)(k * 8), (float)(l * 8)));
                    }
                }
            }

            if (streetlight.Texture != null)
            {
                streetlight.Texture.Draw(streetlight.Position, streetlight.Origin, Color.White);
            }
        }

        private IEnumerator RodSequence()
        {
            while (true)
            {
                if (!IsRunning && _lastState != State)
                {
                    IsRunning = true;
                    sfx.Play((theme == Themes.Normal) ? "event:/game/01_forsaken_city/zip_mover_no_return" : "event:/new_content/game/10_farewell/zip_mover_no_return");
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                    StartShaking(0.1f);
                    yield return 0.1f;
                    StopPlayerRunIntoAnimation = false;
                    Vector2 begin = State ? target : start;
                    Vector2 end = State ? start : target;
                    float at2 = 0.0f;
                    while (at2 < Duration)
                    {
                        yield return null;
                        at2 = Calc.Approach(at2, Duration, Engine.DeltaTime);
                        percent = Ease.SineIn(at2 / Duration);
                        Vector2 vector = Vector2.Lerp(end, begin, percent);
                        ScrapeParticlesCheck(vector);
                        if (Scene.OnInterval(0.1f))
                        {
                            pathRenderer.CreateSparks();
                        }
                        MoveTo(vector);
                    }
                    StopPlayerRunIntoAnimation = true;
                    yield return 0.2f;
                    SceneAs<Level>().Session.SetFlag(Flag, false);
                    IsRunning = false;
                }
                yield return null;
            }
        }

        private void PatchSpeedRunner()
        {
            // This is a patch to SpeedRunner, IDK if there is a better solution though since loading only trigger the Removed not Added.
            Effect fx = RenderInfoStorage.Instance.GetEffect(this.GetUniqueID());
            if (fx == null)
            {
                fx = RenderInfoStorage.Instance.CreateEffect(this.GetUniqueID(), "hue_offset");
                if (fx != null)
                {
                    fx.Parameters["HueOffset"].SetValue(HueOffset);
                    fx.Parameters["SaturationOffset"].SetValue(SaturationOffset);
                }
            }
            RenderTarget2D rt = RenderInfoStorage.Instance.GetSimpleRenderTarget(this.GetUniqueID());
            if (rt == null)
            {
                RenderInfoStorage.Instance.CreateSimpleRenderTarget(this.GetUniqueID(), (int)Width, (int)Height);
            }
        }

        private bool _lastState = false;
    }
}
