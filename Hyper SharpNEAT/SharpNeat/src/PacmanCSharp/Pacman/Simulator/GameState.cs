using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using Pacman.Simulator.Ghosts;

namespace Pacman.Simulator
{
	public enum Maze { Red, LightBlue, Brown, DarkBlue, None };

	public class GameState
	{
		private int level = 0;
		public int Level { get { return level; } }

		public static readonly Random Random = new Random();

		public readonly Pacman Pacman;		
		public readonly Red Red;
		public readonly Pink Pink;
		public readonly Blue Blue;
		public readonly Brown Brown;
		public readonly Ghost[] Ghosts = new Ghost[4];

		public BasePacman Controller;

		public const int MSPF = 40;
		public long Timer = 0;
		public long ElapsedTime = 0;
		public long Frames = 0;

		public Image[] Mazes = new Image[4];

		private Map[] maps = new Map[4];
		private Map map;
		public Map Map { get { return map; } }

		private bool started = false;
		public bool Started { get { return started; } }

		private const int reversalTime1 = 5000, reversalTime2 = 25000; // estimates
		private int reversal1 = reversalTime1, reversal2 = reversalTime2;		

		// settings
		public bool PacmanMortal = true;
		public bool NaturalReversals = true;
		public bool Replay = false;
		public bool AutomaticLevelChange = true;

		public event EventHandler GameOver;
		public event EventHandler PacmanDead = new EventHandler(delegate(object sender, EventArgs e){ });

        public static bool mapsHaveBeenPreloaded;
        public static Map[] preloadedMaps = new Map[4];
        public float timeStep = 0;

		public GameState() {
            if (!mapsHaveBeenPreloaded) {
                PreloadMazes();
            }
            maps = GameState.preloadedMaps;
			map = maps[Level];
			// default position ... find out where
			Pacman = new Pacman(Pacman.StartX, Pacman.StartY, this);
			Ghosts[0] = Red = new Red(Red.StartX, Red.StartY, this);
			Ghosts[1] = Pink = new Pink(Pink.StartX, Pink.StartY, this);
			Ghosts[2] = Blue = new Blue(Blue.StartX, Blue.StartY, this);
			Ghosts[3] = Brown = new Brown(Brown.StartX, Brown.StartY, this);			
		}

		public GameState(Pacman pacman, Red red, Pink pink, Blue blue, Brown brown) {
            if (!mapsHaveBeenPreloaded) {
                PreloadMazes();
            }
            maps = GameState.preloadedMaps;
			map = maps[Level];
			this.Pacman = pacman;
			Ghosts[0] = this.Red = red;
			Ghosts[1] = this.Pink = pink;
			Ghosts[2] = this.Blue = blue;
			Ghosts[3] = this.Brown = brown;			
		}

		private void loadMazes(){
			for( int i = 0; i < 4; i++ ) {
				Mazes[i] = Util.LoadImage("ms_pacman_maze" + (i + 1) + ".gif");
				maps[i] = new Map((Bitmap)Mazes[i],(Maze)i);
			}
		}

        private void PreloadMazes() {
            Map[] maps = new Map[4];
            Image[] Mazes = new Image[4];
            for (int i = 0; i < 4; i++)
            {
                Mazes[i] = Util.LoadImage("ms_pacman_maze" + (i + 1) + ".gif");
                maps[i] = new Map((Bitmap)Mazes[i], (Maze)i);
            }
            GameState.preloadedMaps = maps;
            GameState.mapsHaveBeenPreloaded = true;
        }
         
		public void StartPlay() {
			started = true;
			Timer = 0;
			Frames = 0;
		}

		public void PausePlay() {
            TestShortestPathDir();
			started = false;
		}

        void TestShortestPathDir() {
            var path = Pacman.Node.ShortestPath[Ghosts[0].Node.X, Ghosts[0].Node.Y, (int)Direction.Down];
            if (path != null)
            {
                Console.WriteLine("Down  " + path.Distance);
            }
            else
            {
                Console.WriteLine("Down  " + "--");
            }


            path = Pacman.Node.ShortestPath[Ghosts[0].Node.X, Ghosts[0].Node.Y, (int)Direction.Up];
            if (path != null)
            {
                Console.WriteLine("Up    " + path.Distance);
            }
            else
            {
                Console.WriteLine("Up    " + "--");
            }


            path = Pacman.Node.ShortestPath[Ghosts[0].Node.X, Ghosts[0].Node.Y, (int)Direction.Left];
            if (path != null)
            {
                Console.WriteLine("Left  " + path.Distance);
            }
            else
            {
                Console.WriteLine("Left  " + "--");
            }


            path = Pacman.Node.ShortestPath[Ghosts[0].Node.X, Ghosts[0].Node.Y, (int)Direction.Right];
            if (path != null)
            {
                Console.WriteLine("Right " + path.Distance);
            }
            else
            {
                Console.WriteLine("Right " + "--");
            }

            path = Pacman.Node.ShortestPath[Ghosts[0].Node.X, Ghosts[0].Node.Y, (int)Direction.None];
            if (path != null)
            {
                Console.WriteLine("All   "+ path.Distance +"  ("+path.Direction+")");
                //Console.WriteLine("All   " + path.Direction);
            }
            else
            {
                Console.WriteLine("-" + " All");
            }
            Console.WriteLine("");
        }

		public void ResumePlay() {
			started = true;
		}

		public void ReverseGhosts() {
			foreach( Ghost g in Ghosts ) {
				g.Reversal();
			}
		}

        public int OptionFromNextJunction(Direction initialDir) {
            Node bestNode = null;
            var path = StateInfo.NearestJunction(Pacman.Node, this, initialDir, out bestNode);
            if (bestNode != null && path != null) {
                foreach (var ghost in Ghosts) {
                    var ghostPath = ghost.Node.ShortestPath[bestNode.X,bestNode.Y, (int)Direction.None];
                    if (ghostPath != null) {
                        int ghostDistance = ghostPath.Distance;
                        int pacDistance = path.Distance;
                        if (ghostDistance<pacDistance) {
                            return 0;
                        }
                    }
                }
                return GetMostNumberOfJunctionsInPath(bestNode, path.ParentNode, 20);
            }
            return 0;
        }

        public double GetProportionsOfEdibleGhosts() {
            int countChosts = 0;
            foreach (Ghost ghost in Ghosts) {
                if (ghost.IsEdible()) {
                    countChosts++;
                }
            }
            return countChosts/(double)Ghosts.Length;
        }

        public double GetProportionOfFleeTime() {
            foreach (Ghost ghost in Ghosts) {
                if (Timer < ghost.GetFleetDuration()) break;
                if ((double)ghost.RemainingFlee>0) {
                    return (double)ghost.RemainingFlee / (double)ghost.GetFleetDuration();
                }
            }
            return (double)0;
        }

        public bool AnyGhostEdible() {
            foreach (Ghost ghost in Ghosts)
            {
                if (ghost.IsEdible()) {
                    return true;
                }
            }
            return false;
        }

        public bool AllGhostOutsideLair()
        {
            foreach (Ghost ghost in Ghosts)
            {
                if (ghost.IsInLair())
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsNeareastPowerPillWithinDistance(int dist) {
            int distToPowerPill = DistanceToClosestPowerPill();
            if (distToPowerPill == -1) return false;
            return (distToPowerPill < dist);
        }

        public int DistanceToClosestPowerPill() {
            int minDistance = -1; 
            foreach (Node potentialPowerPill in Map.PowerPillNodes) {
                if (potentialPowerPill.Type == Node.NodeType.PowerPill) {
                    var path = Pacman.Node.ShortestPath[potentialPowerPill.X, potentialPowerPill.Y, (int)Direction.None];
                    if (path != null) {
                        if (minDistance == -1 || path.Distance<minDistance) {
                            minDistance = path.Distance;
                        }
                    }
                }
            }
            return minDistance;
        }

        public int GetMostNumberOfPillInPath(Node node, Node parentNode, int distance) {
            if (!node.Walkable) return 0;
            int nPill = (node.Type == Node.NodeType.Pill)? 1 : 0;
            if (distance <= 1) return nPill;
            List<int> tmpList = new List<int>();
            foreach (Node newNode in node.PossibleDirections) {
                if (newNode != parentNode) {
                    tmpList.Add(nPill+GetMostNumberOfPillInPath(newNode, node, distance - 1));
                }
            }
            if (tmpList.Count>0) {
                tmpList.Sort();
                return tmpList[tmpList.Count-1];
            }
            return 0;
        }

        public int GetMostNumberOfJunctionsInPath(Node node, Node parentNode, int distance) {
            if (!node.Walkable) return 0;
            int nJunction = (node.PossibleDirections.Count>2) ? 1 : 0;
            if (distance <= 1) return nJunction;
            List<int> tmpList = new List<int>();
            foreach (Node newNode in node.PossibleDirections)
            {
                if (newNode != parentNode)
                {
                    tmpList.Add(nJunction + GetMostNumberOfJunctionsInPath(newNode, node, distance - 1));
                }
            }
            if (tmpList.Count > 0)
            {
                tmpList.Sort();
                return tmpList[tmpList.Count - 1];
            }
            return 0;
        }

		public void LoadMaze(Maze maze) {
			map = maps[(int)maze];
			map.Reset();
			Pacman.ResetPosition();
			foreach( Ghost g in Ghosts ) {
				g.ResetPosition();
			}
		}

		public void Update() {
			if( !started ) {
				return;
			}
			Frames++;
			Timer += MSPF;
			ElapsedTime += MSPF;
			// change level
			// TODO: use levels instead of just mazes
			if( Map.PillsLeft == 0 && AutomaticLevelChange ) {
				//level = Level + 1; // test for screenplayer
				if( Level > Mazes.Length - 1 ) level = 0;
				map = maps[Level];
				map.Reset();
				resetTimes();
				Pacman.ResetPosition();
				foreach( Ghost g in Ghosts ) {
					g.ResetPosition();
				}
				if( Controller != null ) {
					Controller.LevelCleared();
				}
				return;
			}
			// ghost reversals
			if( NaturalReversals ) {
				bool ghostFleeing = false;
				foreach( Ghost g in Ghosts ) {
					if( !g.Chasing ) {
						ghostFleeing = true;
						break;
					}
				}
				if( ghostFleeing ) {
					reversal1 += MSPF;
					reversal2 += MSPF;
				} else {
					if( Timer > reversal1 ) {
						reversal1 += 1200000; // 20 min
						ReverseGhosts();
					}
					if( Timer > reversal2 ) {
						reversal2 = Int32.MaxValue;
						ReverseGhosts();
					}
				}
			}
			if( Replay ) {
				// do nothing
			}
			else {
				// move
				Pacman.Move();
				foreach( Ghost g in Ghosts ) {
					g.Move();
					// check collisions				
					if( g.Distance(Pacman) < 4.0f ) {
						if( g.Chasing ) {
							if( PacmanMortal ) {
								resetTimes();
								Pacman.Die();								
								PacmanDead(this, null);
								foreach( Ghost g2 in Ghosts ) {
									g2.PacmanDead();
								}
								if( Pacman.Lives == -1 ) {
									InvokeGameOver();
								}
								break;
							}
						} else if( g.Entered ) {
							Pacman.EatGhost();
							g.Eaten();
						}
					}
				}
			}
		}

		public void InvokeGameOver() {
			GameOver(this, null);
			ElapsedTime = 0;
			level = 0;
			map = maps[Level];
			map.Reset();
			Pacman.Reset();			
			foreach( Ghost g in Ghosts ) {
				g.PacmanDead();
			}			
		}

		private void resetTimes() {
			Timer = 0;
			reversal1 = reversalTime1; 
			reversal2 = reversalTime2;
		}

		public GameState Clone() {
			throw new NotImplementedException();			
		}		
	}
}
