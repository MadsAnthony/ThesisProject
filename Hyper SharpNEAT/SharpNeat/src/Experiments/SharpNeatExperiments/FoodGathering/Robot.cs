using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SharpNeatLib.NeuralNetwork;

namespace SharpNeatLib.Experiments
{
    public class Robot : RoboAgent
    {
        PointF location;
        double[] sensorReadings;
        float sensorRadius=400;
        float maxSpeed = 5;
        public INetwork brain=null;
        int invalidMoves = 0;
        public Robot()
        {
            location = new PointF(0, 0);
            sensorReadings = new double[4];
        }
        public Robot(PointF loc) 
        {
            location = loc;
            sensorReadings = new double[4];
        }
        public Robot(PointF loc, int numSensors)
        {
            location = loc;
            sensorReadings = new double[numSensors];
        }
        public Robot(PointF loc, int numSensors, INetwork net)
        {
            location = loc;
            sensorReadings = new double[numSensors];
            brain = net;
        }
        public void clearSensors()
        {
            for (int i = 0; i < sensorReadings.Length; i++)
                sensorReadings[i] = 0;
        }
        public bool ProcessAgent(RoboAgent target)
        {
            if (target == this || Utilities.Distance(this, target) > SensorRadius)
                return false;
            else
            {
                double angledelta=2*Math.PI/Sensors;
                double testangle=-3*Math.PI/4;
                float angle = (float)Math.Atan2(target.Location.Y - Location.Y, target.Location.X - Location.X);
                
                for (int j = 0; j < sensorReadings.Length; j++, testangle += angledelta)
                {
                    if (testangle >= Math.PI)
                        testangle -= 2*Math.PI;
                    if (angle >= testangle && angle <= testangle + angledelta)
                    {
                        sensorReadings[j] = 1;
                        //sensorReadings[j] += 1 - (Utilites.Distance(this, target) / SensorRadius);
                        return true;
                    }
                }
                sensorReadings[sensorReadings.Length - 1] = 1.0;
                //sensorReadings[sensorReadings.Length - 1] += 1.0 - (Utilites.Distance(this, target) / SensorRadius);
                return true;
            }
        }
        /*public SizeF determineMove()
        {
            if(brain==null)
                return new SizeF((maxSpeed)*(float)(sensorReadings[1] - sensorReadings[3]), (maxSpeed)*(float)(sensorReadings[2] - sensorReadings[0]));
            brain.ClearSignals();
            brain.SetInputSignals(sensorReadings);
            for(int i=0;i<5;i++)
                brain.SingleStep();
            float sum=0, max =-1, maxi=-1;
           float[] outs = new float[brain.OutputNeuronCount];
            for (int outputs = 0; outputs < brain.OutputNeuronCount; outputs++)
            {
                
                float d =(float) brain.GetOutputSignal(outputs);
                if (Double.IsNaN(d))
                    d = 0;
                sum += Math.Abs(d);
                outs[outputs] = d;
                if (Math.Abs(d) > max)
                {
                    max = Math.Abs(d);
                    maxi = outputs;
                }

            }
            if (sum == 0)
                return new SizeF(0, 0);
           /* if(maxi==1)
            {
                return new SizeF(maxSpeed, 0);
            }
            else if (maxi == 3)
            {
                return new SizeF(-maxSpeed, 0);
            }
            else if (maxi == 0)
            {
                return new SizeF(0, -maxSpeed);
            }
            else if (maxi==2)
            {
                return new SizeF(0,maxSpeed);
            }

            float dx=((maxSpeed/sum)*(outs[1]-outs[3]));
            float dy=((maxSpeed/sum)*(outs[2]-outs[0]));
            //System.Diagnostics.Debug.Assert(dx + dy <= maxSpeed);
            return new SizeF(dx, dy);
        }*/
        public SizeF determineMove()
        {
            if (brain == null)
                return new SizeF((maxSpeed) * (float)(sensorReadings[1] - sensorReadings[3]), (maxSpeed) * (float)(sensorReadings[2] - sensorReadings[0]));
            brain.SetInputSignals(sensorReadings);
            brain.SingleStep();
            int maxReadingIdx = -1;
            double maxReading = -2;
            double output;
            SizeF move = new SizeF(0, 0);
            for (int i = 0; i < Sensors; i++)
            {
                output = brain.GetOutputSignal(i);
                if (output == maxReading)
                {
                   // tie = true;
                }
                if (output > maxReading)
                {
                    maxReading = output;
                    maxReadingIdx = i;
                   // tie = false;
                }
            }
            //if (tie)
             //   return new SizeF(0, 0);
            double angledelta = 2 * Math.PI / Sensors;
            double testangle = -3 * Math.PI / 4;
            for (int reading = 0; reading < Sensors; reading++, testangle += angledelta)
            {
            //    if (testangle >= Math.PI)
            //       testangle -= 2 * Math.PI;
                output = brain.GetOutputSignal(reading);
                //System.Diagnostics.Debug.Assert(output == 1);
                if (output != 0)
                {
                    float dx=(float)((maxSpeed*output)*Math.Cos(testangle+(angledelta/2)));
                    float dy = (float)((maxSpeed * output) * Math.Sin(testangle + (angledelta / 2)));
                    move.Height += dy;
                    move.Width += dx;
                }
                if (reading == maxReadingIdx)
                {
                  //  return new SizeF((float)(maxSpeed / maxReading) * (float)Math.Cos(testangle + (angledelta / 2)), (float)(maxSpeed / maxReading) * (float)Math.Sin(testangle + (angledelta / 2)));
                }
            }
            if (move == (new SizeF(0, 0)))
                return new SizeF(0, 0);
            float angle = (float)Math.Atan2((Location.Y+move.Height) - Location.Y, (Location.X+move.Width) - Location.X);
            move = new SizeF(maxSpeed * (float)Math.Cos(angle), maxSpeed * (float)Math.Sin(angle));

            return move;
        }

        public SizeF determineBiasedMove()
        {
            if (brain == null)
                return new SizeF((maxSpeed) * (float)(sensorReadings[1] - sensorReadings[3]), (maxSpeed) * (float)(sensorReadings[2] - sensorReadings[0]));
            brain.SetInputSignals(sensorReadings);
            brain.SingleStep();
            int maxReadingIdx = -1;
            double maxReading = -2;
            double output;
            double sumOuts = 0;
            SizeF move = new SizeF(0, 0);
            int numEqual = 0;
            for (int i = 0; i < Sensors; i++)
            {
                output = brain.GetOutputSignal(i);
                if (output == maxReading)
                {
                    numEqual++;
                    maxReadingIdx = i;
                }
                /*if (output == .5)
                {
                    output = 0;
                    continue;
                }*/
                if (output > maxReading)
                {
                    numEqual=1;
                    maxReading = output;
                    maxReadingIdx = i;
                }
                sumOuts += output;
            }
            //if (numEqual>=Sensors || maxReadingIdx==-1)
            //    return new SizeF(0, 0);
            double angledelta = 2 * Math.PI / Sensors;
            double testangle = -3 * Math.PI / 4;
            for (int reading = 0; reading < Sensors; reading++, testangle += angledelta)
            {
             //   if (testangle >= Math.PI)
             //       testangle -= 2 * Math.PI;
              //  output = brain.GetOutputSignal(reading);
                if (reading == maxReadingIdx)
                {
                    //return new SizeF((float)((maxSpeed * maxReading) * (maxReading / sumOuts)) * (float)Math.Cos(testangle + (angledelta / 2)), (float)((maxSpeed * maxReading) * (maxReading / sumOuts)) * (float)Math.Sin(testangle + (angledelta / 2)));
                    return new SizeF((float)((maxSpeed * maxReading) * (maxReading / sumOuts) * FoodGatherParams.coss[reading]), (float)((maxSpeed * maxReading) * (maxReading / sumOuts)) * FoodGatherParams.sins[reading]);
                }
            }
            return move;
        }

        #region Agent Members

        public System.Drawing.PointF Location
        {
            get { return location; }
        }

        public void Move()
        {
            return;
        }
        public void Move(PointF newPos)
        {
            location = newPos;
        }

        public int Sensors
        {
            get { return sensorReadings.Length; }
        }

        public float SensorRadius
        {
            get { return sensorRadius; }
        }

        public double[] SensorReadings
        {
            get { return sensorReadings; }
        }

        public void Move(SizeF delta)
        {
            location.X += delta.Width;
            location.Y += delta.Height;
        }
        public float MaxSpeed
        {
            get { return maxSpeed; }
        }
        public int InvalidMoves
        {
            get { return invalidMoves; }
            set { invalidMoves = value; }
        }

        #endregion
    }
}
