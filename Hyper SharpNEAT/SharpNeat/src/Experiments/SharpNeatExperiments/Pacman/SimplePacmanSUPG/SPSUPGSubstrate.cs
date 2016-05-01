//#define OUTPUT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeatLib.CPPNs;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;


class SPSUPGSubstrate : Substrate
    {
    private const float shiftScale = 0.2f;

    public SPSUPGSubstrate(uint inputs, uint outputs, uint hidden, IActivationFunction function)
        : base(inputs, outputs, hidden, function)
    {

    }

    public override NeatGenome generateGenome(INetwork network)
    {
        CachedGenome1 = MakeGenome(network, 0);
        CachedGenome2 = MakeGenome(network, 1);
        return CachedGenome1;
    }

    public NeatGenome MakeGenome(INetwork network, int moduleI) {
        // copy the neuron list to a new list and update the x/y values
        NeuronGeneList newNeurons = new NeuronGeneList(neurons);

        // set the x and y value of the SUPGs
        foreach (NeuronGene neuron in newNeurons)
        {
            Point point = GetCustomPos(neuron.InnovationId);
            neuron.XValue = point.X;
            neuron.YValue = point.Y;
            /*if (neuron.NeuronType != NeuronType.Input) {
                neuron.ActivationFunction = new SteepenedSigmoid();
            }*/
            //neuron.TimeConstant = 1;
            //neuron.NeuronBias = GetNeuronBias(network, neuron, moduleI+1);
        }

        ConnectionGeneList connections = new ConnectionGeneList((int)((inputCount * hiddenCount) + (hiddenCount * outputCount)));
        float[] coordinates = new float[5];
        float output;
        uint connectionCounter = 0;
        int iterations = 2 * (network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount)) + 1;

        // Connections from input to hidden
        for (uint source = 0; source < inputCount; source++)
        {
            QueryConnection(network, connections, connectionCounter++, source, 11, moduleI, newNeurons);
            QueryConnection(network, connections, connectionCounter++, source, 12, moduleI, newNeurons);
            QueryConnection(network, connections, connectionCounter++, source, 13, moduleI, newNeurons);
            QueryConnection(network, connections, connectionCounter++, source, 14, moduleI, newNeurons);
            QueryConnection(network, connections, connectionCounter++, source, 15, moduleI, newNeurons);
            QueryConnection(network, connections, connectionCounter++, source, 16, moduleI, newNeurons);

            // output
            /*QueryConnection(network, connections, connectionCounter++, source, 6, moduleI, newNeurons);
            QueryConnection(network, connections, connectionCounter++, source, 7, moduleI, newNeurons);
            QueryConnection(network, connections, connectionCounter++, source, 8, moduleI, newNeurons);
            QueryConnection(network, connections, connectionCounter++, source, 9, moduleI, newNeurons);*/

            // Special Output
            //QueryConnection(network, connections, connectionCounter++, source, 10, 2, newNeurons);
        }
        QueryConnection(network, connections, connectionCounter++, 5, 10, 2, newNeurons);
        // Connection from input to output
        /*QueryConnection(network, connections, connectionCounter++, 5, 6, newNeurons);
        QueryConnection(network, connections, connectionCounter++, 5, 7, newNeurons);*/
        // Connections from hidden to output
        for (uint source = 0; source < hiddenCount; source++)
        {
            uint tmpSource = source + inputCount + outputCount;
            QueryConnection(network, connections, connectionCounter++, tmpSource, 6, moduleI, newNeurons);
            QueryConnection(network, connections, connectionCounter++, tmpSource, 7, moduleI, newNeurons);
            QueryConnection(network, connections, connectionCounter++, tmpSource, 8, moduleI, newNeurons);
            QueryConnection(network, connections, connectionCounter++, tmpSource, 9, moduleI, newNeurons);

            // Special Output
            //QueryConnection(network, connections, connectionCounter++, tmpSource, 10, 2, newNeurons);
        }
        return new SharpNeatLib.NeatGenome.NeatGenome(0, newNeurons, connections, (int)inputCount, (int)outputCount);
    }

    void QueryConnection(INetwork network, ConnectionGeneList connections, uint connectionCounter, uint neuron1id, uint neuron2id, int moduleI, NeuronGeneList newNeurons) {
        int iterations = 2 * (network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount)) + 1;

        network.ClearSignals();
        //network.SetInputSignal(0, 1);
        network.SetInputSignal(0, newNeurons[(int)neuron1id].XValue);
        network.SetInputSignal(1, newNeurons[(int)neuron1id].YValue);
        network.SetInputSignal(2, newNeurons[(int)neuron2id].XValue);
        network.SetInputSignal(3, newNeurons[(int)neuron2id].YValue);
        network.SetInputSignal(4, 1);
        network.MultipleSteps(iterations);

        float output = network.GetOutputSignal(moduleI);
        if (Math.Abs(output) > threshold) {
            float weight = (float)(((Math.Abs(output) - (threshold)) / (1 - threshold)) * weightRange * Math.Sign(output));
            connections.Add(new ConnectionGene(connectionCounter, neuron1id, neuron2id, weight));
        }
    }

    float GetNeuronBias(INetwork network, NeuronGene neuron, int moduleI)
    {
        int iterations = 2 * (network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount)) + 1;

        network.ClearSignals();
        //network.SetInputSignal(0, 1);
        network.SetInputSignal(0, neuron.XValue);
        network.SetInputSignal(1, neuron.YValue);
        network.SetInputSignal(2, 0);
        network.SetInputSignal(3, 0);
        network.SetInputSignal(4, 0);
        network.MultipleSteps(iterations);

        float output = network.GetOutputSignal(moduleI);
        return output;
    }

    double getActivation(INetwork network, uint neuron1id, uint neuron2id, NeuronGeneList newNeurons) {
        network.ClearSignals();
        //network.SetInputSignal(0, 1);
        network.SetInputSignal(0, newNeurons[(int)neuron1id].XValue);
        network.SetInputSignal(1, newNeurons[(int)neuron1id].YValue);
        network.SetInputSignal(2, newNeurons[(int)neuron2id].XValue);
        network.SetInputSignal(3, newNeurons[(int)neuron2id].YValue);
        network.SetInputSignal(4, 0);
        network.MultipleSteps(10);

        float output = network.GetOutputSignal(0);

        float weight = (float)(((Math.Abs(output) - (threshold)) / (1 - threshold)) * weightRange * Math.Sign(output));
        return weight;
    }

    Point GetCustomPos(uint index) {
        Point point = new Point(0, 0);
        switch (index)
        {
            // input nodes
            case 0:
                point = new Point(-0.2f, -0.8f);
                break;
            case 1:
                point = new Point(-0.4f, -0.5f);
                break;
            case 2:
                point = new Point(-0.8f, -0.5f);
                break;
            case 3:
                point = new Point(0.4f, -0.5f);
                break;
            case 4:
                point = new Point(0.8f, -0.5f);
                break;
            case 5:
                point = new Point(0.2f, -0.8f);
                break;
            // hidden nodes
            case 11:
                point = new Point(-0.9f, 0.2f);
                break;
            case 12:
                point = new Point(-0.6f, 0.2f);
                break;
            case 13:
                point = new Point(-0.3f, 0.2f);
                break;
            case 14:
                point = new Point(0.3f, 0.2f);
                break;
            case 15:
                point = new Point(0.6f, 0.2f);
                break;
            case 16:
                point = new Point(0.9f, 0.2f);
                break;
            // output nodes
            case 6:
                point = new Point(-0.8f, 0.8f);
                break;
            case 7:
                point = new Point(-0.4f, 0.8f);
                break;
            case 8:
                point = new Point(0.4f, 0.8f);
                break;
            case 9:
                point = new Point(0.8f, 0.8f);
                break;
            case 10:
                point = new Point(0.1f, 0.7f);
                break;
        }
        return point;
    }

    private class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
    // returns a map that signifies which trigger maps to which hidden neurons.. a value of float.min means that neuron has no trigger
    // any other value indicates the foot which triggers that given neuron.  example: map[16] = 0 means foot 0 triggers neuron 16
    public int[] getSUPGMap()
    {
        int[] map = new int[28];
        for (int i = 0; i < 16; i++)
            map[i] = int.MinValue;
        map[16] = 0;
        map[17] = 0;
        map[18] = 0;
        map[19] = 1;
        map[20] = 1;
        map[21] = 1;
        map[22] = 2;
        map[23] = 2;
        map[24] = 2;
        map[25] = 3;
        map[26] = 3;
        map[27] = 3;
        return map;
    }
}
