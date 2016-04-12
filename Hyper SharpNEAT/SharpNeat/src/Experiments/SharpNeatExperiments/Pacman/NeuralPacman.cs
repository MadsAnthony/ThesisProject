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
	public class NeuralPacman : BasePacman
	{
        INetwork brain;
        GameState gameState;
        Node[,] ghostsPrevNode = new Node[4, 6];
		public NeuralPacman() : base("NeuralPacman") {			
		}

        public void SetBrain(INetwork network) {
            brain = network;
        }

		public override Direction Think(GameState gs) {
			/*List<Direction> possible = gs.Pacman.PossibleDirections();			
			if( possible.Count > 0 ) {
				int select = GameState.Random.Next(0, possible.Count);
				if( possible[select] != gs.Pacman.InverseDirection(gs.Pacman.Direction) )
					return possible[select];
			}*/
            gameState = gs;

            double[] outputForDir = new double[4];
            outputForDir[0] = 0;
            outputForDir[1] = 0;
            outputForDir[2] = 0;
            outputForDir[3] = 0;

            foreach (Direction dir in gs.Pacman.PossibleDirections()) {
                double[] closestGhosts = GetClosestGhosts(dir);

                brain.ClearSignals();

                // *****************
                // Common Undirected Sensors (8 inputs)
                // *****************

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

                //brain.SingleStep();
                brain.MultipleSteps(10);

                // Use two Modules
                double[] preferenceNeurons = new double[2];
                preferenceNeurons[0] = brain.GetOutputSignal(1);
                preferenceNeurons[1] = brain.GetOutputSignal(3);

                outputForDir[(int)dir] = /*brain.GetOutputSignal(0);*/brain.GetOutputSignal(GetModuleIndex(preferenceNeurons));
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
