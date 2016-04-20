//#define OUTPUT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeatLib.CPPNs;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;


class SPMMSubstrate : Substrate
    {
    private const float shiftScale = 0.2f;

    public SPMMSubstrate(uint inputs, uint outputs, uint hidden, IActivationFunction function)
        : base(inputs, outputs, hidden, function)
    {

    }

    public override NeatGenome generateGenome(INetwork network)
    {
        // copy the neuron list to a new list and update the x/y values
        NeuronGeneList newNeurons = new NeuronGeneList(neurons);

        // set the x and y value of the SUPGs
        foreach (NeuronGene neuron in newNeurons)
        {
            Point point = GetCustomPos(neuron.InnovationId);
            neuron.XValue = point.X;
            neuron.YValue = point.Y;
            if (neuron.NeuronType != NeuronType.Input) {
                neuron.ActivationFunction = new SteepenedSigmoid();
            }
        }

        ConnectionGeneList connections = new ConnectionGeneList((int)((inputCount * hiddenCount) + (hiddenCount * outputCount)));
        float[] coordinates = new float[5];
        float output;
        uint connectionCounter = 0;
        int iterations = 2 * (network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount)) + 1;

        // Connections from input to hidden
        for (uint source = 1; source < inputCount-1; source++)
        {
            connections.Add(new ConnectionGene(connectionCounter++, source, 10, getActivation(network, source, 10, newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, source, 11, getActivation(network, source, 11, newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, source, 12, getActivation(network, source, 12, newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, source, 13, getActivation(network, source, 13, newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, source, 14, getActivation(network, source, 14, newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, source, 15, getActivation(network, source, 15, newNeurons)));
        }
        // Connection from input to output
        connections.Add(new ConnectionGene(connectionCounter++, 0, 7, getActivation(network, 0, 7, newNeurons)));
        connections.Add(new ConnectionGene(connectionCounter++, 5, 8, getActivation(network, 5, 8, newNeurons)));
        // Connections from hidden to output
        for (uint source = 0; source < hiddenCount; source++)
        {
            uint tmpSource = source + inputCount + outputCount;
            connections.Add(new ConnectionGene(connectionCounter++, tmpSource, 6, getActivation(network, tmpSource, 6, newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, tmpSource, 7, getActivation(network, tmpSource, 7, newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, tmpSource, 8, getActivation(network, tmpSource, 8, newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, tmpSource, 9, getActivation(network, tmpSource, 9, newNeurons)));
        }

        return new SharpNeatLib.NeatGenome.NeatGenome(0, newNeurons, connections, (int)inputCount, (int)outputCount);
    }

    double getActivation(INetwork network, uint neuron1id, uint neuron2id, NeuronGeneList newNeurons) {
        network.ClearSignals();
        //network.SetInputSignal(0, 1);
        network.SetInputSignal(0, newNeurons[(int)neuron1id].XValue);
        network.SetInputSignal(1, newNeurons[(int)neuron1id].YValue);
        network.SetInputSignal(2, newNeurons[(int)neuron2id].XValue);
        network.SetInputSignal(3, newNeurons[(int)neuron2id].YValue);
        network.MultipleSteps(10);
        return network.GetOutputSignal(1);
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
            case 10:
                point = new Point(-0.9f, 0.2f);
                break;
            case 11:
                point = new Point(-0.6f, 0.2f);
                break;
            case 12:
                point = new Point(-0.3f, 0.2f);
                break;
            case 13:
                point = new Point(0.3f, 0.2f);
                break;
            case 14:
                point = new Point(0.6f, 0.2f);
                break;
            case 15:
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
