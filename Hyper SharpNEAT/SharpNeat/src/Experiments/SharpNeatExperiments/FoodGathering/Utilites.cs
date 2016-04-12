using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace SharpNeatLib.Experiments
{
    class Utilities
    {
        public static SizeF SizetoSizeF(Size size)
        {
            return new SizeF(size.Width, size.Height);
        }
        public static Size SizeFtoSize(SizeF size)
        {
            return new Size((int)size.Width, (int)size.Height);
        }
        public static Point PointFtoPoint(PointF point)
        {
            return new Point((int)point.X, (int)point.Y);
        }
        public static PointF FindCenter(RectangleF rc)
        {
            return new PointF(rc.X - rc.Width / 2, rc.Y - rc.Height / 2);
        }
        public static RectangleF Translate(RectangleF rc)
        {
            return new RectangleF(FindCenter(rc), rc.Size);
        }
        public static Point FindCenter(Rectangle rc)
        {
            return new Point(rc.X - rc.Width / 2, rc.Y - rc.Height / 2);
        }
        public static Rectangle Translate(Rectangle rc)
        {
            return new Rectangle(FindCenter(rc), rc.Size);
        }
        public static float Distance(RoboAgent a, RoboAgent b)
        {
            return (float)Math.Sqrt(Math.Pow(a.Location.X - b.Location.X,2) + Math.Pow(a.Location.Y - b.Location.Y,2));
        }
        public static float ManhattenDistance(RoboAgent a, RoboAgent b)
        {
            return Math.Abs(a.Location.X - b.Location.X) + Math.Abs(a.Location.Y - b.Location.Y);
        }
    }
}
