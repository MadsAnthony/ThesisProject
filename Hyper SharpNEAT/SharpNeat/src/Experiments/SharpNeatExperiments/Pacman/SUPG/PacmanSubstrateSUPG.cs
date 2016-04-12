//#define OUTPUT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeatLib.CPPNs;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;


class PacmanSubstrateSUPG : Substrate
    {
    private const float shiftScale = 0.2f;

    public PacmanSubstrateSUPG(uint inputs, uint outputs, uint hidden, IActivationFunction function)
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
            /*if (neuron.NeuronType == NeuronType.Hidden)
            {*/
                // switch to grid substrate configuration
            Point point = GetCustomPos(neuron.InnovationId);
            neuron.XValue = point.X;
            neuron.YValue = point.Y;
            if (neuron.NeuronType != NeuronType.Input) {
                neuron.ActivationFunction = new SteepenedSigmoid();
            }
            /*neuron.TimeConstant = 1;
            neuron.NeuronBias = 0;*/
                /*neuron.XValue = getXPos3(neuron.InnovationId);
                neuron.YValue = getYPos3(neuron.InnovationId);*/
            //}
        }

        ConnectionGeneList connections = new ConnectionGeneList((int)((inputCount * hiddenCount) + (hiddenCount * outputCount)));
        float[] coordinates = new float[5];
        float output;
        uint connectionCounter = 0;
        int iterations = 2 * (network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount)) + 1;

        // connect hidden layer to outputs
        /*for (uint source = 0; source < hiddenCount; source++)
        {
            coordinates[0] = getXPos(source, false);
            coordinates[1] = getYPos(source, false);

            for (uint target = 0; target < outputCount; target++)
            {
                // only connect hidden nodes to their single nearest output
                if (source == target)
                {
                    coordinates[2] = getXPos(target, true);
                    coordinates[3] = getYPos(target, true);

                    // GWM - fixing weight to 1 for SUPG producing motor outputs
                    connections.Add(new ConnectionGene(connectionCounter++, source + inputCount + outputCount, target + inputCount, 1));
                }
            }
        }*/

        // Connections from input to hidden
        for (uint source = 0; source < inputCount; source++)
        {
            //connections.Add(new ConnectionGene(connectionCounter++, source, 6, getActivation(network,source,6,newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, source, 9, getActivation(network, source, 9, newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, source, 10, getActivation(network, source, 10, newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, source, 11, getActivation(network, source, 11, newNeurons)));
        }
        // Connection from input to output
        connections.Add(new ConnectionGene(connectionCounter++, 0, 6, getActivation(network, 0, 6, newNeurons)));
        connections.Add(new ConnectionGene(connectionCounter++, 3, 6, getActivation(network, 3, 6, newNeurons)));
        // Connections from hidden to hidden
        for (uint source = 0; source < hiddenCount-2; source++)
        {
            uint tmpSource = source + inputCount + outputCount;
            connections.Add(new ConnectionGene(connectionCounter++, tmpSource, 12, getActivation(network, tmpSource, 12, newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, tmpSource, 13, getActivation(network, tmpSource, 13, newNeurons)));
        }
        // Connections from hidden to output
        for (uint source = 0; source < 2; source++)
        {
            uint tmpSource = source + inputCount + outputCount+3;
            connections.Add(new ConnectionGene(connectionCounter++, tmpSource, 4, getActivation(network, tmpSource, 4, newNeurons)));
            connections.Add(new ConnectionGene(connectionCounter++, tmpSource, 5, getActivation(network, tmpSource, 5, newNeurons)));
        }

        return new SharpNeatLib.NeatGenome.NeatGenome(0, newNeurons, connections, (int)inputCount, (int)outputCount);
    }

    double getActivation(INetwork network, uint neuron1id, uint neuron2id, NeuronGeneList newNeurons) {
        network.ClearSignals();
        network.SetInputSignal(0, 1);
        network.SetInputSignal(1, newNeurons[(int)neuron1id].XValue);
        network.SetInputSignal(2, newNeurons[(int)neuron1id].YValue);
        network.SetInputSignal(3, newNeurons[(int)neuron2id].XValue);
        network.SetInputSignal(4, newNeurons[(int)neuron2id].YValue);
        network.MultipleSteps(10);
        return network.GetOutputSignal(0);
    }

    private float getXPos(uint index, bool isOutput)
    {
        float pos = 0;
        float shift = shiftScale;
        if (isOutput)
            shift *= 2;

        switch (index)
        {
            case 0:
            case 6:
                pos = -1;
                break;
            case 1:
            case 2:
            case 7:
            case 8:
                pos = -1 + shift;
                break;
            case 3:
            case 4:
            case 9:
            case 10:
                pos = 1 - shift;
                break;
            case 5:
            case 11:
                pos = 1;
                break;
        }
        return pos;
    }

    private float getYPos(uint index, bool isOutput)
    {
        float pos = 0;
        float shift = shiftScale;
        if (isOutput)
            shift *= 2;

        switch (index)
        {
            case 2:
            case 3:
                pos = 1;
                break;
            case 6:
            case 7:
            case 10:
            case 11:
                pos = -1 + shift;
                break;
            case 0:
            case 1:
            case 4:
            case 5:
                pos = 1 - shift;
                break;
            case 8:
            case 9:
                pos = -1;
                break;
        }
        return pos;
    }

    private float getXPos2(uint index)
    {
        float pos = 0;
        switch (index)
        {
            case 0:
            case 1:
            case 2:
                pos = -1;
                break;
            case 3:
            case 4:
            case 5:
                pos = -.33f;
                break;
            case 6:
            case 7:
            case 8:
                pos = .33f;
                break;
            case 9:
            case 10:
            case 11:
                pos = 1;
                break;

        }
        return pos;
    }

    private float getYPos2(uint index)
    {
        float pos = 0;
        switch (index)
        {
            case 0:
            case 5:
            case 6:
            case 11:
                pos = -1;
                break;
            case 2:
            case 3:
            case 8:
            case 9:
                pos = 0;
                break;
            case 1:
            case 4:
            case 7:
            case 10:
                pos = 1;
                break;

        }
        return pos;
    }

    // getXPos3 is made by Mads
    private float getXPos3(uint index)
    {
        float pos = 0;
        return (index % 2)/2f;
        switch (index)
        {
            case 0:
                pos = -1;
                break;
            case 1:
                pos = -0.8f;
                break;
            case 2:
                pos = -0.6f;
                break;
            case 3:
                pos = -0.4f;
                break;
            case 4:
                pos = -0.2f;
                break;
            case 5:
                pos = 0;
                break;
            case 6:
                pos = -1;
                break;
            case 7:
                pos = -1;
                break;
            case 8:
                pos = .33f;
                break;
            case 9:
                pos = -1;
                break;
            case 10:
                pos = -1;
                break;
            case 11:
                pos = 1;
                break;

        }
        return pos;
    }

    // getYPos3 is made by Mads
    private float getYPos3(uint index)
    {
        float pos = 0;
        return index / 2;
        switch (index)
        {
            case 0:
                pos = -1;
                break;
            case 1:
                pos = -1;
                break;
            case 2:
                pos = -1;
                break;
            case 3:
                pos = -1;
                break;
            case 4:
                pos = -1;
                break;
            case 5:
                pos = -.33f;
                break;
            case 6:
                pos = -1;
                break;
            case 7:
                pos = -1;
                break;
            case 8:
                pos = .33f;
                break;
            case 9:
                pos = -1;
                break;
            case 10:
                pos = -1;
                break;
            case 11:
                pos = 1;
                break;

        }
        return pos;
    }

    Point GetCustomPos(uint index) {
        Point point = new Point(0, 0);
        switch (index)
        {
            // input nodes
            case 0:
                point = new Point(-0.4f, 0.2f);
                break;
            case 1:
                point = new Point(-0.3f, 0.2f);
                break;
            case 2:
                point = new Point(-0.2f, 0.2f);
                break;
            case 3:
                point = new Point(-0.1f, 0.2f);
                break;
            // hidden nodes
            case 9:
                point = new Point(-0.3f, 0);
                break;
            case 10:
                point = new Point(-0.2f, 0);
                break;
            case 11:
                point = new Point(-0.4f, 0f);
                break;
            case 12:
                point = new Point(-0.8f, -0.1f);
                break;
            case 13:
                point = new Point(0.05f, -0.1f);
                break;
            // output nodes
            case 4:
                point = new Point(-0.4f, -0.2f);
                break;
            case 5:
                point = new Point(-0.1f, -0.2f);
                break;
            case 6:
                point = new Point(0.3f, 0.05f);
                break;
            case 7:
                point = new Point(0.2f, -0.1f);
                break;
            case 8:
                point = new Point(0.4f, -0.1f);
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
