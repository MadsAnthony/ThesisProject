using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace SharpNeatLib.Experiments
{
    public class Food : RoboAgent
    {
        private PointF location;
        #region Agent Members
        public Food()
        {
            location = new PointF(0, 0);
        }
        public Food(PointF loc)
        {
            location = loc;
        }
        public PointF Location
        {
            get { return location; }
        }

        public void Move()
        {
            return;
        }

        public int Sensors
        {
            get { return 0; }
        }

        public float SensorRadius
        {
            get { return 0; }
        }

        public double[] SensorReadings
        {
            get { return null; }
        }

        public void Move(PointF newLoc)
        {
            location=newLoc;
        }

        public void Move(SizeF delta)
        {
            location.X += delta.Width;
            location.Y += delta.Height;
        }

        public int InvalidMoves
        {
            get { return 0; }
        }

        #endregion
    }
}
