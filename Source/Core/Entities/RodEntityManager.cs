using Celeste.Mod.Aqua.Miscellaneous;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public class RodEntityManager
    {
        public static RodEntityManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RodEntityManager();
                return _instance;
            }
        }

        private static RodEntityManager _instance;

        private RodEntityManager()
        {
        }

        public IReadOnlyList<IRodControllable> GetEntitiesOfGroup(int group)
        {
            if (_rodEntities.TryGetValue(group, out List<IRodControllable> entities))
            {
                return entities;
            }
            return null;
        }

        public void Add(IRodControllable entity)
        {
            if (!_rodEntities.TryGetValue(entity.Group, out List<IRodControllable> entities))
            {
                entities = new List<IRodControllable>();
                _rodEntities.Add(entity.Group, entities);
            }
            entities.Add(entity);
        }

        public void Remove(IRodControllable entity)
        {
            if (_rodEntities.TryGetValue(entity.Group, out List<IRodControllable> entities))
            {
                entities.RemoveFast(entity);
                if (entities.Count == 0)
                {
                    _rodEntities.Remove(entity.Group);
                }
            }
        }

        private Dictionary<int, List<IRodControllable>> _rodEntities = new Dictionary<int, List<IRodControllable>>(16);
    }
}
