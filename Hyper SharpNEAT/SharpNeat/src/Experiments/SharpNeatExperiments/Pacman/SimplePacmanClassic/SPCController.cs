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
    public class SPCController : SimplePacmanController
    {

        public SPCController(/*SharpNeatExperiments.Pacman.SimplePacman gameState*/)
        {
            pos = new Point(0, 0);
            //this.gameState = gameState;
        }

        override public void Think() {
            double[] outputForDir = new double[4];
            outputForDir[0] = 0;
            outputForDir[1] = 0;
            outputForDir[2] = 0;
            outputForDir[3] = 0;
            Direction[] dirs = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
            foreach (Direction dir in dirs) {
                brain.ClearSignals();
                brain.SetInputSignal(0, 1); // bias
                var closestEnemies = GetClosestEnemies2(dir);
                brain.SetInputSignal(1, Math.Min(closestEnemies[0], 100) / 100f);
                brain.SetInputSignal(2, Math.Min(closestEnemies[1], 100) / 100f);
                brain.SetInputSignal(3, Math.Min(closestEnemies[2], 100) / 100f);
                brain.SetInputSignal(4,1);
                //brain.SetInputSignal(4, Math.Min(closestEnemies[3], 100) / 100f);
                brain.SetInputSignal(5, gameState.enemies[0].isEdible ? 1 : 0);
                brain.MultipleSteps(10);
                
                outputForDir[(int)dir] = brain.GetOutputSignal(0);
            }
            TranslateOutputForBrain(outputForDir);
        }
    }
}
