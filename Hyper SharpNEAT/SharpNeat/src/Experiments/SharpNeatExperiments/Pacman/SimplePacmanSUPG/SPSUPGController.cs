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

            /*var tempGenome = Substrate.CachedGenome1;
            var tempNet = tempGenome.Decode(null);
            brain = tempNet;
            FeedInput();

            //float mainFreq = (float)Math.Sin(((float)timer / wavelength) * 2 * 5* Math.PI* brain.GetOutputSignal(4));//brain.GetOutputSignal(4);

            float mainFreq = brain.GetOutputSignal(4);

            // set up the override array
            float[] overrideSignals = new float[brain.TotalNeuronCount];
            for (int i = 0; i < overrideSignals.Length; i++)
                overrideSignals[i] = float.MinValue;

            for (int i = 0; i < overrideSignals.Length; i++) {
                if (i < 11) continue;
                float supgOutTmp = GetSUPGActivationUsingTime(genome.NeuronGeneList[i], timer,2);
                //if (!IsWithinThreshold(mainFreq,supgOutTmp,0.1f)) {
                    //overrideSignals[i] = 0;
                //}
                overrideSignals[i] = supgOutTmp - mainFreq;
            }

            ((FloatFastConcurrentNetwork)brain).OverrideSignals = overrideSignals;
            SharpNeatExperiments.Pacman.MyForm1.OverrideSignals = overrideSignals;
            var neuron = genome.NeuronGeneList[10];*/

            // USE for 2M

            /*brain = (Substrate.CachedGenome1).Decode(null);
            FeedInput();
            var pref1 = brain.GetOutputSignal(4);

            brain = (Substrate.CachedGenome2).Decode(null);
            FeedInput();
            var pref2 = brain.GetOutputSignal(4);*/
            //brain = (Substrate.CachedGenome1).Decode(null);
            if (/*pref1>pref2*/gameState.enemies[0].isEdible) {
                brain = (Substrate.CachedGenome1).Decode(null);
            } else {
                brain = (Substrate.CachedGenome2).Decode(null);
            }


            //var tempNet = tempGenome.Decode(null);
            //brain = tempNet;

            //SharpNeatExperiments.Pacman.MyForm1.neatGenome = tempGenome;

            double[] outputForDir = new double[4];
            outputForDir[0] = 0;
            outputForDir[1] = 0;
            outputForDir[2] = 0;
            outputForDir[3] = 0;
            Direction[] dirs = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

            FeedInput();

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
            float clampDist = 40f;
            brain.SetInputSignal(1, Math.Min(GetClosestEnemies2(Direction.Up)[0], clampDist) / clampDist);
            brain.SetInputSignal(2, Math.Min(GetClosestEnemies2(Direction.Down)[0], clampDist) / clampDist);
            brain.SetInputSignal(3, Math.Min(GetClosestEnemies2(Direction.Right)[0], clampDist) / clampDist);
            brain.SetInputSignal(4, Math.Min(GetClosestEnemies2(Direction.Left)[0], clampDist) / clampDist);
            brain.SetInputSignal(5, gameState.enemies[0].isEdible ? 1 : 0);
            brain.MultipleSteps(4);
        }
    }
}
