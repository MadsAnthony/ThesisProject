using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;
using System.Xml;
using SharpNeatLib.NeatGenome.Xml;
using System.IO;

namespace SharpNeatLib.Experiments
{
    public partial class Form1 :AbstractExperimentView
    {
        INetwork currentBest;
        NetworkWiring nw;
        public bool viewing = false;
        public bool abort = false;
        public Board board;
        public Form1()
        {
            InitializeComponent();
            board = new Board(0, 500);
            this.Size = Utilities.SizeFtoSize(board.Size);
            //board.AddRobot(new PointF(250, 250));
        }
        public Form1(Robot r)
        {
            InitializeComponent();
            board = new Board(0, 500);
            board.AddRobot(r);
            this.Size = Utilities.SizeFtoSize(board.Size);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Type type=new Robot().GetType();
            Graphics g=e.Graphics;
            e.Graphics.FillRectangle(Brushes.White, new RectangleF(board.Location, board.Size));
            Brush myBrush = new SolidBrush(System.Drawing.Color.Red);
            foreach(Robot r in board.Robots)
            {
                float arc = 360F / r.Sensors;
                int i = 0;
                for (float f = 225; Math.Ceiling(f) < 360 + 225; f += arc, i++)
                {
                    //if (r.SensorReadings[i] > 0)
                    {
                        ((SolidBrush)myBrush).Color = Color.FromArgb(255, (byte)(255 * (1 - r.SensorReadings[i])), (byte)(255 * (1 - r.SensorReadings[i])));
                        g.FillPie(myBrush, r.Location.X - r.SensorRadius, r.Location.Y - r.SensorRadius, r.SensorRadius * 2, r.SensorRadius * 2, f, arc);
                        g.DrawPie(Pens.Black, r.Location.X - r.SensorRadius, r.Location.Y - r.SensorRadius, r.SensorRadius * 2, r.SensorRadius * 2, f, arc);
                    }
                    //else
                    //    g.DrawPie(Pens.Black, r.Location.X - r.SensorRadius, r.Location.Y - r.SensorRadius, r.SensorRadius * 2, r.SensorRadius * 2, f, arc);

                }
                g.FillEllipse(Brushes.Blue, Utilities.Translate(new RectangleF(r.Location, board.RobotSize)));
            }
            for(int j=0;j<board.Agents.Count;j++)
            {
                RoboAgent a = board.Agents[j];
                g.FillEllipse(Brushes.Black, Utilities.Translate(new RectangleF(a.Location, board.FoodSize)));
            }
            
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
           
        }

        private void Form1_Click(object sender, EventArgs e)
        {
                MouseEventArgs mouse = (MouseEventArgs)e;
                switch (mouse.Button)
                {
                    case (MouseButtons.Left):
                        board.AddFood(mouse.Location);
                        //Invalidate(Utilites.Translate(new Rectangle(mouse.Location, board.FoodSize)));
                        Invalidate();
                        break;
                    case (MouseButtons.Right):
                        board.Robots[0].Move(mouse.Location);
                        Invalidate();
                        break;
                }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch(e.KeyChar)
            {
                case('w'):
                    board.Robots[0].Move(new SizeF(0,-1));
                    break;
                case('a'):
                    board.Robots[0].Move(new SizeF(-1,0));
                    break;
                case('s'):
                    board.Robots[0].Move(new SizeF(0,1));
                    break;
                case('d'):
                    board.Robots[0].Move(new SizeF(1,0));
                    break; 
                case('+'):
                    FoodGatherParams.resolution*=2;
                    FoodGatherParams.fillFood();
                    if(FoodGatherParams.circle)
                        board.Robots[0] = new Robot(board.Robots[0].Location, (int)FoodGatherParams.resolution, FoodGathererNetworkEvaluator.substrate.generateNetwork(currentBest));
                    else
                        board.Robots[0] = new Robot(board.Robots[0].Location, (int)FoodGatherParams.resolution, FoodGathererNetworkEvaluator.substrate.generateNetwork(currentBest));
                    break;
                case('-'):
                    if(board.Robots[0].Sensors - 1>=1)
                        board.Robots[0] = new Robot(board.Robots[0].Location, board.Robots[0].Sensors - 1);
                    break;
                case('t'):
                    board.turn();
                    updateWiring();
                    break;
                case('g'):
                    MessageBox.Show(board.game().ToString());
                    break;
                case('x'):
                    board.Robots[0].Move(board.Agents[0].Location);
                    break;
                case('p'):
                    Demo();
                    break;
            }
            Invalidate(Utilities.Translate(new Rectangle(new Point((int)board.Robots[0].Location.X + 2, (int)board.Robots[0].Location.Y + 2), new Size(3 * ((int)board.Robots[0].SensorRadius + 2), 3 * (int)board.Robots[0].SensorRadius + 2))));
        }

        private void seeNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            nw=new NetworkWiring();
            if(!nw.Visible)
                nw.Show();
            updateWiring();
        }

        private void Demo()
        {
            FoodGatherParams.fillLookups();
            FoodGatherParams.fillFood();
            for (int j = 0; j < FoodGatherParams.foodLocations.Length; j++)
            {
                Robot r;
                board = new Board(0, 500);
                if(FoodGatherParams.circle)
                    r = new Robot(new PointF(250, 250), (int)FoodGatherParams.resolution, FoodGathererNetworkEvaluator.substrate.generateNetwork(currentBest));
                else
                    r = new Robot(new PointF(250, 250),(int)FoodGatherParams.resolution, FoodGathererNetworkEvaluator.substrate.generateNetwork(currentBest));
                board.AddRobot(r);
                board.AddFood(new Food(FoodGatherParams.foodLocations[j]));
                board.gameView(this);
            }
        }

        private void updateWiring()
        {
            if (board.Robots.Count > 0) 
                    nw.RefreshView(board.Robots[0].brain);
            
        }

        public override void RefreshView(SharpNeatLib.NeuralNetwork.INetwork network)
        {
            currentBest = network;
            if(nw!=null)
            nw.RefreshView(network);
            viewing = true;
        }

        private void loadNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename;
            OpenFileDialog oDialog = new OpenFileDialog();
			oDialog.AddExtension = true;
			oDialog.DefaultExt = "xml";
            oDialog.Title = "Load Seed Genome";
			oDialog.RestoreDirectory = true;

			// Show Open Dialog and Respond to OK
			if(oDialog.ShowDialog() == DialogResult.OK)
				filename=oDialog.FileName;
			else
				return;


            NeatGenome.NeatGenome seedGenome = null;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                seedGenome = XmlNeatGenomeReaderStatic.Read(doc);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem loading genome. \n" + ex.Message);
                return;
            }
            currentBest = GenomeDecoder.DecodeToFloatFastConcurrentNetwork(seedGenome, null);
        }

        private void playWithNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FoodGatherParams.fillLookups();
            if (currentBest != null)
            {
                board = new Board(0, 500);
                Robot r;
                if (FoodGatherParams.circle)
                    r = new Robot(new PointF(250, 250), (int)FoodGatherParams.resolution, FoodGathererNetworkEvaluator.substrate.generateNetwork(currentBest));
                else
                    r = new Robot(new PointF(250, 250), (int)FoodGatherParams.resolution, FoodGathererNetworkEvaluator.substrate.generateNetwork(currentBest));
                board.AddRobot(r);
                this.Refresh();
            }
        }

        private void adHocAnalysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename;
            StreamWriter sw;
            
            NeatGenome.NeatGenome seedGenome = null;

            Stats currentStats=new Stats();

            for(FoodGatherParams.resolution=8;FoodGatherParams.resolution<=128;FoodGatherParams.resolution*=2)
            {
                FoodGatherParams.fillFood();
                FoodGatherParams.fillLookups();
                sw = new StreamWriter("logfile" + FoodGatherParams.resolution.ToString() + ".txt");
                for(int run=1;run<=20;run++)
                {
                    
                    for (int generation = 1; generation <=500; generation++)
                    {
                        filename = "run"+run.ToString()+@"\genome_" + generation.ToString()+".xml";

                        try
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load("run"+run.ToString()+"\\genome_"+generation.ToString()+".xml");
                            seedGenome = XmlNeatGenomeReaderStatic.Read(doc);
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show(generation.ToString());
                            currentStats.generation = generation;
                            currentStats.run = run;
                            writeStats(sw, currentStats);
                            continue;
                        //do some output
                        }
                        
                        
                        currentStats=FoodGathererNetworkEvaluator.postHocAnalyzer(seedGenome);
                        currentStats.CPPNconnections = seedGenome.ConnectionGeneList.Count;
                        currentStats.CPPNneurons = seedGenome.NeuronGeneList.Count;
                            
                        
                        currentStats.generation = generation;
                        currentStats.run = run;
                        writeStats(sw, currentStats);
                        //break;
                    }
                  // sw.Flush();
                }
                sw.Close();
            }
        }
        private void writeStats(StreamWriter sw, Stats info)
        {
            sw.WriteLine(info.run.ToString() + " " + info.generation.ToString() + " " + FoodGatherParams.resolution.ToString() + " " + info.numFoods.ToString() + " " +
                info.avgSpeed.ToString() + " " + info.CPPNneurons.ToString() + " " + info.CPPNconnections.ToString() + " " + info.NNconnections.ToString());
        }

        private void GNGViewer_Load(object sender, EventArgs e)
        {

        }
    }
}