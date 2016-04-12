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
using System.Drawing;
namespace PacmanAINeural
{
    public class SimplePacmanController
    {
        public SharpNeatExperiments.Pacman.SimplePacman gameState;
        public Point pos;

        INetwork brain;
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

        public SimplePacmanController(/*SharpNeatExperiments.Pacman.SimplePacman gameState*/) {
            pos = new Point(0, 0);
            //this.gameState = gameState;
        }

        public void CheckForHit() {
            foreach (var enemy in gameState.enemies) {
                if (IsWithinThreshold(enemy.pos, pos, 5) && !enemy.isSleeping) {
                    if (enemy.isEdible) {
                        gameState.score += 1;
                        gameState.eatScore += 1;
                    } else {
                        //gameState.CloseGame();
                        gameState.score -= 5;
                        gameState.lifeScore -= 1;
                    }
                    enemy.Sleep();
                }
            }
        }

        bool IsWithinThreshold(Point point1, Point point2, float threshold) {
            return ((point2.X < point1.X + threshold && point2.X > point1.X - threshold) && 
                    (point2.Y < point1.Y + threshold && point2.Y > point1.Y - threshold));
        }

        public void SetBrain(INetwork network, bool useSUPG = false, NeatGenome genome = null, INetwork cppn = null, int[] triggerMap = null)
        {
            if (useSUPG)
            {
                supgOutputs = new float[network.TotalNeuronCount /*- (network.InputNeuronCount + network.OutputNeuronCount)*/, wavelength]; // need at least as many rows as the number of hidden neurons
                // set all supgOutputs to min value to signal they have not been cached yet
                for (int i = 0; i < network.TotalNeuronCount /*- (network.InputNeuronCount + network.OutputNeuronCount)*/; i++)
                    for (int j = 0; j < wavelength; j++)
                        supgOutputs[i, j] = float.MinValue;
            }

            this.network = network;
            this.useSUPG = useSUPG;
            this.genome = genome;
            this.cppn = cppn;
            this.triggerMap = triggerMap;
            bool[] useSUPGArray = new bool[genome.NeuronGeneList.Count];
            useSUPGArray[7] = true;
            useSUPGArray[8] = true;
            
            if (useSUPG) {
                ((FloatFastConcurrentNetwork)network).UseSUPG = false;
                ((FloatFastConcurrentNetwork)network).useSUPGArray = useSUPGArray;
            }
            brain = network;
        }

        public void update(float timestep, double[] sensors, float[] triggers)
        {
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
                            if (neuron.InnovationId == 7)
                            {
                                neuron.TimeCounter = 0;
                            }
                            if (neuron.InnovationId == 8)
                            {
                                neuron.TimeCounter = wavelength / 2;
                            }
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
                        if (neuron.TimeCounter == 0)
                        {
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
                    foreach (NeuronGene neuron in genome.NeuronGeneList)
                    {
                        /*if (neuron.InnovationId == 6)
                        {
                            overrideSignals[neuron.InnovationId] = 0.5f;
                        }*/
                        if (neuron.InnovationId == 7)
                        {
                            overrideSignals[neuron.InnovationId] = getSUPGActivation(neuron,10);//0.7f;// dummySUPGActivation(neuron);
                        }
                        if (neuron.InnovationId == 8)
                        {
                            overrideSignals[neuron.InnovationId] = getSUPGActivation(neuron,10);//0.3f;// dummySUPGActivation(neuron);
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

        private float getSUPGActivation(NeuronGene neuron, int cppnIterations)
        {
            float activation = 0;
            int offset = 0; // network.InputNeuronCount + network.OutputNeuronCount; // assume that SUPGs are placed at front of hidden neurons
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

        float dummySUPGActivation(NeuronGene neuron)
        {
            float myTimer = (float)neuron.TimeCounter;
            float returnFreq = (float)Math.Sin(myTimer / 5f);
            return returnFreq;
        }

        public void Think() {
            update(gameState.timer, new double[2], new float[40]);

            double[] outputForDir = new double[4];
            outputForDir[0] = 0;
            outputForDir[1] = 0;
            outputForDir[2] = 0;
            outputForDir[3] = 0;
            Direction[] dirs = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right};
            foreach (Direction dir in dirs) {
                brain.SetInputSignal(0, 1); // bias
                brain.SetInputSignal(1, Math.Min(GetClosestEnemies2(dir)[0], 100) / 100f);
                brain.SetInputSignal(2, Math.Min(GetClosestEnemies2(dir)[1], 100) / 100f);
                brain.SetInputSignal(3, gameState.enemies[0].isEdible ? 1 : 0);
                brain.MultipleSteps(10);

                float result0 = 0;
                float result1 = 0;
                if (IsWithinThreshold(network.GetOutputSignal(2), network.GetOutputSignal(3), 0.2f)) {
                    result0 = brain.GetOutputSignal(0);
                }
                else
                {
                    result1 = brain.GetOutputSignal(1);
                }

                /*if (gameState.enemies[0].isEdible)
                {
                    outputForDir[(int)dir] = brain.GetOutputSignal(0);
                }
                else
                {
                    outputForDir[(int)dir] = brain.GetOutputSignal(1);
                }*/
                /*float result1 = 0;
                if (IsWithinThreshold(network.GetOutputSignal(2), network.GetOutputSignal(4), 0.2f)) {
                    result1 = brain.GetOutputSignal(1);
                }*/
                outputForDir[(int)dir] = Math.Max(result0, result1);
                //Console.WriteLine(Math.Min(GetClosestEnemies2(dir)[0], 100) / 100f);
            }
            //Console.WriteLine("");
            SharpNeatExperiments.Pacman.MyForm1.freqMaster = brain.GetOutputSignal(2);
            SharpNeatExperiments.Pacman.MyForm1.freq1 = brain.GetOutputSignal(3);
            SharpNeatExperiments.Pacman.MyForm1.freq2 = brain.GetOutputSignal(4);
            TranslateOutputForBrain(outputForDir);
        }

        private void TranslateOutputForBrain(double[] outputSignals) {
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
                    SetDirection(Direction.Up);
                    return;
                case 1:
                    SetDirection(Direction.Down);
                    return;
                case 2:
                    SetDirection(Direction.Left);
                    return;
                case 3:
                    SetDirection(Direction.Right);
                    return;
            }

            return;
        }

        bool IsWithinThreshold(float input1, float input2, float threshold)
        {
            return (input2 < input1 + threshold && input2 > input1 - threshold);
        }

        int[] GetClosestEnemies(Direction dir) {
            int[] minDistances = new int[4];
            Point newPos = pos;
            switch (dir) {
                case Direction.Up:
                    newPos.Y -= 1;
                    break;
                case Direction.Down:
                    newPos.Y += 1;
                    break;
                case Direction.Left:
                    newPos.X -= 1;
                    break;
                case Direction.Right:
                    newPos.X += 1;
                    break;
            }
            for (int i = 0; i < gameState.enemies.Length; i++)
            {
                if (gameState.enemies[i].isSleeping) {
                    //minDistances[i] = 200; // set to far away.
                    //continue;
                }
                int[] tmpMinDist = new int[5];
                tmpMinDist[0] = GetManhattenDistance(newPos, gameState.enemies[i].pos);

                //ouside edges
                var upPos = new Point(gameState.enemies[i].pos.X,gameState.enemies[i].pos.Y-gameState.height);
                tmpMinDist[1] = GetManhattenDistance(newPos, upPos);
                var downPos = new Point(gameState.enemies[i].pos.X,gameState.enemies[i].pos.Y+gameState.height);
                tmpMinDist[2] = GetManhattenDistance(newPos, downPos);
                var leftPos = new Point(gameState.enemies[i].pos.X-gameState.width, gameState.enemies[i].pos.Y);
                tmpMinDist[3] = GetManhattenDistance(newPos, leftPos);
                var rightPos = new Point(gameState.enemies[i].pos.X+gameState.width, gameState.enemies[i].pos.Y);
                tmpMinDist[4] = GetManhattenDistance(newPos, rightPos);

                Array.Sort(tmpMinDist);
                minDistances[i] = tmpMinDist[0];
            }
            Array.Sort(minDistances);
            return (minDistances);
        }

        int[] GetClosestEnemies2(Direction dir)
        {
            int[] minDistances = new int[4];
            for (int i = 0; i < gameState.enemies.Length; i++)
            {
                if (gameState.enemies[i].isSleeping)
                {
                    //minDistances[i] = 200; // set to far away.
                    //continue;
                }
                Point enemyPos = gameState.enemies[i].pos;
                Point tmpV = new Point(pos.X - enemyPos.X, pos.Y - enemyPos.Y);

                switch (dir)
                {
                    case Direction.Up:
                        if (tmpV.Y <= 0) {
                            minDistances[i] = (gameState.height - Math.Abs(tmpV.Y)) + Math.Abs(tmpV.X);
                        }
                        else {
                            minDistances[i] = Math.Abs(tmpV.Y) + Math.Abs(tmpV.X);
                        }
                        break;
                    case Direction.Down:
                        if (tmpV.Y >= 0) {
                            minDistances[i] = (gameState.height - Math.Abs(tmpV.Y)) + Math.Abs(tmpV.X);
                        }
                        else {
                            minDistances[i] = Math.Abs(tmpV.Y) + Math.Abs(tmpV.X);
                        }
                        break;
                    case Direction.Left:
                        if (tmpV.X <= 0) {
                            minDistances[i] = (gameState.width - Math.Abs(tmpV.X)) + Math.Abs(tmpV.Y);
                        }
                        else {
                            minDistances[i] = Math.Abs(tmpV.X) + Math.Abs(tmpV.Y);
                        }
                        break;
                    case Direction.Right:
                        if (tmpV.X >= 0) {
                            minDistances[i] = (gameState.width - Math.Abs(tmpV.X)) + Math.Abs(tmpV.Y);
                        }
                        else {
                            minDistances[i] = Math.Abs(tmpV.X) + Math.Abs(tmpV.Y);
                        }
                        break;
                }
                //if (tmpV.Y<)

                //Array.Sort(tmpMinDist);
                //minDistances[i] = tmpMinDist[0];
            }
            Array.Sort(minDistances);
            return (minDistances);
        }

        int GetManhattenDistance(Point pos1, Point pos2) {
            Point tmpV = new Point(Math.Abs(pos1.X - pos2.X), Math.Abs(pos1.Y - pos2.Y));
            return tmpV.X + tmpV.Y;
        }

        public void SetDirection(Direction dir) {
            int newX, newY;
            switch (dir) {
                case Direction.Up:
                    newY = pos.Y - 1;
                    if (newY<0) {
                        newY = gameState.height-1;
                    }
                    pos = new Point(pos.X, newY);
                    break;
                case Direction.Down:
                    newY = pos.Y + 1;
                    if (newY > gameState.height-1) {
                        newY = 0;
                    }
                    pos = new Point(pos.X, newY);
                    break;
                case Direction.Left:
                    newX = pos.X - 1;
                    if (newX < 0) {
                        newX = gameState.width-1;
                    }
                    pos = new Point(newX, pos.Y);
                    break;
                case Direction.Right:
                    newX = pos.X + 1;
                    if (newX > gameState.width-1) {
                        newX = 0;
                    }
                    pos = new Point(newX, pos.Y);
                    break;
            }
        }
    }
}
