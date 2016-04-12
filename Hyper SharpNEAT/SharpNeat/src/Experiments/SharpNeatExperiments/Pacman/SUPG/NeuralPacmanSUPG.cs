using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Pacman.Simulator;
using Pacman.Simulator.Ghosts;

using SharpNeatLib.Evolution;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.CPPNs;
namespace PacmanAINeural
{
	public class NeuralPacmanSUPG : BasePacman
	{
        INetwork brain;
        GameState gameState;
        Node[,] ghostsPrevNode = new Node[4, 6];

        private static int wavelength = 100;  // SUPG wavelength
        private static int compression = 50;
        // arrays added to cache CPPN outputs for SUPG activation
        private float[,] supgOutputs;
        private bool kickstart = true;

        INetwork network;
        bool useSUPG;
        NeatGenome genome;
        INetwork cppn;
        int[] triggerMap;

		public NeuralPacmanSUPG() : base("NeuralPacmanSUPG") {			
		}

        public void SetBrain(INetwork network, bool useSUPG = false, NeatGenome genome = null, INetwork cppn = null, int[] triggerMap = null) {
            if (useSUPG)
            {
                supgOutputs = new float[network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount), wavelength]; // need at least as many rows as the number of hidden neurons
                // set all supgOutputs to min value to signal they have not been cached yet
                for (int i = 0; i < network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount); i++)
                    for (int j = 0; j < wavelength; j++)
                        supgOutputs[i, j] = float.MinValue;
            }

            this.network = network;
            this.useSUPG = useSUPG;
            this.genome = genome;
            this.cppn = cppn;
            this.triggerMap = triggerMap;
            if (useSUPG)
                ((FloatFastConcurrentNetwork)network).UseSUPG = true;
            brain = network;
        }

        public void update(float timestep, double[] sensors, float[] triggers)
        {
            /*if (SW != null && false)
            {
                SW.WriteLine(triggers[0] + "," + triggers[1] + "," + triggers[2] + "," + triggers[3]);
                SW.Flush();
            }*/

            if (network != null)
            {
                int iterations = 17;

                network.ClearSignals();
                if (useSUPG)
                {
                    int cppnIterations = 2 * (cppn.TotalNeuronCount - (cppn.InputNeuronCount + cppn.OutputNeuronCount)) + 1;
                    // kickstart by setting leg timers to 1, w/2, w/2, 1
                    if (kickstart)
                    {
                        kickstart = false;
                        // set triggers to 0
                        triggers = new float[triggers.Length];

                        // set time counters to the kickstart values
                        foreach (NeuronGene neuron in genome.NeuronGeneList)
                        {
                            // get offset value from 2nd cppn output
                            if (neuron.InnovationId == 7) {
                                neuron.TimeCounter = 0;
                            }
                            if (neuron.InnovationId == 8) {
                                neuron.TimeCounter = wavelength/2;
                            }
                            if (neuron.InnovationId >= 16 && neuron.InnovationId <= 18)
                                neuron.TimeCounter = getOffset(1, cppnIterations, neuron);
                            if (neuron.InnovationId >= 19 && neuron.InnovationId <= 21)
                                neuron.TimeCounter = getOffset(2, cppnIterations, neuron);
                            if (neuron.InnovationId >= 22 && neuron.InnovationId <= 24)
                                neuron.TimeCounter = getOffset(3, cppnIterations, neuron);
                            if (neuron.InnovationId >= 25 && neuron.InnovationId <= 27)
                                neuron.TimeCounter = getOffset(4, cppnIterations, neuron);
                        }
                    }

                    // set up the override array
                    float[] overrideSignals = new float[network.TotalNeuronCount];
                    for (int i = 0; i < overrideSignals.Length; i++)
                        overrideSignals[i] = float.MinValue;

                    // update the SUPGs
                    foreach (NeuronGene neuron in genome.NeuronGeneList)
                    {
                        /* code for triggers */
                        // increment the time counter of any SUPG that is currently running

                        /*if (neuron.TimeCounter > 0)
                        {*/
                            neuron.TimeCounter = (neuron.TimeCounter + 1) % wavelength;
                            // if the time counter finished and went back to zero, the first step is complete
                            if (neuron.TimeCounter == 0) {
                                neuron.TimeCounter = 1; // added this line to keep the frequency going
                                neuron.FirstStepComplete = true;
                            }
                        //}

                        // check if the neuron is a triggered neuron
                        if (triggerMap[neuron.InnovationId] != int.MinValue)
                        {
                            // check the trigger
                            if (triggers[triggerMap[neuron.InnovationId]] == 1)
                            {
                                // if the time counter was non zero, then the first step has been completed
                                if (neuron.TimeCounter > 0)
                                    neuron.FirstStepComplete = true;

                                // set the neuron's time to 1
                                neuron.TimeCounter = 1;
                            }
                        }
                    }

                    // determine the proper outputs of the SUPGs and send the override array to the network

                    /*foreach (NeuronGene neuron in genome.NeuronGeneList)
                    {
                        if (neuron.TimeCounter > 0)  // only need to check neurons whose time counter is non zero
                        {
                        overrideSignals[neuron.InnovationId] = getSUPGActivation(neuron, cppnIterations);
                        }
                    }*/
                    foreach (NeuronGene neuron in genome.NeuronGeneList)
                    {
                        if (neuron.InnovationId == 6)
                        {
                            overrideSignals[neuron.InnovationId] = 0.5f;
                        }
                        if (neuron.InnovationId == 7) {
                            overrideSignals[neuron.InnovationId] = dummySUPGActivation(neuron);
                        }
                        if (neuron.InnovationId == 8)
                        {
                            overrideSignals[neuron.InnovationId] = dummySUPGActivation(neuron);
                        }
                    }
                    ((FloatFastConcurrentNetwork)network).OverrideSignals = overrideSignals;
                }
                else
                {
                    network.SetInputSignals(sensors);
                }
                network.MultipleSteps(iterations);

            }
        }

        float dummySUPGActivation(NeuronGene neuron) {
            float myTimer = (float)neuron.TimeCounter;
            float returnFreq = (float)Math.Sin(myTimer / 5f);
            return returnFreq;
        }

        private float getSUPGActivation(NeuronGene neuron, int cppnIterations)
        {
            float activation = 0;
            int offset = network.InputNeuronCount + network.OutputNeuronCount; // assume that SUPGs are placed at front of hidden neurons
            // if the element is float.min, then we have not yet cached the SUPG output
            if (supgOutputs[neuron.InnovationId - offset, neuron.TimeCounter] == float.MinValue)
            {
                double[] coordinates = new double[3];


                coordinates[0] = (float)neuron.XValue;
                coordinates[1] = (float)neuron.YValue;


                coordinates[0] = coordinates[0] / compression;

                coordinates[2] = (float)neuron.TimeCounter / wavelength;

                cppn.ClearSignals();
                cppn.SetInputSignals(coordinates);
                cppn.MultipleSteps(cppnIterations);

                if (neuron.FirstStepComplete)
                {
                    activation = cppn.GetOutputSignal(0);
                    supgOutputs[neuron.InnovationId - offset, neuron.TimeCounter] = activation;  // only cache the output if the first step is complete
                }
                else
                    activation = cppn.GetOutputSignal(0);

            }
            else
            {
                // get the cached value
                activation = supgOutputs[neuron.InnovationId - offset, neuron.TimeCounter];
            }

            //Console.WriteLine(activation);
            return activation;
        }

        private int getOffset(int leg, int cppnIterations, NeuronGene neuron)
        {
            int offset = 0;
            /*double[] coordinates = new double[3];
            coordinates[0] = (float)neuron.XValue / compression;
            cppn.ClearSignals();
            cppn.SetInputSignals(coordinates);
            cppn.MultipleSteps(cppnIterations);
            float activation = cppn.GetOutputSignal(1);
            offset = (int)Math.Ceiling((activation + 1) * wavelength / 2);
            if (offset <= 0)
                offset = 1;
            if (offset >= wavelength)
                offset = wavelength - 1;
            */
            offset += 2;
            return offset;
        }

        public float[] getOutputs()
        {
            float[] outs = new float[12];
            for (int i = 0; i < 12; i++)
            {
                outs[i] = network.GetOutputSignal(i);
                if (useSUPG)
                {
                    // with the SUPG architecture, we need the outputs to be normalized between 0 and 1
                    // because the CPPN uses bipolar sigmoid for all outputs, which increases the range to -1, 1
                    // when SUPGs start becoming true hidden nodes, we can remove this modification
                    outs[i] = (outs[i] + 1) / 2;
                }
            }
            return outs;
        }

		public override Direction Think(GameState gs) {
			/*List<Direction> possible = gs.Pacman.PossibleDirections();			
			if( possible.Count > 0 ) {
				int select = GameState.Random.Next(0, possible.Count);
				if( possible[select] != gs.Pacman.InverseDirection(gs.Pacman.Direction) )
					return possible[select];
			}*/
            gameState = gs;
            gameState.timeStep++;
            update(gameState.timeStep,new double[2], new float[40]);

            double[] outputForDir = new double[4];
            outputForDir[0] = 0;
            outputForDir[1] = 0;
            outputForDir[2] = 0;
            outputForDir[3] = 0;
            //Console.WriteLine(brain.GetOutputSignal(0));

            foreach (Direction dir in gs.Pacman.PossibleDirections()) {
                double[] closestGhosts = GetClosestGhosts(dir);

                brain.ClearSignals();

                // *****************
                // Common Undirected Sensors (8 inputs)
                // *****************
                /*
                brain.SetInputSignal(0, 1); // Bias
                brain.SetInputSignal(1, gameState.Map.PillsLeft / (double)gameState.Map.PillNodes.Count); // Proportion of regular pills in maze
                brain.SetInputSignal(2, gameState.Map.PowerPillsLeft / (double)gameState.Map.PowerPillNodes.Count); // Proportion of power pills in maze
                brain.SetInputSignal(3, gameState.GetProportionsOfEdibleGhosts()); // Proportion of edible ghosts
                brain.SetInputSignal(4, gameState.GetProportionOfFleeTime()); // Proportion of remaining ghost edible time
                brain.SetInputSignal(5, gameState.AnyGhostEdible() ? 1 : 0); // Any ghost edible
                brain.SetInputSignal(6, gameState.AllGhostOutsideLair() ? 1 : 0); // Are all ghosts outside lair
                brain.SetInputSignal(7, gameState.IsNeareastPowerPillWithinDistance(10) ? 1 : 0); // Are there any power pills in a distance of 10 units.

                // *****************
                // Split Sensors (16 inputs)
                // *****************
                // 1st, 2nd, 3rd, 4th closest ghosts
                brain.SetInputSignal(8, closestGhosts[0]);
                brain.SetInputSignal(9, closestGhosts[1]);
                brain.SetInputSignal(10, closestGhosts[2]);
                brain.SetInputSignal(11, closestGhosts[3]);

                // Is each ghosts approaching
                bool[] ghostsApproaching = AreGhostsApproaching(dir);
                brain.SetInputSignal(12, ghostsApproaching[0] ? 1 : 0);
                brain.SetInputSignal(13, ghostsApproaching[1] ? 1 : 0);
                brain.SetInputSignal(14, ghostsApproaching[2] ? 1 : 0);
                brain.SetInputSignal(15, ghostsApproaching[3] ? 1 : 0);

                // Has ghost on the directed path a junction
                brain.SetInputSignal(16, (Map.GetNumberOfJunctionsInPath(gameState.Pacman.Node, gameState.Ghosts[0].Node, dir) > 0) ? 1 : 0);
                brain.SetInputSignal(17, (Map.GetNumberOfJunctionsInPath(gameState.Pacman.Node, gameState.Ghosts[1].Node, dir) > 0) ? 1 : 0);
                brain.SetInputSignal(18, (Map.GetNumberOfJunctionsInPath(gameState.Pacman.Node, gameState.Ghosts[2].Node, dir) > 0) ? 1 : 0);
                brain.SetInputSignal(19, (Map.GetNumberOfJunctionsInPath(gameState.Pacman.Node, gameState.Ghosts[3].Node, dir) > 0) ? 1 : 0);

                // Is each ghost edible
                brain.SetInputSignal(20, gameState.Ghosts[0].IsEdible() ? 1 : 0);
                brain.SetInputSignal(21, gameState.Ghosts[1].IsEdible() ? 1 : 0);
                brain.SetInputSignal(22, gameState.Ghosts[2].IsEdible() ? 1 : 0);
                brain.SetInputSignal(23, gameState.Ghosts[3].IsEdible() ? 1 : 0);

                // *****************
                // Directed Sensors (6 inputs)
                // *****************

                brain.SetInputSignal(24, GetDistanceForPathWithinRange(StateInfo.NearestPill(gameState.Pacman.Node, gameState, dir, Node.NodeType.Pill))); // nearest regular pill
                brain.SetInputSignal(25, GetDistanceForPathWithinRange(StateInfo.NearestPill(gameState.Pacman.Node, gameState, dir, Node.NodeType.PowerPill))); // nearest power pill
                brain.SetInputSignal(26, GetDistanceForPathWithinRange(StateInfo.NearestJunction(gameState.Pacman.Node, gameState, dir))); // nearest junction
                brain.SetInputSignal(27, gameState.GetMostNumberOfPillInPath(gameState.Pacman.Node.GetNode(dir), gameState.Pacman.Node, 30) / (double)30); // Number of regular pills in the path with most regular pills (30 steps)
                brain.SetInputSignal(28, gameState.GetMostNumberOfJunctionsInPath(gameState.Pacman.Node.GetNode(dir), gameState.Pacman.Node, 30) / (double)30); // Number of junction in the path with most junctions (30 steps)
                brain.SetInputSignal(29, Math.Min(gameState.OptionFromNextJunction(dir),10)/ (double)10);
                */
                //brain.SingleStep();
                brain.MultipleSteps(10);

                // Use two Modules
                /*double[] preferenceNeurons = new double[2];
                preferenceNeurons[0] = brain.GetOutputSignal(1);
                preferenceNeurons[1] = brain.GetOutputSignal(3);*/

                var result = brain.GetOutputSignal(0);
                //Console.WriteLine(result);
                SharpNeatExperiments.Pacman.MyForm1.freqMaster = brain.GetOutputSignal(2);
                SharpNeatExperiments.Pacman.MyForm1.freq1 = brain.GetOutputSignal(3);
                SharpNeatExperiments.Pacman.MyForm1.freq2 = brain.GetOutputSignal(4);
                if (result < -0.5f) {
                    return Direction.Left;
                }
                if (result > 0.5f)
                {
                    return Direction.Right;
                }
                if (result > -0.5f && result < 0f)
                {
                    return Direction.Down;
                }
                if (result > 0f && result < 0.5f)
                {
                    return Direction.Up;
                }


                outputForDir[(int)dir] = brain.GetOutputSignal(0);//brain.GetOutputSignal(GetModuleIndex(preferenceNeurons));
            }

            return TranslateOutputForBrain(outputForDir);
		}

        int GetModuleIndex(double[] preferenceNeurons) {
            int length = preferenceNeurons.Length;
            double maxValue = 0;
            int maxIndex = 0;
            for (int i = 0; i < length; i++) {
                if (preferenceNeurons[i] > maxValue)
                {
                    maxValue = preferenceNeurons[i];
                    maxIndex = i;
                }
            }
            return 2*maxIndex;
        }

        double GetDistanceForPathWithinRange(Node.PathInfo path) {
            if (path != null) {
                return Math.Min(path.Distance, 40) / 40f;
            }
            return 1;
        }

        bool[] AreGhostsApproaching(Direction dir) {
            bool[] isGhostsApproaching = new bool[4];
            for (int i = 0; i < 4; i++) {
                Node prevGhostNode = ghostsPrevNode[i, (int)dir];
                Node curGhostNode = gameState.Ghosts[i].Node;
                bool isAproaching = false;
                if (prevGhostNode != null)
                {
                    var prevPath = gameState.Pacman.Node.ShortestPath[prevGhostNode.X, prevGhostNode.Y, (int)dir];
                    var curPath = gameState.Pacman.Node.ShortestPath[curGhostNode.X, curGhostNode.Y, (int)dir];

                    if (prevPath != null && curPath != null) {
                        isAproaching = (prevPath.Distance - curPath.Distance) > 0;
                    }
                }

                isGhostsApproaching[i] = isAproaching;
                // save currentNode as prev
                ghostsPrevNode[i, (int)dir] = curGhostNode;
            }
            return isGhostsApproaching;
        }

        private Direction TranslateOutputForBrain(double[] outputSignals) {
            double maxValue = 0;
            int maxIndex = 0;
            for (int i = 0; i < 4; i++)
            {
                if (outputSignals[i] > maxValue)
                {
                    maxValue = outputSignals[i];
                    maxIndex = i;
                }
            }
            switch (maxIndex)
            {
                case 0:
                    return Direction.Up;
                case 1:
                    return Direction.Down;
                case 2:
                    return Direction.Left;
                case 3:
                    return Direction.Right;
            }

            return Direction.None;
        }
        double[] GetClosestGhosts(Direction dir) {
            double[] minDistances = new double[4];
            for (int i = 0; i < gameState.Ghosts.Count();i++){
                minDistances[i] = 1;
                Ghost ghost = gameState.Ghosts[i];
                if (ghost == null) continue;
                Node.PathInfo path = gameState.Pacman.Node.ShortestPath[ghost.Node.X, ghost.Node.Y, (int)dir];
                if (path != null) {
                    minDistances[i] = Math.Min(path.Distance, 40) / 40f;
                }
            }
            Array.Sort(minDistances);
            return (minDistances);
        }

        private Direction TranslateOutputBrain() {
            double[] outputSignals = new double[4];
            outputSignals[0] = brain.GetOutputSignal(0);
            outputSignals[1] = brain.GetOutputSignal(1);
            outputSignals[2] = brain.GetOutputSignal(2);
            outputSignals[3] = brain.GetOutputSignal(3);

            double maxValue = 0;
            int maxIndex = 0;
            for (int i=0;i<4;i++) {
                if (outputSignals[i] > maxValue) {
                    maxValue = outputSignals[i];
                    maxIndex = i;
                }
            }

            switch (maxIndex) {
                case 0:
                    return Direction.Up;
                case 1:
                    return Direction.Down;
                case 2:
                    return Direction.Left;
                case 3:
                    return Direction.Right;
            }
            /*List<double> bla = new List<double>();
            if (brain.GetOutputSignal(0)>0.5f) {
                return Direction.Up;
            }
            if (brain.GetOutputSignal(1) > 0.5f)
            {
                return Direction.Down;
            }
            if (brain.GetOutputSignal(2) > 0.5f)
            {
                return Direction.Left;
            }
            if (brain.GetOutputSignal(3) > 0.5f)
            {
                return Direction.Right;
            }*/
            return Direction.None;
        }
	}
}
