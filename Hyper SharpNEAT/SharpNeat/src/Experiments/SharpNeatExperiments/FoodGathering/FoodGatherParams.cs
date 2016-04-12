using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib.NeuralNetwork;

namespace SharpNeatLib.Experiments
{
    public class FoodGatherParams : HyperNEATParameters
    {
        public static System.Drawing.PointF[] foodLocations;
        public static float[] sins;
        public static float[] coss;
        public static uint resolution;
        public static bool circle;
        public static bool distance;

        static FoodGatherParams()
        {
            loadParameterFile();
            string tempParam = null;
            tempParam = getParameter("circle");
            if (tempParam != null)
                bool.TryParse(tempParam, out circle);
            tempParam = getParameter("resolution");
            if (tempParam != null)
                uint.TryParse(tempParam, out resolution);
            tempParam = getParameter("distance");
            if(tempParam!=null)
                bool.TryParse(tempParam, out distance);
            fillLookups();
            fillFood();
        }
        public static void fillFood()
        {
            double angle = - 3 * Math.PI / 4.0, angledelta = 2 * Math.PI / (resolution*2);
            foodLocations = new System.Drawing.PointF[resolution*2];
            for (int j = 0; j < resolution*2; j++,angle+=angledelta)
            {
               foodLocations[j] = new System.Drawing.PointF(250 +100* (float)Math.Cos(angle + angledelta / 2), 250 + 100* (float)Math.Sin(angle + angledelta / 2));    
            }
        }

            public static void fillLookups()
            {
                sins = new float[resolution];
                coss = new float[resolution];
                double angledelta = (2 * Math.PI) / resolution;
                double angle = -3 * Math.PI / 4;
                for(int j=0;j<resolution;j++, angle+=angledelta)
                {
                    sins[j] = (float)Math.Sin(angle + (angledelta / 2));
                    coss[j] = (float)Math.Cos(angle + (angledelta / 2));
                }

            }

        }
    }
