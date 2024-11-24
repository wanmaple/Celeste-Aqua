using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.Aqua.Debug
{
    public class ConditionTrigger : IDisposable
    {
        public bool IsSatisfied => _satisfied;

        public ConditionTrigger(bool condition = false)
        {
            _satisfied = condition;
        }

        public void Trigger()
        {
            _satisfied = true;
        }

        public void Dispose()
        {
            _satisfied = false;
        }

        private bool _satisfied;
    }
}
