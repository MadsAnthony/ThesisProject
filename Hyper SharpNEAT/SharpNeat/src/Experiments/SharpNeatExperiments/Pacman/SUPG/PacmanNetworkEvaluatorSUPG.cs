using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SharpNeatLib.Evolution;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.CPPNs;
using Pacman.Simulator;
using System.Threading;
using PacmanAI;

namespace SharpNeatLib.Experiments
{
    class PacmanNetworkEvaluatorSUPG : INetworkEvaluator
    {
        //public static PointF[] foodLocations;

        //public static float[] sins;
        //public static float[] coss;
        //public static double sqrt2 = Math.Sqrt(2);
        //public static uint resolution=8;
        //public static double threshold = 0;
        //public static float weightRange = 3.0F;
        //public static bool circle = true;

        //public static FoodGatherParams parameters = new FoodGatherParams();

        Maths.FastRandom rand = new SharpNeatLib.Maths.FastRandom();

        public static PacmanSubstrateSUPG substrate = new PacmanSubstrateSUPG(4, 5, 5, HyperNEATParameters.substrateActivationFunction);

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


        public double[] EvaluateNetworkMultipleObjective(INetwork network) {
            INetwork tempNet = null;
            NeatGenome.NeatGenome tempGenome = null;

            tempGenome = substrate.generateGenome(network);

            tempNet = tempGenome.Decode(null);
            SharpNeatExperiments.Pacman.MyForm1.neatGenome = tempGenome;
            SharpNeatExperiments.Pacman.MyForm1.network = tempNet;

            double retries = 1;
            double totalFitness = 0;
            double totalEatScore = 0;
            double totalLifeScore = 0;
            for (int i = 0; i < retries; i++)
            {
                var pacman = new PacmanAINeural.NeuralPacmanSUPG();
                //pacman.SetBrain(network);
                pacman.SetBrain(tempNet, true, tempGenome, network, substrate.getSUPGMap());

                var simplePacmanController = new PacmanAINeural.SimplePacmanController();
                simplePacmanController.SetBrain(tempNet, true, tempGenome, network, substrate.getSUPGMap());

                Visualizer visualizer = null;
                SharpNeatExperiments.Pacman.SimplePacman simplePacman = null;
                Thread visualizerThread;

                visualizerThread = new System.Threading.Thread(delegate()
                {
                    simplePacman = new SharpNeatExperiments.Pacman.SimplePacman(simplePacmanController, false);
                    System.Windows.Forms.Application.Run(simplePacman);

                    //visualizer = new Visualizer(pacman);
                    //System.Windows.Forms.Application.Run(visualizer);
                });
                visualizerThread.Start();
                visualizerThread.Join();

                totalFitness += simplePacman.returnGameScore;
                totalEatScore += simplePacman.returnEatScore;
                totalLifeScore += simplePacman.returnLifeScore;
            }
            double avgFitness = totalFitness / retries;
            double avgEatScore = totalEatScore / retries;
            double avgLifeScore = totalLifeScore / retries;

            return new double[] { avgFitness, avgEatScore, avgLifeScore };
        }

        public double EvaluateNetwork(INetwork network)
        {
            INetwork tempNet = null;
            NeatGenome.NeatGenome tempGenome = null;

            tempGenome = substrate.generateGenome(network);

            tempNet = tempGenome.Decode(null);
            SharpNeatExperiments.Pacman.MyForm1.neatGenome = tempGenome;
            SharpNeatExperiments.Pacman.MyForm1.network = tempNet;

            double retries = 5;
            double totalFitness = 0;
            for (int i = 0; i < retries; i++) {
                var pacman = new PacmanAINeural.NeuralPacmanSUPG();
                //pacman.SetBrain(network);
                pacman.SetBrain(tempNet, true, tempGenome, network, substrate.getSUPGMap());

                var simplePacmanController = new PacmanAINeural.SimplePacmanController();
                simplePacmanController.SetBrain(tempNet, true, tempGenome, network, substrate.getSUPGMap());

                Visualizer visualizer = null;
                SharpNeatExperiments.Pacman.SimplePacman simplePacman = null;
                Thread visualizerThread;

                visualizerThread = new System.Threading.Thread(delegate()
                {
                    bool fastNoDraw = false;
                    simplePacman = new SharpNeatExperiments.Pacman.SimplePacman(simplePacmanController, fastNoDraw);
                    if (!fastNoDraw) {
                        System.Windows.Forms.Application.Run(simplePacman);
                    }

                    //visualizer = new Visualizer(pacman);
                    //System.Windows.Forms.Application.Run(visualizer);
                });
                visualizerThread.Start();
                visualizerThread.Join();

                totalFitness += simplePacman.returnGameScore;// visualizer.returnGameState;
            }
            double avgFitness = totalFitness / retries;

            return avgFitness;
            /*int time = visualizer.returnGameState;
            return (double)time;//fitness;*/
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
