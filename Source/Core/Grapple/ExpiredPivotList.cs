using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.Aqua.Core
{
    public struct RopeSegment
    {
        public RopePivot Pivot1 { get; set; }
        public RopePivot Pivot2 { get; set; }

        public Vector2 Vector => Pivot2.point - Pivot1.point;
        public Vector2 Direction => Calc.SafeNormalize(Vector);
        public float Length => Vector.Length();

        public RopeSegment(RopePivot pt1, RopePivot pt2)
        {
            Pivot1 = pt1;
            Pivot2 = pt2;
        }
    }

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

        public void GetSegments(int index, List<RopeSegment> segments)
        {
            segments.Clear();
            int prev = index == 0 ? 0 : _indexes[index - 1];
            for (int i = 0; i < _indexes[index] - prev; i++)
            {
                int prevIdx = prev + i;
                int nextIdx = prevIdx + 1;
                RopePivot prevPivot = _pivots[prevIdx];
                RopePivot nextPivot = _pivots[nextIdx];
                RopeSegment seg = new RopeSegment(prevPivot, nextPivot);
                segments.Add(seg);
            }
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
