using System;
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

public partial class MyForm : Form
{
    int circleDiameter = 100;

    private Image image;
    private Graphics g;
    private TimerEventHandler tickHandler;
    private uint fastTimer;

    public MyForm()
    {
        int width = 20;
        int height = 500;
        image = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        g = Graphics.FromImage(image);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

        //int myData = 0; // dummy data
        //Application.Idle += HandleApplicationIdle;
        //tickHandler = new TimerEventHandler(tick);
        //fastTimer = timeSetEvent(20, 20, tickHandler, ref myData, 1);

        //g.Clear(Color.Black);
        //InitializeComponent();
    }

    private void tick(uint id, uint msg, ref int userCtx, int rsv1, int rsv2) {
        draw();
    }

    void draw() {
        g.Clear(Color.Black);
    }

    void HandleApplicationIdle(object sender, EventArgs e)
    {
        //TODO: Implement me.
    }

    protected override void OnPaint(PaintEventArgs e) {
        /*base.OnPaint(e);
        Graphics g;

        g = e.Graphics;

        Pen myPen = new Pen(Color.Red);
        myPen.Width = 30;
        g.DrawLine(myPen, 30, 30, 45, 65);

        g.DrawLine(myPen, 1, 1, 45, 65);*/
    }

    private void Form1_Paint(object sender, PaintEventArgs e)
    {

        Point CenterPoint = new Point()
        {
            X = this.ClientRectangle.Width / 2,
            Y = this.ClientRectangle.Height / 2
        };
        Point topLeft = new Point()
        {
            X = (this.ClientRectangle.Width - circleDiameter) / 2,
            Y = (this.ClientRectangle.Height - circleDiameter) / 2
        };
        Point topRight = new Point()
        {
            X = (this.ClientRectangle.Width + circleDiameter) / 2,
            Y = (this.ClientRectangle.Height - circleDiameter) / 2
        };
        Point bottomLeft = new Point()
        {
            X = (this.ClientRectangle.Width - circleDiameter) / 2,
            Y = (this.ClientRectangle.Height + circleDiameter) / 2
        };
        Point bottomRight = new Point()
        {
            X = (this.ClientRectangle.Width + circleDiameter) / 2,
            Y = (this.ClientRectangle.Height + circleDiameter) / 2
        };

        e.Graphics.DrawRectangle(Pens.Red, topLeft.X, topLeft.Y, circleDiameter, circleDiameter);
        e.Graphics.DrawLine(Pens.Red, CenterPoint, topLeft);
        e.Graphics.DrawLine(Pens.Red, CenterPoint, topRight);
        e.Graphics.DrawLine(Pens.Red, CenterPoint, bottomLeft);
        e.Graphics.DrawLine(Pens.Red, CenterPoint, bottomRight);
    }

    private void Form1_Resize(object sender, EventArgs e)
    {
        this.Invalidate();
    }

    [DllImport("WinMM.dll", SetLastError = true)]
	private static extern uint timeSetEvent(int msDelay, int msResolution, 
        TimerEventHandler handler, ref int userCtx, int eventType);

	[DllImport("WinMM.dll", SetLastError = true)]
	static extern uint timeKillEvent(uint timerEventId);

    public delegate void TimerEventHandler(uint id, uint msg, ref int userCtx,
            int rsv1, int rsv2);
}