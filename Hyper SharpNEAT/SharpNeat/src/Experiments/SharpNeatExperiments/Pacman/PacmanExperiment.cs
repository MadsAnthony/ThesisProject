using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib.Evolution;
using SharpNeatLib.NeuralNetwork;

namespace SharpNeatLib.Experiments
{
    public class PacmanExperiment : IExperiment 
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
            populationEvaluator = new PacmanPopulationEvaluator(new PacmanNetworkEvaluator(), activationFn);
        }

        public int InputNeuronCount
        {
            get { return 30;/*FoodGatherParams.distance ? 5 : 4;*/ }
        }

        public int OutputNeuronCount
        {
            get { return 4; }
        }

        public NeatParameters DefaultNeatParameters
        {
            get
            {
                NeatParameters np = new NeatParameters();
                np.connectionWeightRange = 3;
                np.pMutateAddConnection = .40;
                np.pMutateAddNode = .20;
                np.pMutateConnectionWeights = .40;
                np.pMutateDeleteConnection = 0;
                np.pMutateDeleteSimpleNeuron = 0;
                np.targetSpeciesCountMin = 6;
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
