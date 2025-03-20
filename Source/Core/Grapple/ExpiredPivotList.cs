using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.Aqua.Core
{
    public class ExpiredPivotList
    {
        public IReadOnlyList<RopePivot> Pivots => _pivots;

        public void RecordPivots(IEnumerable<RopePivot> pivots, Vector2 playerPrevPos)
        {
            _pivots.Clear();
            _pivots.AddRange(pivots);
            _pivots.Add(new RopePivot(playerPrevPos, Cornors.Free, null));
            _indexes.Clear();
            for (int i = 1; i <= pivots.Count(); i++)
            {
                _indexes.Add(i);
            }
        }

        public List<Segment> GetSegments(int index)
        {
            List<Segment> segments = new List<Segment>(_indexes[index]);
            int prev = index == 0 ? 0 : _indexes[index - 1];
            for (int i = 0; i < _indexes[index] - prev; i++)
            {
                int prevIdx = prev + i;
                int nextIdx = prevIdx + 1;
                Vector2 prevPos = _pivots[prevIdx].point;
                Vector2 nextPos = _pivots[nextIdx].point;
                Segment seg = new Segment(prevPos, nextPos);
                segments.Add(seg);
            }
            return segments;
        }

        public RopePivot GetPivotLast(int index)
        {
            int idx = _indexes[index];
            return _pivots[idx - 1];
        }

        public void Decrease(int index)
        {
            _indexes.RemoveAt(index);
        }

        public bool CheckPivotNum(int num)
        {
            return _indexes.Count == num;
        }

        public void Dump()
        {
            AquaDebugger.LogInfo("PVTS {0}", string.Join(',', _pivots));
            AquaDebugger.LogInfo("IDX {0}", string.Join(',', _indexes));
        }

        private List<RopePivot> _pivots = new List<RopePivot>(8);
        private List<int> _indexes = new List<int>(8);
    }
}
