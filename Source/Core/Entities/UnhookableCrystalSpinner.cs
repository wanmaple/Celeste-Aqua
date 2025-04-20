using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Unhookable Crystal")]
    [Tracked(true)]
    public class UnhookableCrystalSpinner : CrystalStaticSpinner
    {
        public Color MainColor { get; private set; }
        public string CustomForegroundTexture { get; private set; }
        public string CustomBackgroundTexture { get; private set; }

        public int ID => (int)_fieldID.GetValue(this);

        public UnhookableCrystalSpinner(EntityData data, Vector2 offset)
            : base(data, offset, CrystalColor.Purple)
        {
            MainColor = data.HexColor("color");
            CustomForegroundTexture = data.Attr("custom_foreground_texture");
            CustomBackgroundTexture = data.Attr("custom_background_texture");
            this.SetHookable(true);
            Add(new HookInteractable(OnHookInteract));

            if (_fieldID == null)
            {
                _fieldID = typeof(CrystalStaticSpinner).GetField("ID", BindingFlags.Instance | BindingFlags.NonPublic);
            }
        }

        public override void Awake(Scene scene)
        {
            foreach (var com in Components)
            {
                com.EntityAwake();
            }

            if (InView())
            {
                CreateSpritesEx();
            }
        }

        public override void Update()
        {
            foreach (Component com in Components)
            {
                com.Update();
            }
            if (!Visible)
            {
                Collidable = false;
                if (InView())
                {
                    Visible = true;
                    if (!expanded)
                    {
                        CreateSpritesEx();
                    }
                }
            }
            else
            {
                if (Scene.OnInterval(0.25f, offset) && !InView())
                {
                    Visible = false;
                }

                if (Scene.OnInterval(0.05f, offset))
                {
                    Player entity = Scene.Tracker.GetEntity<Player>();
                    if (entity != null)
                    {
                        Collidable = Math.Abs(entity.X - X) < 128f && Math.Abs(entity.Y - Y) < 128f;
                    }
                }
            }

            if (filler != null)
            {
                filler.Position = Position;
            }
        }

        private bool OnHookInteract(GrapplingHook grapple, Vector2 at)
        {
            Audio.Play("event:/char/madeline/unhookable", grapple.Position);
            grapple.Revoke();
            return true;
        }

        private void CreateSpritesEx()
        {
            if (expanded)
            {
                return;
            }

            Calc.PushRandom(randomSeed);
            string fgTextureName = !string.IsNullOrEmpty(CustomForegroundTexture) ? CustomForegroundTexture : fgTextureLookup[CrystalColor.Rainbow];
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(fgTextureName);
            MTexture mTexture = Calc.Random.Choose(atlasSubtextures);
            Color color = MainColor;
            if (!SolidCheck(new Vector2(X - 4f, Y - 4f)))
            {
                Add(new Image(mTexture.GetSubtexture(0, 0, 14, 14)).SetOrigin(12f, 12f).SetColor(color));
            }
            if (!SolidCheck(new Vector2(X + 4f, Y - 4f)))
            {
                Add(new Image(mTexture.GetSubtexture(10, 0, 14, 14)).SetOrigin(2f, 12f).SetColor(color));
            }
            if (!SolidCheck(new Vector2(X + 4f, Y + 4f)))
            {
                Add(new Image(mTexture.GetSubtexture(10, 10, 14, 14)).SetOrigin(2f, 2f).SetColor(color));
            }
            if (!SolidCheck(new Vector2(X - 4f, Y + 4f)))
            {
                Add(new Image(mTexture.GetSubtexture(0, 10, 14, 14)).SetOrigin(12f, 2f).SetColor(color));
            }

            foreach (CrystalStaticSpinner entity in Scene.Tracker.GetEntities<CrystalStaticSpinner>())
            {
                if ((int)_fieldID.GetValue(entity) > ID && entity.AttachToSolid == AttachToSolid && (entity.Position - Position).LengthSquared() < 576f)
                {
                    AddSpriteEx((Position + entity.Position) / 2f - Position);
                }
            }
            foreach (UnhookableCrystalSpinner entity in Scene.Tracker.GetEntities<UnhookableCrystalSpinner>())
            {
                if (entity.ID > ID && entity.AttachToSolid == AttachToSolid && (entity.Position - Position).LengthSquared() < 576f)
                {
                    AddSpriteEx((Position + entity.Position) / 2f - Position);
                }
            }
            Scene.Add(border = new Border(this, filler));
            expanded = true;
            Calc.PopRandom();
        }

        private void AddSpriteEx(Vector2 offset)
        {
            if (filler == null)
            {
                Scene.Add(filler = new Entity(Position));
                filler.Depth = Depth + 1;
            }

            string bgTextureName = !string.IsNullOrEmpty(CustomBackgroundTexture) ? CustomBackgroundTexture : bgTextureLookup[CrystalColor.Rainbow];
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(bgTextureName);
            Image image = new Image(Calc.Random.Choose(atlasSubtextures));
            image.Position = offset;
            image.Rotation = Calc.Random.Choose(0, 1, 2, 3) * (MathF.PI / 2f);
            image.CenterOrigin();
            image.Color = MainColor;
            filler.Add(image);
        }

        private static FieldInfo _fieldID;
    }
}
