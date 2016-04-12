﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Pacman.Simulator.Ghosts;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.NeuralNetwork;

namespace SharpNeatExperiments.Pacman
{
    public partial class MyForm1 : Form
    {
        private Image image;
        private Graphics g;
        private TimerEventHandler tickHandler;
        private uint fastTimer;

        int myTimer = 0;

        Point[] points1;
        Point[] points2;
        Point[] pointsMaster;

        public static double freq1;
        public static double freq2;
        public static double freqMaster;

        public static NeatGenome neatGenome;
        public static INetwork network;

        public MyForm1()
        {
            InitializeComponent();
            image = new Bitmap(Picture.Width, Picture.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(image);


            int myData = 0; // dummy data
            tickHandler = new TimerEventHandler(tick);
            fastTimer = timeSetEvent(50, 50, tickHandler, ref myData, 1);

            Application.ApplicationExit += new EventHandler(closeHandler);
            InitPoints();
        }

        double someSinus1 = 0;
        double someSinus2 = 0;
        private void tick(uint id, uint msg, ref int userCtx, int rsv1, int rsv2)
        {
            myTimer++;

            someSinus1 = Math.Sin(myTimer/5f);
            //freq1 = someSinus1;

            someSinus2 = Math.Sin((myTimer+50) / 5f);
            //freq2 = someSinus2;

            //freqMaster = 0.5f;
            Draw();
        }

        void InitPoints() {
            points1 = new Point[100];
            for (int i = 0; i < points1.Length; i++) {
                points1[i].X = i * 5;
            }
            points2 = new Point[100];
            for (int i = 0; i < points2.Length; i++) {
                points2[i].X = i * 5;
            }
            pointsMaster = new Point[100];
            for (int i = 0; i < pointsMaster.Length; i++) {
                pointsMaster[i].X = i * 5;
            }
        }

        private void Draw() {
            g.Clear(Color.Black);
            DrawFreq(freq1, points1, 50, System.Drawing.Color.YellowGreen);
            DrawFreq(freq2, points2, 50, System.Drawing.Color.Tomato);
            DrawFreq(freqMaster, pointsMaster, 50, System.Drawing.Color.WhiteSmoke);
            DrawNet();
            Picture.Image = image;
        }

        void DrawFreq(double freq, Point[] points, int yOffset, System.Drawing.Color color) {
            Pen pen = new System.Drawing.Pen(color);

            points[points.Length - 1].Y = (int)(freq * 50 + yOffset);
            for (int i = 0; i < points.Length; i++)
            {
                if (i < points.Length - 1)
                {
                    points[i].Y = points[i + 1].Y;
                }
            }
            g.DrawLines(pen, points);
        }

        void DrawNet() {
            if (neatGenome == null) return;
            int networkXOffset = 200;
            int networkYOffset = 350;
            int spread = 200;
            int radius = 10;
            var neurons = neatGenome.NeuronGeneList;
            var connections = neatGenome.ConnectionGeneList;
            foreach (var neuron in neurons) {
                System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(networkXOffset + (int)(neuron.XValue * spread), networkYOffset + (int)(neuron.YValue * spread), radius, radius);
                var color = System.Drawing.Color.Aquamarine;
                if (neuron.NeuronType == SharpNeatLib.NeuralNetwork.NeuronType.Output) {
                    color = System.Drawing.Color.Red;
                }
                if (neuron.NeuronType == SharpNeatLib.NeuralNetwork.NeuronType.Hidden)
                {
                    color = System.Drawing.Color.Yellow;
                }
                if (neuron.NeuronType == SharpNeatLib.NeuralNetwork.NeuronType.Input)
                {
                    color = System.Drawing.Color.Green;
                }
                if (neuron.InnovationId == 7 || neuron.InnovationId == 4) {
                    if (IsWithinThreshold(network.GetOutputSignal(2),network.GetOutputSignal(3), 0.2f)) {
                        color = System.Drawing.Color.White;
                    }
                }
                if (neuron.InnovationId == 8 || neuron.InnovationId == 5) {
                    if (IsWithinThreshold(network.GetOutputSignal(2),network.GetOutputSignal(4), 0.2f)) {
                        color = System.Drawing.Color.White;
                    }
                }
                Pen pen = new System.Drawing.Pen(color);
                g.DrawEllipse(pen, rectangle);
            }
            foreach (var connection in connections) {
                var src = GetNeuronWithNeuronId(neurons, connection.SourceNeuronId);//neurons[(int)connection.SourceNeuronId];
                var dst = GetNeuronWithNeuronId(neurons, connection.TargetNeuronId);//neurons[(int)connection.TargetNeuronId];
                Point srcPoint = new Point((int)(networkXOffset + radius / 2f + src.XValue * spread), (int)(networkYOffset + radius / 2f + src.YValue * spread));
                Point dstPoint = new Point((int)(networkXOffset + radius / 2f + dst.XValue * spread), (int)(networkYOffset + radius / 2f + dst.YValue * spread));
                g.DrawLine(System.Drawing.Pens.Aquamarine, srcPoint, dstPoint);
            }
        }

        bool IsWithinThreshold(float input1, float input2, float threshold) {
            return (input2 < input1 + threshold && input2 > input1 - threshold);
        }

        NeuronGene GetNeuronWithNeuronId(NeuronGeneList neurons, uint neuronId) {
            foreach (var neuron in neurons) {
                if (neuron.InnovationId == neuronId) {
                    return neuron;
                }
            }
            return null;
        }

        private void closeHandler(object sender, EventArgs args)
        {
            timeKillEvent(fastTimer);
            this.Close();
        }

        [DllImport("WinMM.dll", SetLastError = true)]
        private static extern uint timeSetEvent(int msDelay, int msResolution,
            TimerEventHandler handler, ref int userCtx, int eventType);

        [DllImport("WinMM.dll", SetLastError = true)]
        static extern uint timeKillEvent(uint timerEventId);

        public delegate void TimerEventHandler(uint id, uint msg, ref int userCtx,
                int rsv1, int rsv2);
    }
}