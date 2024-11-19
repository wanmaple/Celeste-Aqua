using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Freeze Listener")]
    public class FreezeListener : Entity
    {
        public string[] ListenInputs { get; private set; }

        public FreezeListener(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            string inputsText = data.Attr("inputs");
            if (!string.IsNullOrEmpty(inputsText))
            {
                ListenInputs = inputsText.Split(',');
                for (int i = 0; i < ListenInputs.Length; i++)
                {
                    ListenInputs[i] = ListenInputs[i].Trim();
                }
            }
            AddTag(Tags.FrozenUpdate); 
        }

        public override void Update()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            Level level = player.Scene as Level;
            if (player.CollideCheck(this) && !_playerEnter)
            {
                level.Frozen = true;
                _playerEnter = true;
            }
            else if (_playerEnter && CheckInputs())
            {
                level.Frozen = false;
                RemoveSelf();
            }
        }

        private bool CheckInputs()
        {
            bool ret = true;
            foreach (string identifier in ListenInputs)
            {
                switch (identifier)
                {
                    case "Up":
                        ret = ret && Input.MoveY.Value < 0;
                        break;
                    case "Down":
                        ret = ret && Input.MoveY.Value > 0;
                        break;
                    case "Left":
                        ret = ret && Input.MoveX.Value < 0;
                        break;
                    case "Right":
                        ret = ret && Input.MoveX.Value > 0;
                        break;
                    case "UpLeft":
                        ret = ret && Input.MoveX.Value < 0 && Input.MoveY.Value < 0;
                        break;
                    case "UpRight":
                        ret = ret && Input.MoveX.Value > 0 && Input.MoveY.Value < 0;
                        break;
                    case "DownLeft":
                        ret = ret && Input.MoveX.Value < 0 && Input.MoveY.Value > 0;
                        break;
                    case "DownRight":
                        ret = ret && Input.MoveX.Value > 0 && Input.MoveY.Value > 0;
                        break;
                    case "Dash":
                        ret = ret && Input.Dash.Check;
                        break;
                    case "Jump":
                        ret = ret && Input.Jump.Check;
                        break;
                    case "Grab":
                        ret = ret && Input.Grab.Check;
                        break;
                    default:
                        if (identifier.StartsWith("mod:"))
                        {
                            string[] modKeyInfo = identifier.Substring(4).Split('/');
                            EverestModule everestModule = Enumerable.FirstOrDefault(Everest.Modules, (EverestModule m) => m.Metadata.Name == modKeyInfo[0]);
                            if (everestModule?.SettingsType != null)
                            {
                                ButtonBinding modBinding = everestModule.SettingsType.GetProperty(modKeyInfo[1])?.GetGetMethod()?.Invoke(everestModule._Settings, null) as ButtonBinding;
                                if (modBinding != null)
                                {
                                    ret = ret && modBinding.Check;
                                }
                            }
                        }
                        break;
                }
            }
            return ret;
        }

        private bool _playerEnter = false;
    }
}
