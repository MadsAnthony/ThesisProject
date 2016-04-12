using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib.Evolution;
using SharpNeatLib.NeuralNetwork;

namespace SharpNeatLib.Experiments
{
    public class FoodGathererExperiment : IExperiment 
    {
        IPopulationEvaluator populationEvaluator;
        IActivationFunction activationFunction = new SteepenedSigmoid();

        #region IExperiment Members

        public void LoadExperimentParameters(System.Collections.Hashtable parameterTable)
        {
            
        }

        public IPopulationEvaluator PopulationEvaluator
        {
            get
            {
                if (populationEvaluator == null)
                    ResetEvaluator(activationFunction);

                return populationEvaluator;
            }
        }

        public void ResetEvaluator(IActivationFunction activationFn)
        {
           // populationEvaluator = new SingleFilePopulationEvaluator(new RobotDualNetworkEvaluator(), activationFn);
            populationEvaluator = new FoodGathererPopulationEvaluator(new FoodGathererNetworkEvaluator(), activationFn);
        }

        public int InputNeuronCount
        {
            get { return FoodGatherParams.distance ? 5 : 4; }
        }

        public int OutputNeuronCount
        {
            get { return 1; }
        }

        public NeatParameters DefaultNeatParameters
        {
            get
            {
                NeatParameters np = new NeatParameters();
                np.connectionWeightRange = 3;
                np.pMutateAddConnection = .03;
                np.pMutateAddNode = .01;
                np.pMutateConnectionWeights = .96;
                np.pMutateDeleteConnection = 0;
                np.pMutateDeleteSimpleNeuron = 0;
               /* np.populationSize = 150;
                np.pOffspringAsexual = 0.8;
                np.pOffspringSexual = 0.2;

                np.targetSpeciesCountMin = 40;
                np.targetSpeciesCountMax = 50;*/

                return np;
            }
        }

        public IActivationFunction SuggestedActivationFunction
        {
            get { return activationFunction; }
        }

        public AbstractExperimentView CreateExperimentView()
        {
            //return null;
            return new Form1();
           // return new NetworkWiring();
        }

        public string ExplanatoryText
        {
            get { return "does stuff and//or things."; }
        }

        #endregion
    }
}
