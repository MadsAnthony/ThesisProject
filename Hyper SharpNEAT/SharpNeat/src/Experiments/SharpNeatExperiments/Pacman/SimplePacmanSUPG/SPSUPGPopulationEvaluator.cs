using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib.Evolution;
using SharpNeatLib.NeuralNetwork;

namespace SharpNeatLib.Experiments
{
    public class SPSUPGPopulationEvaluator : SingleFilePopulationEvaluator
    {
        public static bool requestResolutionUp = false;
        public static bool requestResolutionDown = false;
        public SPSUPGPopulationEvaluator(INetworkEvaluator eval,IActivationFunction act) : base(eval,act)
        {
            activationFn = act;
        }
        public override void EvaluatePopulation(Population pop, EvolutionAlgorithm ea)
        {
            // Evaluate in single-file each genome within the population. 
            // Only evaluate new genomes (those with EvaluationCount==0).
            FoodGatherParams.fillLookups();
            FoodGatherParams.fillFood();
            int count = pop.GenomeList.Count;
            for (int i = 0; i < count; i++)
            {
                IGenome g = pop.GenomeList[i];
                if (g.EvaluationCount != 0)
                    continue;

                INetwork network = g.Decode(activationFn);
                if (network == null)
                {	// Future genomes may not decode - handle the possibility.
                    g.Fitness = EvolutionAlgorithm.MIN_GENOME_FITNESS;
                }
                else
                {
                    bool isNSGAiiEnabled = false;
                    if (isNSGAiiEnabled) {
                        double[] resultFitness = networkEvaluator.EvaluateNetworkMultipleObjective(network);
                        g.Fitness = Math.Max(resultFitness[0], EvolutionAlgorithm.MIN_GENOME_FITNESS);
                        g.ObjectiveFitness = g.Fitness;
                        g.MultiObjectiveFitness = new double[]{resultFitness[1],resultFitness[2]};
                    } else {
                        g.Fitness = Math.Max(networkEvaluator.EvaluateNetwork(network), EvolutionAlgorithm.MIN_GENOME_FITNESS);
                        g.ObjectiveFitness = g.Fitness;
                    }
                }

                // Reset these genome level statistics.
                g.TotalFitness = g.Fitness;
                g.EvaluationCount = 1;

                // Update master evaluation counter.
                evaluationCount++;
            }
            if (requestResolutionUp == true)
            {
                requestResolutionUp = false;
                requestResolutionDown = false;
                FoodGatherParams.resolution *= 2;
            }
            else if (requestResolutionDown == true)
            {
                requestResolutionUp = false;
                requestResolutionDown = false;
                if(FoodGatherParams.resolution>4)
                    FoodGatherParams.resolution /= 2;
            }
        }
    }
}
