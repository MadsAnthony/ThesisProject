using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SharpNeatLib.Evolution;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.CPPNs;


namespace SharpNeatLib.Experiments
{
    class FoodGathererNetworkEvaluator : INetworkEvaluator
    {
        //public static PointF[] foodLocations;

        //public static float[] sins;
        //public static float[] coss;
        //public static double sqrt2 = Math.Sqrt(2);
        //public static uint resolution=8;
        //public static double threshold = 0;
        //public static float weightRange = 3.0F;
        //public static bool circle = true;

        public static FoodGatherParams parameters = new FoodGatherParams();

        Maths.FastRandom rand = new SharpNeatLib.Maths.FastRandom();

        public static Substrate substrate = new FoodGathererSubstrate(FoodGatherParams.resolution, FoodGatherParams.resolution, FoodGatherParams.circle);

        public static Stats postHocAnalyzer(NeatGenome.NeatGenome genome)
        {
            Stats stats=new Stats();
            FloatFastConcurrentNetwork ffcn = GenomeDecoder.DecodeToFloatFastConcurrentNetwork(genome, null);
            INetwork network;
            network=substrate.generateNetwork(ffcn);
            stats.NNconnections = ((FloatFastConcurrentNetwork)network).connectionArray.Length;

            double avgSpeed=0;
            double totalTime=0;
            int numFood=0;
            double timetaken = 0;


            for (int i = 0; i < FoodGatherParams.foodLocations.Length; i++)
            {
                Board testingArena = new Board(0, 500);
                Robot tester = new Robot(new PointF(testingArena.Size.Width / 2.0F, testingArena.Size.Height / 2.0F), (int)FoodGatherParams.resolution, network);
                testingArena.AddRobot(tester);
                testingArena.AddFood(FoodGatherParams.foodLocations[i]);
                
                timetaken = testingArena.game();
                if(timetaken<1000)
                    numFood++;
                totalTime += timetaken;
            }
            avgSpeed = totalTime / FoodGatherParams.foodLocations.Length;

            stats.numFoods = numFood;
            stats.avgSpeed = avgSpeed;

            return stats;
            
        }

        public void endOfGeneration()
        {
            // dummy code
        }
       

        
        #region INetworkEvaluator Members

        public double[] EvaluateNetworkMultipleObjective(INetwork network)
        {
            return new double[] { 0 };
        }

        public double EvaluateNetwork(INetwork network)
        {

            network = substrate.generateNetwork(network);
            
            double distance = 0;
            double timetaken = 0;
            double fitness = 0;

            for (int i = 0; i < FoodGatherParams.foodLocations.Length; i++)
            {
                Board testingArena = new Board(0, 500);
                Robot tester = new Robot(new PointF(testingArena.Size.Width / 2.0F, testingArena.Size.Height / 2.0F), (int)FoodGatherParams.resolution, network);
                testingArena.AddRobot(tester);
                //testingArena.AddRobot(new System.Drawing.PointF(, resolution, network);
                testingArena.AddFood(FoodGatherParams.foodLocations[i]);
                //distance = Math.Abs(testingArena.Robots[0].Location.X - testingArena.Agents[0].Location.X) + Math.Abs(testingArena.Robots[0].Location.Y - testingArena.Agents[0].Location.Y);//distance = Utilites.Distance(testingArena.Robots[0], testingArena.Agents[0]);
                distance = Utilities.ManhattenDistance(testingArena.Agents[0], testingArena.Robots[0]);
                timetaken = testingArena.game();
                if (timetaken >= 1000)
                  ;  //timetaken += Utilites.ManhattenDistance(testingArena.Robots[0], testingArena.Agents[0]);
                else
                {
                    fitness += 100000;
                }
                //System.Diagnostics.Debug.Assert(((distance / testingArena.Robots[0].MaxSpeed )) / (timetaken+1.0F) <= 1);
                //fitness += ((distance / testingArena.Robots[0].MaxSpeed)) / (timetaken+1.0F);
                fitness += 1000 - timetaken;
            }
            
            return fitness;
        }

        public string EvaluatorStateMessage
        {
            get
            {
                if (FoodGatherParams.circle) return "circle " + FoodGatherParams.resolution.ToString();
                else return "topdown " + FoodGatherParams.resolution.ToString();
            }
        }

        #endregion

        #region INetworkEvaluator Members


        public double[] threadSafeEvaluateNetwork(INetwork network, System.Threading.Semaphore sem)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
