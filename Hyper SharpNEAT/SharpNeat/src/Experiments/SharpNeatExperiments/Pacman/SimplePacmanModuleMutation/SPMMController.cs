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
    public class SPMMController : SimplePacmanController
    {

        public SPMMController(/*SharpNeatExperiments.Pacman.SimplePacman gameState*/)
        {
            pos = new Point(0, 0);
            //this.gameState = gameState;
        }

        override public void Think() {
            //SharpNeatExperiments.Pacman.MyForm1.NeuronsToLightUp.Clear();

            double[] outputForDir = new double[4];
            outputForDir[0] = 0;
            outputForDir[1] = 0;
            outputForDir[2] = 0;
            outputForDir[3] = 0;
            Direction[] dirs = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
            /*foreach (Direction dir in dirs) {
                brain.ClearSignals();
                brain.SetInputSignal(0, 1); // bias
                var closestEnemies = GetClosestEnemies2(dir);
                brain.SetInputSignal(1, Math.Min(closestEnemies[0], 100) / 100f);
                brain.SetInputSignal(2, Math.Min(closestEnemies[1], 100) / 100f);
                brain.SetInputSignal(3, 0);
                brain.SetInputSignal(4, 0);
                //brain.SetInputSignal(3, Math.Min(closestEnemies[2], 100) / 100f);
                //brain.SetInputSignal(4, Math.Min(closestEnemies[3], 100) / 100f);
                brain.SetInputSignal(5, gameState.enemies[0].isEdible ? 1 : 0);
                brain.MultipleSteps(10);

                if (brain.GetOutputSignal(1) > brain.GetOutputSignal(2)) {
                    //SharpNeatExperiments.Pacman.MyForm1.NeuronsToLightUp.Add(6);
                    //Console.WriteLine("a");
                    outputForDir[(int)dir] = brain.GetOutputSignal(0);
                } else {
                    //SharpNeatExperiments.Pacman.MyForm1.NeuronsToLightUp.Add(7);
                    //Console.WriteLine("b");
                    outputForDir[(int)dir] = brain.GetOutputSignal(3);
                }
            }*/
            var tempGenome = Substrate.CachedGenome1;
            if (gameState.enemies[0].isEdible) {
                tempGenome = Substrate.CachedGenome2;//Substrate.generateGenome(network);
            }
            SharpNeatExperiments.Pacman.MyForm1.neatGenome = tempGenome;
            var tempNet = tempGenome.Decode(null);

            brain = tempNet;

            brain.ClearSignals();
            brain.SetInputSignal(0, 1); // bias
            brain.SetInputSignal(1, Math.Min(GetClosestEnemies2(Direction.Up)[0], 100) / 100f);
            brain.SetInputSignal(2, Math.Min(GetClosestEnemies2(Direction.Down)[0], 100) / 100f);
            brain.SetInputSignal(3, Math.Min(GetClosestEnemies2(Direction.Right)[0], 100) / 100f);
            brain.SetInputSignal(4, Math.Min(GetClosestEnemies2(Direction.Left)[0], 100) / 100f);
            brain.SetInputSignal(5, gameState.enemies[0].isEdible ? 1 : 0);
            brain.MultipleSteps(4);

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
