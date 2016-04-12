using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SharpNeatLib.NeuralNetwork;

namespace SharpNeatLib.Experiments
{
    public class Board
    {
        public static Size robotSize = new Size(25, 25);
        private static Size foodSize = new Size(5,5);
        private PointF location;
        private SizeF size;
        private List<RoboAgent> agents=new List<RoboAgent>();
        private List<Robot> robots = new List<Robot>();
        public int ticks =0;
        public int timerTicks = 0;
        public PointF Location
        {
            get { return location; }
        }
        public SizeF Size
        {
            get { return size; }
        }

        public Board()
        {
            location = new PointF(0.0F, 0.0F);
            size = new SizeF(0, 0);
        }
        public Board(float where, float dim)
        {
            location = new PointF(where, where);
            size = new SizeF(dim, dim);
        }
        public void AddRobot(Robot r)
        {
            if (robots.Count == 0)
            {
                robots.Add(r);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Only one robot at a time fella!");
            }
        }
        public void AddRobot(PointF location,INetwork net)
        {
            if (robots.Count == 0)
            {
                robots.Add(new Robot(location,4,net));
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Only one robot at a time fella!");
            }
        }
        public void AddRobot(PointF location)
        {
            if (robots.Count == 0)
            {
                robots.Add(new Robot(location));
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Only one robot at a time fella!");
            }
        }
        public bool isValidMove(RoboAgent a, SizeF move)
        {
            if (move.Height == 0 && move.Width == 0)
                return false;
            double dx = a.Location.X + move.Width;
            double dy = a.Location.Y + move.Height;
            if (dx > this.Size.Width || dx < 0 || dy > this.size.Height || dy < 0)
                return false;
            else
                return true;
        }
        public int game()
        {
            Robots[0].brain.MultipleSteps(5);
            ticks = 0;
            while (turn() == false)
                if(ticks>=1000)
                    break;
            return ticks;
        }
        public int gameView(System.Windows.Forms.Form f)
        {
            ticks = 0;
            while (turnView() == false)
            {
                f.Refresh();
                System.Threading.Thread.Sleep(10);
                if (ticks >= 1000)
                    break;
            }
            return ticks;  
        }

        public bool turn()
        {
            ticks++;
            for(int j=0;j<Robots.Count;j++) //(Robot r in robots)
            {
                Robots[j].clearSensors();
                foreach (RoboAgent a in agents)
                {
                    if (Utilities.Distance(Robots[j], a) <= 5)
                    {
                        Agents.Remove(a);
                        return true;
                    }
                    else
                        Robots[j].ProcessAgent(a);
                }
                SizeF pt = Robots[j].determineBiasedMove();
                //SizeF pt = Robots[j].determineMove();
                if (isValidMove(Robots[j], pt))
                    Robots[j].Move(pt);
                else
                {
                    Robots[j].InvalidMoves = Robots[j].InvalidMoves + 1;
                    if (Robots[j].InvalidMoves >= 10)
                    {
                        ticks = 1000;
                        return true;
                    }
                }
                
            }
            return false;
        }
        public bool turnView()
        {
            ticks++;
            for (int j = 0; j < Robots.Count; j++)
            {
                Robots[j].clearSensors();
                foreach (RoboAgent a in agents)
                {
                    if (Utilities.Distance(Robots[j], a) <= 10)
                    {
                        Agents.Remove(a);
                        return true;
                    }
                    else
                        Robots[j].ProcessAgent(a);
                }
                SizeF pt = Robots[j].determineBiasedMove();
                //SizeF pt = Robots[j].determineMove();
                if (isValidMove(Robots[j], pt))
                    Robots[j].Move(pt);
                else
                {
                    Robots[j].InvalidMoves = Robots[j].InvalidMoves + 1;
                    if (Robots[j].InvalidMoves >= 10)
                    {
                        ticks = 1000;
                        return true;
                    }
                }
            }
            return false;

        }
        #region Accessors
        public void AddFood(Food f)
        {
            agents.Add(f);
        }
        public void AddFood(PointF loc)
        {
            agents.Add(new Food(loc));
        }
        public List<RoboAgent> Agents
        {
            get { return agents; }
        }
        public List<Robot> Robots
        {
            get { return robots; }
            set { robots = value; }
        }
        public Size RobotSize
        {
            get { return robotSize; }
        }
        public Size FoodSize
        {
            get { return foodSize; }
        }
        public int Ticks
        {
            get {return ticks;}
        }
        #endregion


    }
}
