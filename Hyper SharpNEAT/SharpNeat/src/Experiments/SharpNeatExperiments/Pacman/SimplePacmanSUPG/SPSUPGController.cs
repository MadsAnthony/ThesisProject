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

            float mainFreq = brain.GetOutputSignal(4);
            for (int i = 0; i < overrideSignals.Length; i++) {
                if (i < 11) continue;
                float supgOut = GetSUPGActivationUsingTime(genome.NeuronGeneList[10], timer);
                if (IsWithinThreshold(mainFreq,supgOut,0.2f)) {
                    overrideSignals[i] = 0;
                }
            }
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

            /*var tempGenome = Substrate.CachedGenome1;
            if (gameState.enemies[0].isEdible)
            {
                tempGenome = Substrate.CachedGenome2;//Substrate.generateGenome(network);
            }
            SharpNeatExperiments.Pacman.MyForm1.neatGenome = tempGenome;
            var tempNet = tempGenome.Decode(null);
            brain = tempNet;*/

            double[] outputForDir = new double[4];
            outputForDir[0] = 0;
            outputForDir[1] = 0;
            outputForDir[2] = 0;
            outputForDir[3] = 0;
            Direction[] dirs = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

            brain.ClearSignals();
            brain.SetInputSignal(0, 1); // bias
            brain.SetInputSignal(1, Math.Min(GetClosestEnemies2(Direction.Up)[0], 100) / 100f);
            brain.SetInputSignal(2, Math.Min(GetClosestEnemies2(Direction.Down)[0], 100) / 100f);
            brain.SetInputSignal(3, Math.Min(GetClosestEnemies2(Direction.Right)[0], 100) / 100f);
            brain.SetInputSignal(4, Math.Min(GetClosestEnemies2(Direction.Left)[0], 100) / 100f);
            brain.SetInputSignal(5, gameState.enemies[0].isEdible ? 1 : 0);
            brain.MultipleSteps(4);

            var masterFreq = brain.GetOutputSignal(4);//((((float)timer+25) / wavelength) > 0.5f) ? 1 : -0.7;//(float)Math.Sin(((float)timer / wavelength) * 2 * Math.PI*2);
            var freq1 = GetSUPGActivationUsingTime(genome.NeuronGeneList[10], timer);
            //gameState.score -= 0.2f * (float)Math.Pow((double)(masterFreq - freq1), (double)2);
            SharpNeatExperiments.Pacman.MyForm1.freqMaster = masterFreq; //brain.GetOutputSignal(0);//GetSUPGActivationUsingTime(genome.NeuronGeneList[10], timer); //Math.Sin(timer * 0.05f);
            SharpNeatExperiments.Pacman.MyForm1.freq1 = freq1;
            //SharpNeatExperiments.Pacman.MyForm1.freq2 = GetSUPGActivationUsingTime(genome.NeuronGeneList[14], timer);

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
    }
}
