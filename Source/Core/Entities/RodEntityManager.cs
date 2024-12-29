using Celeste.Mod.Aqua.Miscellaneous;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public class RodEntityManager : Entity
    {
        public static RodEntityManager Instance => _instance;

        private static RodEntityManager _instance;

        public RodEntityManager()
        {
            _instance = this;
        }

        public IReadOnlyList<IRodControllable> GetEntitiesOfFlag(string flag)
        {
            if (_rodEntities.TryGetValue(flag, out List<IRodControllable> entities))
            {
                return entities;
            }
            return null;
        }

        public void Add(IRodControllable entity)
        {
            if (!_rodEntities.TryGetValue(entity.Flag, out List<IRodControllable> entities))
            {
                entities = new List<IRodControllable>();
                _rodEntities.Add(entity.Flag, entities);
            }
            entities.Add(entity);
        }

        public void Remove(IRodControllable entity)
        {
            if (_rodEntities.TryGetValue(entity.Flag, out List<IRodControllable> entities))
            {
                entities.RemoveFast(entity);
                if (entities.Count == 0)
                {
                    _rodEntities.Remove(entity.Flag);
                }
            }
        }

        private Dictionary<string, List<IRodControllable>> _rodEntities = new Dictionary<string, List<IRodControllable>>(16);
    }
}
