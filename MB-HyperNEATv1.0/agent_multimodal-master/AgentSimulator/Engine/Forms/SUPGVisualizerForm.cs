using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using SharpNeatLib.NeuralNetwork;
using System.Drawing;
using SharpNeatLib.NeatGenome;

namespace Engine.Forms
{
    class SUPGVisualizerForm : Form
    {
        private Robot selectedRobot;
        SolidBrush brush;
        Pen penConnection;
        ModularNetwork net;
        bool checkZ = false; // Schrum: added
        float dtx = 100;
        float dty = 100;
        float w;
        float startX;
        float startY;// = 1.1f * dty;
        Graphics g;
        int index;
        public GenomeVisualizerForm genomeVisualizerForm;

        // Schrum: Old constructor, without a brain counter, simply calls new one with extra argument
        public SUPGVisualizerForm(Robot _selectedRobot, ModularNetwork _net) : this(_selectedRobot, _net, -1, false) {
        }

        // Schrum: Just added the brainCounter to the old constructor
        public SUPGVisualizerForm(Robot _selectedRobot, ModularNetwork _net, int brainCounter, bool checkZ) 
        {
            this.checkZ = checkZ; // Schrum: added
            //Console.WriteLine("Draw:" + brainCounter);
            _net.UpdateNetworkEvent += networkUpdated;
            InitializeComponent();
            net = _net;
            selectedRobot = _selectedRobot;
            // Schrum: Modified this to identify the brain being accessed
            this.Text = "Network Visualizer [z="+ selectedRobot.zstack+(brainCounter != -1 ? ",s="+brainCounter : "")+"]";
            SetBounds(1, 1, 320, 320);
            brush = new SolidBrush(Color.Red);
            penConnection = new Pen(Color.Black);
            startX = 1.1f * dtx;
            startY = 1.1f * dty;

            //set up double buffering
            this.SetStyle(
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.DoubleBuffer, true);
        }

        //This function gets called when the current simulated network sends an update event
        public void networkUpdated(ModularNetwork _net)
        {
            //Console.WriteLine("networkUpdated");
            //net = _net;
            Refresh();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // NetworkVisualizerForm
            // 
            this.ClientSize = new System.Drawing.Size(524, 435);
            this.Name = "NetworkVisualizerForm";
            this.Text = "?";
            this.Load += new System.EventHandler(this.NetworkVisualizerForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.NetworkVisualizerForm_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NetworkVisualizerForm_MouseClick);
            this.SizeChanged += new System.EventHandler(this.NetworkVisualizerForm_SizeChanged);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.NetworkVisualizerForm_MouseMove);
            this.ResumeLayout(false);


            InitPoints();
        }

        Point[] masterPoints;
        List<Point[]> pointList;

        void InitPoints() {
            int offSetX = 10;
            masterPoints = new Point[100];
            for (int i = 0; i < masterPoints.Length; i++)
            {
                masterPoints[i].X = i * 2 + offSetX;
            }

            pointList = new List<Point[]>();
            for (int j = 0; j < 10; j++)
            {
                var points1 = new Point[100];
                for (int i = 0; i < points1.Length; i++)
                {
                    points1[i].X = i * 2+offSetX;
                }
                pointList.Add(points1);
            }
        }

        float timer = 0;
        private void NetworkVisualizerForm_Paint(object sender, PaintEventArgs e)
        {
            //Console.WriteLine("NetworkVisualizerForm_Paint");
            if (net != null && net.genome.ConnectionGeneList != null)
            {
                g = e.Graphics;
                index = 0;

                penConnection.Color = Color.Black;
                penConnection.Width = Math.Abs(net.connections[index].weight);

                timer += 0.1f;
                //DrawFreq(Math.Sin(timer), points1, 50, Color.Black);
                DrawFreqs();
            }
        }

        private int supgTimer;
        void DrawFreqs() {
            if (net.supgFreqs == null) return;
            Pen pen = new System.Drawing.Pen(Color.Blue);
            if (supgTimer != net.supgTimer) {
                DrawFreq(net.masterFreq, masterPoints, 50);
            }
            g.DrawLines(pen, masterPoints);

            int i = 0;
            foreach (var supgFreq in net.supgFreqs) {
                //if (i >= 1) break;
                if (supgTimer != net.supgTimer) {
                    DrawFreq(supgFreq, pointList[i], 75 * i+100);
                }
                pen = new System.Drawing.Pen(Color.Black);
                Brush brush = new SolidBrush(Color.Bisque);
                g.FillRectangle(brush, new Rectangle(10, 75 * i + 100-25, 200, 50));
                g.DrawLines(pen, pointList[i]);
                i++;
            }
            supgTimer = net.supgTimer;
        }

        void DrawFreq(double freq, Point[] points, int yOffset)
        {            
            points[points.Length - 1].Y = (int)(-freq * 25 + yOffset);
            for (int i = 0; i < points.Length; i++)
            {
                if (i < points.Length - 1)
                {
                    points[i].Y = points[i + 1].Y;
                }
            }
        }

        private void NetworkVisualizerForm_SizeChanged(object sender, EventArgs e)
        {
            dtx = /*30;//*/this.Width / 2.5f;
            dty = /*30;//*/this.Height / 3.0f;
            startX = /*500+2f * dtx;//*/1.1f * dtx;
            startY = /*250+2f * dty;//*/1.1f * dty;
            Refresh();
        }

        private void NetworkVisualizerForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (genomeVisualizerForm != null)
            {
                MouseEventArgs m = new MouseEventArgs(MouseButtons.Left, 0, 
                    (int)((float)(e.X) / (startX+dtx) * 200.0f), 
                    (int)((float)(e.Y) / (startY+dty) * 200.0f), 0);
               
                genomeVisualizerForm.NetworkVisualizerForm_MouseMove(sender, m);
            }
        }

        private void NetworkVisualizerForm_Load(object sender, EventArgs e)
        {

        }

        private void NetworkVisualizerForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (genomeVisualizerForm != null)
            {
                MouseEventArgs m = new MouseEventArgs(MouseButtons.Left, 0,
                    (int)((float)(e.X) / (startX + dtx) * 200.0f),
                    (int)((float)(e.Y) / (startY + dty) * 200.0f), 0);
                genomeVisualizerForm.pictureBox1_MouseClick(sender, m);
            }
        }
    }
}
