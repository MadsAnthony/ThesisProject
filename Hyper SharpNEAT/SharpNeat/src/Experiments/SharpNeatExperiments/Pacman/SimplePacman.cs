using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Pacman.Simulator;

namespace SharpNeatExperiments.Pacman
{
    public partial class SimplePacman : Form
    {
        private Image image;
        private Graphics g;
        private TimerEventHandler tickHandler;
        private uint fastTimer;
        private static bool[] keys_down;
        private static Keys[] key_props;
        public int width;
        public int height;
        int zoom = 4;

        public int timeOut = 5000;
        public int timer = 0;
        int switchTime = 200;
        int taskTimer = 0;
        public int score = 1000;
        public int eatScore = 0;
        public int lifeScore = 1000;
        public PacmanAINeural.SimplePacmanController controller;
        public PacmanAINeural.SimplePacmanEnemyController[] enemies;
        public int returnGameScore;
        public int returnEatScore;
        public int returnLifeScore;

        bool fastNoDraw = true;
        bool dontThink = false;
        public Random Random;

        public SimplePacman(PacmanAINeural.SimplePacmanController controller, bool fastNoDraw, Random rand = null)
        {
            InitializeComponent();
            KeyDown += new KeyEventHandler(keyDownHandler);
            KeyUp += new KeyEventHandler(keyUpHandler);

            keys_down = new bool[1];
            key_props = new[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right};

            width = Picture.Width / zoom;
            height = Picture.Height / zoom;
            image = new Bitmap(Picture.Width, Picture.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(image);

            if (controller != null) {
                this.controller = controller;
            } else {
                this.controller = new PacmanAINeural.SimplePacmanController();
            }
            this.controller.gameState = this;

            enemies = new PacmanAINeural.SimplePacmanEnemyController[3];

            for (int i = 0; i<enemies.Length;i++) {
                enemies[i] = new PacmanAINeural.SimplePacmanEnemyController(this);
                enemies[i].reactionTime = (i*15)+ 10;
            }
            controller.pos = new Point(3*width / 8, 3*height / 8);
            enemies[0].pos = new Point(  width / 8,   height / 8);
            enemies[1].pos = new Point(6*width / 8,   height / 8);
            /*enemies[2].pos = new Point(  width / 8, 6*height / 8);
            enemies[3].pos = new Point(6*width / 8, 6*height / 8);*/

            //int myData = 0; // dummy data
            /*tickHandler = new TimerEventHandler(tick);
            fastTimer = timeSetEvent(fastNoDraw ? 1 : 20, fastNoDraw? 1:20, tickHandler, ref myData, 1);*/

            Application.ApplicationExit += new EventHandler(closeHandler);

            if (rand == null) {
                Random = new Random();
            } else {
                Random = rand;
            }
            this.fastNoDraw = fastNoDraw;
            if (fastNoDraw) {
                loop();
            } else {
                loopWithWaiting();
                /*int myData = 0; // dummy data
                tickHandler = new TimerEventHandler(tick);
                fastTimer = timeSetEvent(fastNoDraw ? 1 : 20, fastNoDraw? 1:20, tickHandler, ref myData, 1);*/
            }
        }

        void loopWithWaiting() {
            /*while (true) {
                myTick(true);
                Thread.Sleep(1);
            }
            closingStuff();*/
            Thread extraWindowThread;
            extraWindowThread = new System.Threading.Thread(delegate()
            {
                while (myTick(true))
                {
                    //int myData = 0;
                    //myTick(true);
                    //tick(0, 0, ref myData, 0, 0);
                    Thread.Sleep(20);
                }
                closingStuff();
                CloseGame();
            });
            extraWindowThread.Start();
        }

        void loop() {
            while (myTick(false)) {
            }
            closingStuff();
            /*Thread extraWindowThread;
            extraWindowThread = new System.Threading.Thread(delegate()
            {
                while (true)
                {
                    int myData = 0;
                    tick(0, 0, ref myData, 0, 0);
                    //Thread.Sleep(20);
                }
            });
            extraWindowThread.Start();*/
        }
        void closingStuff() {
            returnGameScore = score;
            returnEatScore = eatScore;
            returnLifeScore = lifeScore;
        }

        public void CloseGame() {
            closingStuff();
            /*returnGameScore = score;
            returnEatScore = eatScore;
            returnLifeScore = lifeScore;*/
            timeKillEvent(fastTimer);
            this.Close();
        }

        public bool myTick(bool shouldDraw) {
            timer++;
            if (timer > timeOut)
            {
                return false;
                //CloseGame();
            }

            taskTimer++;

            CheckIfSwitchTask();

            // move
            if (!dontThink)
            {
                controller.Think();
            }
            MoveController();
            foreach (var enemy in enemies)
            {
                enemy.Move();
            }

            // check for collision
            controller.CheckForHit();

            // draw
            if (shouldDraw)
            {
                Draw();
            }
            return true;
        }

        private void tick(uint id, uint msg, ref int userCtx, int rsv1, int rsv2) {
            timer++;
            if (timer>timeOut) {
                CloseGame();
            }

            taskTimer++;

            CheckIfSwitchTask();

            // move
            if (!dontThink) {
                controller.Think();
            }
            MoveController();
            foreach (var enemy in enemies) {
                enemy.Move();
            }

            // check for collision
            controller.CheckForHit();

            // draw
            if (!fastNoDraw) {
                Draw();
            }
        }

        private void CheckIfSwitchTask() {
            if (taskTimer==switchTime) {
                foreach (var enemy in enemies) {
                    enemy.isGettingReady = true;
                    enemy.isSleeping = false;
                    enemy.isEdible = !enemy.isEdible;
                }
                //taskTimer = 0;
            }
            if (taskTimer > switchTime+10)
            {
                foreach (var enemy in enemies)
                {
                    enemy.isGettingReady = false;
                }
                /*Random r = new Random();
                switchTime = r.Next(10, 200);*/
                taskTimer = 0;
            }
        }

        private void Draw() {
            g.Clear(Color.Black);

            label1.Text = score.ToString();
            DrawController();
            foreach (var enemy in enemies) {
                DrawEnemy(enemy);
            }
            Picture.Image = image;
        }

        private void DrawController() {
            int radius = 20;
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(controller.pos.X * zoom, controller.pos.Y * zoom, radius, radius);
            Pen pen = new System.Drawing.Pen(Color.Cyan);
            g.DrawEllipse(pen, rectangle);
        }

        private void DrawEnemy( PacmanAINeural.SimplePacmanEnemyController enemy) {
            int radius = 20;
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(enemy.pos.X * zoom, enemy.pos.Y * zoom, radius, radius);
            Pen pen = new System.Drawing.Pen(Color.Yellow);
            if (!enemy.isSleeping) {
                pen = new System.Drawing.Pen(enemy.isEdible ? Color.Green : Color.Red);
                if (enemy.isGettingReady) {
                pen = new System.Drawing.Pen(Color.DarkMagenta);
                }
            }
            g.DrawEllipse(pen, rectangle);
        }

        private void closeHandler(object sender, EventArgs args) {
            timeKillEvent(fastTimer);
            returnGameScore = score;
            this.Close();
        }

        private void MoveController() {
            byte n = 0;
            foreach (var v in keys_down) {
                if (n == 0 && v) {controller.SetDirection(Direction.Up); break; }
                if (n == 1 && v) {controller.SetDirection(Direction.Down); break; }
                if (n == 2 && v) {controller.SetDirection(Direction.Left); break; }
                if (n == 3 && v) {controller.SetDirection(Direction.Right); break; } 
                n++;
            }
        }

        private void keyDownHandler(object sender, KeyEventArgs e) {
            byte n = 0;
            foreach (var v in keys_down) {
                if (e.KeyCode == key_props[n])
                    keys_down[n] = true;
                n++;
            }
        }

        private void keyUpHandler(object sender, KeyEventArgs e) {
            byte n = 0;
            foreach (var v in keys_down)
            {
                if (e.KeyCode == key_props[n])
                    keys_down[n] = false;
                n++;
            }
        }

        [DllImport("WinMM.dll", SetLastError = true)]
        private static extern uint timeSetEvent(int msDelay, int msResolution,
            TimerEventHandler handler, ref int userCtx, int eventType);

        [DllImport("WinMM.dll", SetLastError = true)]
        static extern uint timeKillEvent(uint timerEventId);

        public delegate void TimerEventHandler(uint id, uint msg, ref int userCtx,
                int rsv1, int rsv2);

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
