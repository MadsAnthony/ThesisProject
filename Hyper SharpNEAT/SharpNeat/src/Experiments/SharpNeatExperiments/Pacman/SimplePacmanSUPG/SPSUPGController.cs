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
    public class SPSUPGController : SimplePacmanController
    {

        int timer;

        public SPSUPGController(/*SharpNeatExperiments.Pacman.SimplePacman gameState*/)
        {
            pos = new Point(0, 0);
            //this.gameState = gameState;
            prevPoints = new LinkedList<double>();
            predPoints = new LinkedList<double>();
            predPoints2 = new LinkedList<double>();
        }

        LinkedList<double> prevPoints;
        LinkedList<double> predPoints;
        LinkedList<double> predPoints2;

        float CalculateR2(LinkedList<double> predictions) {
            var mean = prevPoints.Sum() / prevPoints.Count;
            double var = 0;
            foreach(var prevP in prevPoints) {
                var += Math.Pow(prevP - mean, 2);
            }

            double squareError = 0;
            for (int i = 0; i<prevPoints.Count; i++) {
                squareError += Math.Pow(prevPoints.ElementAt(i) - predictions.ElementAt(i), 2);
            }
            float R2 = 0;
            if (var != 0) {
                R2 = 1 - ((float)squareError/(float)var);
            }
            return (float)R2;
        }

        override public void Think() {
            timer ++;
            if (timer > wavelength) {
                timer = 0;
            }
            // set up the override array
            float[] overrideSignals = new float[brain.TotalNeuronCount];
            for (int i = 0; i < overrideSignals.Length; i++)
                overrideSignals[i] = float.MinValue;


            var tempGenome = Substrate.CachedGenome1;
            var tempNet = tempGenome.Decode(null);
            brain = tempNet;
            //FeedInput();
            //float mainFreq = (float)Math.Sin(((float)timer / wavelength) * 2 * 5* Math.PI* brain.GetOutputSignal(4));//brain.GetOutputSignal(4);///*(float)Math.Sin(((float)timer / wavelength) * 2 * Math.PI) * */ gameState.enemies[0].isEdible ? 1 : 0;
            //prevPoints.AddLast(Math.Pow((double)mainFreq - GetSUPGActivationUsingTime(genome.NeuronGeneList[11], timer),2));
            /*var supgOut = (float)Math.Sin(((float)timer / wavelength) * 2 * 5 * Math.PI * 1f);
            var supgOut2 = (float)Math.Sin(((float)timer / wavelength) * 2 * 5 *Math.PI * -1f);//GetSUPGActivationUsingTime(genome.NeuronGeneList[11], timer);
            prevPoints.AddLast(Math.Pow((double)mainFreq - supgOut, 2));
            predPoints.AddLast(Math.Pow((double)mainFreq - supgOut, 2));
            predPoints2.AddLast(Math.Pow((double)mainFreq - supgOut2, 2));*/
            if (prevPoints.Count > 20)
            {
                prevPoints.RemoveFirst();
                predPoints.RemoveFirst();
                predPoints2.RemoveFirst();
            }
            if (predPoints.Sum() < 5) {
                //Console.WriteLine(predPoints.Sum());
            }
            if (predPoints2.Sum() < 5)
            {
                //Console.WriteLine(predPoints2.Sum());
            }
            //mainFreq += Math.Min(GetClosestEnemies2(Direction.Left)[0], 100) / 100f;
            /*mainFreq = brain.GetOutputSignal(4);
            mainFreq *= (float)Math.Sin(((float)timer / wavelength) * 2 * Math.PI);
            for (int i = 0; i < overrideSignals.Length; i++) {
                if (i < 11) continue;
                float supgOutTmp = GetSUPGActivationUsingTime(genome.NeuronGeneList[i], timer);
                if (!IsWithinThreshold(mainFreq,supgOutTmp,0.1f)) {
                    overrideSignals[i] = 0;
                }
            }*/
            /*if (gameState.enemies[0].isEdible) {
                overrideSignals[11] = 0;
                overrideSignals[12] = 0;
                overrideSignals[13] = 0;
            } else {
                overrideSignals[14] = 0;
                overrideSignals[15] = 0;
                overrideSignals[16] = 0;
            }*/
            ((FloatFastConcurrentNetwork)brain).OverrideSignals = overrideSignals;
            SharpNeatExperiments.Pacman.MyForm1.OverrideSignals = overrideSignals;
            var neuron = genome.NeuronGeneList[10];
            //neuron.TimeCounter = (neuron.TimeCounter + 1) % wavelength;
            //Console.WriteLine();

            // float mainFreq = gameState.enemies[0].isEdible ? 1 : -1;//brain.GetOutputSignal(4);
            //mainFreq = (float)Math.Sin(((float)timer / wavelength) * 2 * Math.PI * brain.GetOutputSignal(4));
            //float supgOut = GetSUPGActivationUsingTime(genome.NeuronGeneList[11], timer,3);
            /*var tempGenome = Substrate.CachedGenome1;
            var tempNet = tempGenome.Decode(null);
            brain = tempNet;*/
            /*FeedInput();

            var bla1 = brain.GetOutputSignal(4);


            tempGenome = Substrate.CachedGenome2;
            tempNet = tempGenome.Decode(null);
            brain = tempNet;
            FeedInput();
            var bla2 = brain.GetOutputSignal(4);
            //if (bla1>bla2gameState.enemies[0].isEdible)
            {
                //Console.WriteLine("HEY1");
                tempGenome = Substrate.CachedGenome1;//Substrate.generateGenome(network);
            } else {
                //Console.WriteLine("HEY2");
                tempGenome = Substrate.CachedGenome2;
            }
            tempNet = tempGenome.Decode(null);
            brain = tempNet;*/
            //if (predPoints2.Sum()<predPoints.Sum()/*gameState.enemies[0].isEdible*/)
            /*{
                tempGenome = Substrate.CachedGenome2;
            }
            tempNet = tempGenome.Decode(null);
            brain = tempNet;*/

            SharpNeatExperiments.Pacman.MyForm1.neatGenome = tempGenome;

            double[] outputForDir = new double[4];
            outputForDir[0] = 0;
            outputForDir[1] = 0;
            outputForDir[2] = 0;
            outputForDir[3] = 0;
            Direction[] dirs = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

            FeedInput();
            /*brain.ClearSignals();
            brain.SetInputSignal(0, 1); // bias

            // Used for freq Test
            brain.SetInputSignal(1, Math.Min(GetClosestEnemies2(Direction.Up)[0], 100) / 100f);
            brain.SetInputSignal(2, Math.Min(GetClosestEnemies2(Direction.Down)[0], 100) / 100f);
            brain.SetInputSignal(3, Math.Min(GetClosestEnemies2(Direction.Right)[0], 100) / 100f);
            brain.SetInputSignal(4, Math.Min(GetClosestEnemies2(Direction.Left)[0], 100) / 100f);
            brain.SetInputSignal(5, gameState.enemies[0].isEdible ? 1 : 0);
            brain.MultipleSteps(4);*/

            //var masterFreq = ((((float)timer) / wavelength) > 0.5f) ? 1 : -1;//(float)Math.Sin(((float)timer / wavelength) * 2 * Math.PI*2);
            //var masterFreq = (float)Math.Sin(((float)timer / wavelength) * 2 * Math.PI*2);
            //var masterFreq = (float)Math.Sin(((float)timer / wavelength)  * Math.PI * 2*4+0.5f)*0.7f;
            //var masterFreq = 2 * Math.Exp(-Math.Pow(((float)timer / wavelength) * 2.5, 2)) - 1; // gauss

            //var masterFreq = (float)Math.Sin(((float)timer / wavelength) * 2 * Math.PI);
            //var freq1 = GetSUPGActivationUsingTime(genome.NeuronGeneList[16], timer);
            //gameState.score -= 0.01f * (float)Math.Pow((double)(masterFreq - freq1), (double)2);

            //var masterFreq2 = ((((float)timer) / wavelength) > 0.5f) ? -1 : 1;
            //var masterFreq2 = (float)Math.Sin(((float)timer / wavelength) * 2 * Math.PI * 6);
            //var freq2 = GetSUPGActivationUsingTime(genome.NeuronGeneList[11], timer);
            //gameState.score -= 0.01f * (float)Math.Pow((double)(masterFreq2 - freq2), (double)2);

            //var masterFreq3 = (float)Math.Sin(((float)timer / wavelength) * Math.PI * 2 * 4 + 0.5f) * 0.7f;
            //var freq3 = GetSUPGActivationUsingTime(genome.NeuronGeneList[14], timer);
            //gameState.score -= 0.01f * (float)Math.Pow((double)(masterFreq3 - freq3), (double)2);

            //SharpNeatExperiments.Pacman.MyForm1.freqMaster = masterFreq;//masterFreq; //brain.GetOutputSignal(0);//GetSUPGActivationUsingTime(genome.NeuronGeneList[10], timer); //Math.Sin(timer * 0.05f);
            //SharpNeatExperiments.Pacman.MyForm1.freqMaster2 = masterFreq2;
            //SharpNeatExperiments.Pacman.MyForm1.freqMaster3 = masterFreq3;
            //SharpNeatExperiments.Pacman.MyForm1.freq1 = freq1;//freq1;
            //SharpNeatExperiments.Pacman.MyForm1.freq2 = freq2;//GetSUPGActivationUsingTime(genome.NeuronGeneList[14], timer);
            //SharpNeatExperiments.Pacman.MyForm1.freq3 = freq3;

            outputForDir[0] = brain.GetOutputSignal(0);
            outputForDir[1] = brain.GetOutputSignal(1);
            outputForDir[2] = brain.GetOutputSignal(2);
            outputForDir[3] = brain.GetOutputSignal(3);

            /*Console.WriteLine(brain.GetOutputSignal(0));
            Console.WriteLine(brain.GetOutputSignal(1));
            Console.WriteLine(brain.GetOutputSignal(2));
            Console.WriteLine(brain.GetOutputSignal(3));
            Console.WriteLine("");*/
            TranslateOutputForBrain(outputForDir);
        }

        void FeedInput() {
            brain.ClearSignals();
            brain.SetInputSignal(0, 1); // bias
            brain.SetInputSignal(1, Math.Min(GetClosestEnemies2(Direction.Up)[0], 100) / 100f);
            brain.SetInputSignal(2, Math.Min(GetClosestEnemies2(Direction.Down)[0], 100) / 100f);
            brain.SetInputSignal(3, Math.Min(GetClosestEnemies2(Direction.Right)[0], 100) / 100f);
            brain.SetInputSignal(4, Math.Min(GetClosestEnemies2(Direction.Left)[0], 100) / 100f);
            brain.SetInputSignal(5, gameState.enemies[0].isEdible ? 1 : 0);
            brain.MultipleSteps(4);
        }
    }
}
