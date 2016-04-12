using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace SharpNeatLib.Experiments
{
    public interface RoboAgent
    {
        int Sensors
        {
            get;
        }
        float SensorRadius
        {
            get;
        }
        double[] SensorReadings
        {
            get;
        }
        PointF Location
        {
            get;
        }
        int InvalidMoves
        {
            get;
        }
        void Move();
        void Move(PointF newLoc);
        void Move(SizeF delta);
    }
}
