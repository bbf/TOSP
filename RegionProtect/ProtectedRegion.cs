using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Terraria
{
    public class ProtectedRegion
    {
        public string Owner { get; set; }
        public HashSet<string> Members { get; set; }
        public Vector2 CornerA { get; set; }
        public Vector2 CornerB { get; set; }

        public ProtectedRegion()
        {
            // Required for XmlSerializer
        }

        public ProtectedRegion(String owner, Vector2 a, Vector2 b)
        {
            Owner = owner;
            Members = new HashSet<string>();
            CornerA = a;
            CornerB = b;
        }

        public bool Contains(float x, float y, int borderX = 0, int borderY = 0)
        {
            return Contains(new Vector2(x, y), borderX, borderY);
        }

        public bool Contains(Vector2 point, int borderX = 0, int borderY = 0)
        {
            return ContainsInX(point, borderX) && ContainsInY(point, borderY);
        }

        protected bool ContainsInX(Vector2 point, int border = 0)
        {
            if (CornerA.X > CornerB.X)
            {
                return CornerB.X + border <= point.X && CornerA.X + border >= point.X;
            }
            else
            {
                return CornerA.X + border <= point.X && CornerB.X + border >= point.X;
            }
        }

        protected bool ContainsInY(Vector2 point, int border = 0)
        {
            if (CornerA.Y > CornerB.Y)
            {
                return CornerB.Y + border <= point.Y && CornerA.Y + border >= point.Y;
            }
            else
            {
                return CornerA.Y + border <= point.Y && CornerB.Y + border >= point.Y;
            }
        }

        public override string ToString()
        {
            return Owner + " A(" + CornerA.X + "," + CornerA.Y + ") B(" + CornerB.X + "," + CornerB.Y + ")";
        }
    }
}
