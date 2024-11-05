using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core.Hook
{
    public class ExpiredPivotList
    {
        public void RecordPivots(IEnumerable<RopePivot> pivots, Vector2 playerPrevPos)
        {
            _pivots.Clear();
            _pivots.AddRange(pivots);
            _pivots.Add(new RopePivot(playerPrevPos, Cornors.Free, null));
            _indexes.Clear();
        }

        public List<Segment> GetSegments(int index)
        {
            if (index >= _indexes.Count)
            {

            }
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

        public void Increase(int index)
        {
            if (index >= _indexes.Count)
            {
                int num = index == 0 ? 1 : _indexes[index - 1] + 1;
                _indexes.Add(num);
            }
            else
            {
                for (int i = index; i < _indexes.Count; i++)
                {
                    _indexes[i]++;
                }
            }
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
