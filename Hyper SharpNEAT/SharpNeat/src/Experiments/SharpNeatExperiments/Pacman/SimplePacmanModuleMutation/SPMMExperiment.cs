using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib.Evolution;
using SharpNeatLib.NeuralNetwork;

namespace SharpNeatLib.Experiments
{
    public class SPMMExperiment : IExperiment 
    {
        private uint inputs;
        private uint outputs;
        private uint hidden;
        private int cppnInputs;
        private int cppnOutputs;
        IPopulationEvaluator populationEvaluator;
        IActivationFunction activationFunction = new SteepenedSigmoid();
        private NeatParameters neatParams = null;

        #region IExperiment Members

        public SPMMExperiment(uint inputs, uint outputs, uint hidden, int cppnInputs, int cppnOutputs)
        {
            this.inputs = inputs;
            this.outputs = outputs;
            this.hidden = hidden;
            this.cppnInputs = cppnInputs;
            this.cppnOutputs = cppnOutputs;
        }

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
            populationEvaluator = new SPMMPopulationEvaluator(new SPMMNetworkEvaluator(), activationFn);
        }

        public int InputNeuronCount
        {
            get { return cppnInputs;}
        }

        public int OutputNeuronCount
        {
            get { return cppnOutputs; }
        }

        public NeatParameters DefaultNeatParameters
        {
            get
            {
                if (neatParams == null)
                {
                    NeatParameters np = new NeatParameters();
                    np.activationProbabilities = new double[4];
                    np.activationProbabilities[0] = .25;
                    np.activationProbabilities[1] = .25;
                    np.activationProbabilities[2] = .25;
                    np.activationProbabilities[3] = .25;
                    np.compatibilityDisjointCoeff = 1;
                    np.compatibilityExcessCoeff = 1;
                    np.compatibilityThreshold = 100;
                    np.compatibilityWeightDeltaCoeff = 3;
                    np.connectionWeightRange = 3;
                    np.elitismProportion = .1;
                    np.pInitialPopulationInterconnections = 1;
                    np.pInterspeciesMating = 0.01;
                    np.pMutateAddConnection = .06;
                    np.pMutateAddNode = .81;
                    np.pMutateConnectionWeights = .16;
                    np.pMutateDeleteConnection = 0;
                    np.pMutateDeleteSimpleNeuron = 0;
                    np.populationSize = 300;
                    np.pruningPhaseBeginComplexityThreshold = float.MaxValue;
                    np.pruningPhaseBeginFitnessStagnationThreshold = int.MaxValue;
                    np.pruningPhaseEndComplexityStagnationThreshold = int.MinValue;
                    np.selectionProportion = .8;
                    np.speciesDropoffAge = 1500;
                    np.targetSpeciesCountMax = np.populationSize / 10;
                    np.targetSpeciesCountMin = np.populationSize / 10 - 2;
                    np.pInitialPopulationInterconnections = 0.5f;

                    neatParams = np;
                }
                return neatParams;
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
