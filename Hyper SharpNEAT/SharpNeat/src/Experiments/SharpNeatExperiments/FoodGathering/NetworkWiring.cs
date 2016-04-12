using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpNeatLib.NeuralNetwork;

namespace SharpNeatLib.Experiments
{
    //DAVID make sure it works for input-output-hidden
    public partial class NetworkWiring :Form//AbstractExperimentView
    {
        float size;
        float[] neurons;
        public uint resolution = FoodGatherParams.resolution;
        float spacing;
        int activeNueronIdx = -1;
        int centering = 0;
        int connectiontypes = 0; //0=all, 1=incoming, 2=outgoing
        FloatFastConnection[] connections;
        public NetworkWiring()
        {
            if (FoodGatherParams.circle)
                neurons = new float[resolution * 2];
            else
                neurons = new float[resolution * 2];
            connections = new FloatFastConnection[0];
            InitializeComponent();
            sizeitUp();
        }

        private void NetworkWiring_Paint(object sender, PaintEventArgs e)
        {
            if(FoodGatherParams.circle)
                drawCircles(e.Graphics);
            else
                drawTopDown(e.Graphics);
            
        }

        private void NetworkWiring_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PointF pt;
                int counter = 0;
                if (FoodGatherParams.circle)
                {
                    for(int j=0;j<resolution*3;j++)
                    {
                            pt=getCircleCoordinates(j);
                            if (Math.Abs(e.X - pt.X) <= size && Math.Abs(e.Y - pt.Y) <= size)
                            {
                                activeNueronIdx = j;
                                this.Refresh();
                                return;
                            }
                            counter++;
                        }
                    
                }
                else
                {
                    
                    for(int j=0;j<resolution*resolution;j++)
                    {
                            pt=getTopDownCoordinates(j);
                            if (Math.Abs(e.X - pt.X) <= size && Math.Abs(e.Y - pt.Y) <= size)
                            {
                                activeNueronIdx = j;
                                this.Refresh();
                                return;
                            }
                            
                     }
                }
            }
            else if (e.Button == MouseButtons.Right)
                activeNueronIdx = -1;
            else if (e.Button == MouseButtons.Middle)
            {
                PointF pt;
                if (FoodGatherParams.circle)
                {
                    for (int j = 0; j < resolution * 3; j++)
                    {
                        pt = getCircleCoordinates(j);
                        if (Math.Abs(e.X - pt.X) <= size && Math.Abs(e.Y - pt.Y) <= size)
                        {
                            MessageBox.Show(neurons[j].ToString());
                        }
                    }

                }
                else
                {

                    for (int j = 0; j < resolution * resolution; j++)
                    {
                        pt = getTopDownCoordinates(j);
                        if (Math.Abs(e.X - pt.X) <= size && Math.Abs(e.Y - pt.Y) <= size)
                        {
                            MessageBox.Show(neurons[j].ToString());
                        }

                    }
                }
            }

            this.Refresh();
        }

        private void NetworkWiring_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case ('+'): FoodGathererPopulationEvaluator.requestResolutionUp = true;
                            resolution *= 2;
                            sizeitUp();
                            break;
                case ('-'): FoodGathererPopulationEvaluator.requestResolutionDown=true;
                            resolution /= 2;
                            sizeitUp();
                            break;
                case ('i'): connectiontypes = 1;
                            break;
                case ('o'): connectiontypes = 2;
                            break;
                case ('p'): connectiontypes = 0;
                            break;
            }
            Refresh();
        }

        private void sizeitUp()
        {
            if (FoodGatherParams.circle)
            {
                spacing = 450.0F;
                this.Height = (int)((resolution / 2) * (spacing + size));
                this.Width = (int)((resolution / 2) * (spacing + size));
            }
            else
            {
                spacing = 800.0F / (resolution);
                this.Height = (int)((resolution+1 ) * (spacing + size));
                this.Width = (int)((resolution+1 ) * (spacing + size));
            }
            size = 20.0f / (resolution/2);
            
        }

        private void drawTopDown(Graphics g)
        {
            //DAVID
            Brush b;
            for (int j = 0; j < resolution * 2; j++)
            {
                if (neurons[j] > 0)
                    b = Brushes.Blue;
                else if (neurons[j] < 0)
                    b = Brushes.Red;
                //else
                //   continue;
                g.FillRectangle(Brushes.Black, new RectangleF(getTopDownCoordinates(j), new SizeF((size), (size))));
                //g.FillRectangle(Brushes.Blue, new RectangleF(getTopDownCoordinates(j), new SizeF(5*(size * neurons[j]), 5*(size * neurons[j]))));
            }
            
            Pen p = new Pen(Color.Black);
            p.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            foreach (FloatFastConnection ffc in connections)
            {
                if (ffc.weight >= 0)
                    p.Color = Color.Black;
                else
                    p.Color = Color.Gray;
                p.Width = 2 * Math.Abs(ffc.weight) + .001f;
                if (activeNueronIdx == -1)
                {
                    g.DrawLine(p, getTopDownCoordinates(ffc.sourceNeuronIdx), getTopDownCoordinates(ffc.targetNeuronIdx));
                }
                else
                {
                    if (connectiontypes == 0)
                    {
                        if (ffc.sourceNeuronIdx == activeNueronIdx || ffc.targetNeuronIdx == activeNueronIdx)
                            g.DrawLine(p, getTopDownCoordinates(ffc.sourceNeuronIdx), getTopDownCoordinates(ffc.targetNeuronIdx));
                    }
                    else if (connectiontypes == 1)
                    {
                        if (ffc.targetNeuronIdx == activeNueronIdx)
                            g.DrawLine(p, getTopDownCoordinates(ffc.sourceNeuronIdx), getTopDownCoordinates(ffc.targetNeuronIdx));
                    }
                    else
                    {
                        if (ffc.sourceNeuronIdx == activeNueronIdx)
                            g.DrawLine(p, getTopDownCoordinates(ffc.sourceNeuronIdx), getTopDownCoordinates(ffc.targetNeuronIdx));
                    }
                    
                }
            }
        }

        private void drawCircles(Graphics g)
        {
            centering = 500;
            Brush b;
            for(int c=0;c<2;c++)
                g.DrawEllipse(Pens.Red,Utilities.Translate(new RectangleF(centering,centering,2*spacing*(c+1)/2.0f,2*spacing*(c+1)/2.0f)));
            for (int j = 0; j < resolution * 2; j++)
            {
                if (neurons[j] > 0)
                    b = Brushes.Black;
                else if (neurons[j] < 0)
                    b = Brushes.Red;
                //else
                //    continue;
                g.FillRectangle(Brushes.Black, new RectangleF(getCircleCoordinates(j), new SizeF((size), (size))));
            }
        
            Pen p = new Pen(Color.Black);
            p.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            foreach (FloatFastConnection ffc in connections)
            {
                if (ffc.weight >= 0)
                    p.Color = Color.Black;
                else
                    p.Color = Color.Gray;
                p.Width = 2 * Math.Abs(ffc.weight) + .001f;
                if (activeNueronIdx == -1)
                {
                    g.DrawLine(p, getCircleCoordinates(ffc.sourceNeuronIdx), getCircleCoordinates(ffc.targetNeuronIdx));
                    //g.DrawLine(p, new Point((int)(ffc.sourceNeuronIdx % resolution) * (int)spacing, (int)(ffc.sourceNeuronIdx / resolution) * (int)spacing), new Point((int)(ffc.targetNeuronIdx % resolution) * (int)spacing, (int)(ffc.targetNeuronIdx / resolution) * (int)spacing));
                }
                else
                {
                    if (connectiontypes == 0)
                    {
                        if (ffc.sourceNeuronIdx == activeNueronIdx || ffc.targetNeuronIdx == activeNueronIdx)
                            g.DrawLine(p, getCircleCoordinates(ffc.sourceNeuronIdx), getCircleCoordinates(ffc.targetNeuronIdx));
                    }
                    else if (connectiontypes == 1)
                    {
                        if (ffc.targetNeuronIdx == activeNueronIdx)
                            g.DrawLine(p, getCircleCoordinates(ffc.sourceNeuronIdx), getCircleCoordinates(ffc.targetNeuronIdx));
                            
                    }
                    else
                    {
                        if (ffc.sourceNeuronIdx == activeNueronIdx)
                            g.DrawLine(p, getCircleCoordinates(ffc.sourceNeuronIdx), getCircleCoordinates(ffc.targetNeuronIdx));
                    }
                }
            }
        }
        
        private PointF getCircleCoordinates(int neuronidx)
        {
            float angle=(float)(-3*Math.PI/4), angledelta=(float)(2*Math.PI/resolution);
            for(int j=0;j<neuronidx;j++,angle+=angledelta)
            {
              //  if(angle>=Math.PI)
              //      angle-=(float)(2*Math.PI);
            }
            if (neuronidx < resolution)
                return new PointF(centering + spacing * ((1.0f / 2.0f)) * (float)Math.Cos(angle + angledelta / 2), centering + spacing * (1.0f / 2.0f) * (float)Math.Sin(angle + angledelta / 2));
            else if(neuronidx< resolution*2)
                return new PointF(centering + spacing * ((3.0f / 3.0f)) * (float)Math.Cos(angle + angledelta / 2), centering + spacing * (3.0f / 3.0f) * (float)Math.Sin(angle + angledelta / 2));
            else
                return new PointF(centering + spacing * (2.0f / 3.0f) * (float)Math.Cos(angle + angledelta / 2), centering + spacing * (2.0f / 3.0f) * (float)Math.Sin(angle + angledelta / 2));
        }

        private PointF getTopDownCoordinates(int neuronidx)
        {
            if (neuronidx < resolution)
                return new PointF(spacing * (neuronidx+1), spacing);
            else if (neuronidx < resolution * 2)
                return new PointF(spacing * (neuronidx+1 - resolution), spacing * (resolution-1));
            else
                return new PointF(spacing * ((neuronidx % resolution)), spacing * ((neuronidx / resolution)-1));
        }

        //public override void RefreshView(SharpNeatLib.NeuralNetwork.INetwork network)
        public void RefreshView(SharpNeatLib.NeuralNetwork.INetwork network)
        {
            if (network.OutputNeuronCount == 1)
            {
                if (FoodGatherParams.circle)
                    network = FoodGathererNetworkEvaluator.substrate.generateNetwork(network);
                else
                    network = FoodGathererNetworkEvaluator.substrate.generateNetwork(network);
            }
            connections=((FloatFastConcurrentNetwork)network).connectionArray;
            neurons = ((FloatFastConcurrentNetwork)network).neuronSignalArray;
            resolution = FoodGatherParams.resolution;
            this.Refresh();
        }
    }
}