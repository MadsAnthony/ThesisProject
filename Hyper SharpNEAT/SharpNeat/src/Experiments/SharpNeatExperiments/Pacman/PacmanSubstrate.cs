using System;
using System.Collections.Generic;
using System.Text;
using SharpNeatLib.CPPNs;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;

namespace SharpNeatLib.Experiments
{
    class PacmanSubstrate : Substrate
    {
        public bool concentric=false;

        public PacmanSubstrate(uint inputs, uint outputs, uint hidden, bool circle) : base(inputs,outputs,hidden,ActivationFunctionFactory.GetActivationFunction("SteepenedSigmoid"))
        {
            concentric=circle;
        }

        public override SharpNeatLib.NeatGenome.NeatGenome generateGenome(SharpNeatLib.NeuralNetwork.INetwork CPPN)
        {
 	        if(concentric)
                return CPPN.InputNeuronCount==5 ? generatePerceptronCircle(CPPN,true) : generatePerceptronCircle(CPPN,false);
            else
                return CPPN.InputNeuronCount==5 ? generatePerceptronPattern(CPPN,true) : generatePerceptronPattern(CPPN,false);
        }

        public NeatGenome.NeatGenome generatePerceptronCircle(INetwork network, bool distance)
        {
            ConnectionGeneList connections = new ConnectionGeneList((int)(inputCount*outputCount));

            double[] inputs;
            if (distance)//|| angle)
                inputs = new double[5];
            else
                inputs = new double[4];
            double output;
            uint counter = 0;
            double inputAngleDelta = (2 * Math.PI) / inputCount;
            double outputAngleDelta = (2 * Math.PI) / outputCount;

            double angleFrom = -3 * Math.PI / 4;
            for (uint neuronFrom = 0; neuronFrom < inputCount; neuronFrom++, angleFrom += inputAngleDelta)
            {
                inputs[0] = .5 * Math.Cos(angleFrom + (inputAngleDelta / 2.0));
                inputs[1] = .5 * Math.Sin(angleFrom + (inputAngleDelta / 2.0));
                double angleTo = -3 * Math.PI / 4;
                for (uint neuronTo = 0; neuronTo < outputCount; neuronTo++, angleTo += outputAngleDelta)
                {
                    inputs[2] = Math.Cos(angleTo + (outputAngleDelta / 2.0));
                    inputs[3] = Math.Sin(angleTo + (outputAngleDelta / 2.0));
                    //if(angle)
                        //inputs[4] = Math.Abs(angleFrom - angleTo);
                    if(distance)
                        inputs[4]=((Math.Sqrt(Math.Pow(inputs[0] - inputs[2], 2) + Math.Pow(inputs[1] - inputs[3], 2)) / (2*sqrt2)));
                    network.ClearSignals();
                    network.SetInputSignals(inputs);
                    network.MultipleSteps(5);
                    output = network.GetOutputSignal(0);
                    if (Math.Abs(output) > threshold)
                    {
                        float weight = (float)(((Math.Abs(output) - (threshold)) / (1 - threshold)) * weightRange * Math.Sign(output));
                        connections.Add(new ConnectionGene(counter++, neuronFrom, neuronTo + inputCount, weight));
                    }

                }
            }
            NeatGenome.NeatGenome g = new NeatGenome.NeatGenome(0, neurons, connections, (int)inputCount, (int)outputCount);
            return g;
        }

        public NeatGenome.NeatGenome generatePerceptronPattern(INetwork network, bool distance)
        {
            ConnectionGeneList connections = new ConnectionGeneList((int)(inputCount * outputCount));

            double[] inputs;
            if (distance)
                inputs = new double[5];
            else
                inputs = new double[4];
            //for this particular config, these inputs will never change so just set them now
            inputs[1] = 1;
            inputs[3] = -1;
            uint counter = 0;
            double output;
            double x1 = -1, x2 = -1;

            double inputDelta=(2.0 / (inputCount - 1));
            double outputDelta = (2.0 / (outputCount - 1));

            for (uint nodeFrom = 0; nodeFrom < inputCount; nodeFrom++, x1 +=inputDelta )
            {
                inputs[0] = x1;
                x2 = -1;
                for (uint nodeTo = 0; nodeTo < outputCount; nodeTo++, x2 += outputDelta)
                {
                    inputs[2] = x2;
                    if(distance)
                        inputs[4] = ((Math.Sqrt(Math.Pow(inputs[0] - inputs[2], 2) + Math.Pow(inputs[1] - inputs[3], 2)) / (2*sqrt2)));
                    network.ClearSignals();
                    network.SetInputSignals(inputs);
                    //currenly assuming a depth no greater than 5
                    network.MultipleSteps(5);
                    output = network.GetOutputSignal(0);
                    if (Math.Abs(output) > threshold)
                    {
                        float weight = (float)(((Math.Abs(output) - (threshold)) / (1 - threshold)) * weightRange * Math.Sign(output));
                        connections.Add(new ConnectionGene(counter++, nodeFrom, nodeTo + inputCount, weight));
                    }
                }

            }
            NeatGenome.NeatGenome g = new SharpNeatLib.NeatGenome.NeatGenome(0, neurons, connections, (int)inputCount, (int)outputCount);
            return g;

        }
    }
}
