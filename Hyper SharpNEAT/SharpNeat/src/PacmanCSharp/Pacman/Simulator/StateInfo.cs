using System;
using System.Collections.Generic;
using System.Text;

namespace Pacman.Simulator
{
	public static class StateInfo
	{
		public static bool IsInverse(Direction d1, Direction d2) {
			if( d1 == Direction.Up && d2 == Direction.Down ) return true;
			if( d1 == Direction.Down && d2 == Direction.Up ) return true;
			if( d1 == Direction.Left && d2 == Direction.Right ) return true;
			if( d1 == Direction.Right && d2 == Direction.Left ) return true;
			return false;
		}

		public static Direction GetInverse(Direction d) {
			switch( d ) {
				case Direction.Up: return Direction.Down;
				case Direction.Down: return Direction.Up;
				case Direction.Left: return Direction.Right;
				case Direction.Right: return Direction.Left;
			}
			return Direction.None;
		}

		public static PillPath NearestPill(Node currentNode, GameState gs) {
			Node bestNode = null;
			Node.PathInfo bestPath = null;
			foreach( Node node in gs.Map.Nodes ) {
				if( node.Walkable ) {					
					if( node.Type == Node.NodeType.Pill || node.Type == Node.NodeType.PowerPill ) {
						if( bestPath == null ) {
							bestNode = node;
                            bestPath = gs.Pacman.Node.ShortestPath[node.X, node.Y, (int)Direction.None];
							continue;
						}
                        Node.PathInfo curPath = currentNode.ShortestPath[node.X, node.Y, (int)Direction.None];
						if( curPath != null && curPath.Distance < bestPath.Distance ) {
							bestNode = node;
							bestPath = curPath;
						}
					}
				}
			}
			return new PillPath(bestNode, bestPath);			
		}

        public static Node.PathInfo NearestPill(Node currentNode, GameState gs, Direction initialDir, Node.NodeType type)
        {
            Node bestNode = null;
            Node.PathInfo bestPath = null;
            foreach (Node node in gs.Map.Nodes)
            {
                if (node.Walkable)
                {
                    if (node.Type == type)
                    {
                        if (bestPath == null)
                        {
                            bestNode = node;
                            bestPath = gs.Pacman.Node.ShortestPath[node.X, node.Y, (int)initialDir];
                            continue;
                        }
                        Node.PathInfo curPath = currentNode.ShortestPath[node.X, node.Y, (int)initialDir];
                        if (curPath != null && curPath.Distance < bestPath.Distance)
                        {
                            bestNode = node;
                            bestPath = curPath;
                        }
                    }
                }
            }
            return bestPath;
        }

        public static Node.PathInfo NearestJunction(Node currentNode, GameState gs, Direction initialDir)
        {
            Node bestNode = null;
            return NearestJunction(currentNode, gs, initialDir, out bestNode);
        }
        public static Node.PathInfo NearestJunction(Node currentNode, GameState gs, Direction initialDir, out Node bestNode)
        {
            bestNode = null;
            Node.PathInfo bestPath = null;
            foreach (Node node in gs.Map.Nodes)
            {
                if (node.Walkable)
                {
                    if (node.PossibleDirections.Count > 2)
                    {
                        if (bestPath == null)
                        {
                            bestNode = node;
                            bestPath = gs.Pacman.Node.ShortestPath[node.X, node.Y, (int)initialDir];
                            continue;
                        }
                        Node.PathInfo curPath = currentNode.ShortestPath[node.X, node.Y, (int)initialDir];
                        if (curPath != null && curPath.Distance < bestPath.Distance)
                        {
                            bestNode = node;
                            bestPath = curPath;
                        }
                    }
                }
            }
            return bestPath;
        }

		public static PillPath FurthestPill(Node currentNode, GameState gs) {
			Node bestNode = null;
			Node.PathInfo bestPath = null;
			foreach( Node node in gs.Map.Nodes ) {
				if( node.Walkable ) {
					if( node.Type == Node.NodeType.Pill || node.Type == Node.NodeType.PowerPill ) {
						if( bestPath == null ) {
							bestNode = node;
                            bestPath = gs.Pacman.Node.ShortestPath[node.X, node.Y, (int)Direction.None];
							continue;
						}
                        Node.PathInfo curPath = currentNode.ShortestPath[node.X, node.Y, (int)Direction.None];
						if( curPath != null && curPath.Distance > bestPath.Distance ) {
							bestNode = node;
							bestPath = curPath;
						}
					}
				}
			}
			return new PillPath(bestNode, bestPath);
		}

		public class PillPath
		{
			public Node Target;
			public Node.PathInfo PathInfo;

			public PillPath(Node target, Node.PathInfo pathInfo) {
				this.Target = target;
				this.PathInfo = pathInfo;
			}
		}
	}
}
