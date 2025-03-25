using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace Celeste.Mod.Aqua.Debug
{
    public class GrappleUnitTest
    {
        public List<Vector2> PreviousPivots;
        public Vector2 CurrentPivot1;
        public Vector2 CurrentPivot2;
        public Vector2 PreviousPoint;
        public Vector2 CurrentPoint;

        public void Write(Stream stream)
        {
            using (var sw = new StreamWriter(stream))
            {
                sw.WriteLine("PreviousPivots:");
                for (int i = 0; i < PreviousPivots.Count; i++)
                {
                    sw.Write("  ");
                    sw.Write("- X: ");
                    sw.WriteLine(PreviousPivots[i].X);
                    sw.Write("    ");
                    sw.Write("Y: ");
                    sw.WriteLine(PreviousPivots[i].Y);
                }
                sw.WriteLine("CurrentPivot1:");
                sw.Write("  X: ");
                sw.WriteLine(CurrentPivot1.X);
                sw.Write("  Y: ");
                sw.WriteLine(CurrentPivot1.Y);
                sw.WriteLine("CurrentPivot2:");
                sw.Write("  X: ");
                sw.WriteLine(CurrentPivot2.X);
                sw.Write("  Y: ");
                sw.WriteLine(CurrentPivot2.Y);
                sw.WriteLine("PreviousPoint:");
                sw.Write("  X: ");
                sw.WriteLine(PreviousPoint.X);
                sw.Write("  Y: ");
                sw.WriteLine(PreviousPoint.Y);
                sw.WriteLine("CurrentPoint:");
                sw.Write("  X: ");
                sw.WriteLine(CurrentPoint.X);
                sw.Write("  Y: ");
                sw.Write(CurrentPoint.Y);
            }
        }
    }
}
