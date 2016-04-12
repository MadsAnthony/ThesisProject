using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Pacman.Simulator;
using Pacman.Simulator.Ghosts;

using SharpNeatLib.Evolution;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.CPPNs;
using System.Drawing;
namespace PacmanAINeural
{
    public class SimplePacmanEnemyController
    {
        SharpNeatExperiments.Pacman.SimplePacman gameState;
        public Point pos;
        public bool isEdible = true;
        public bool isSleeping;
        int timer = 0;
        public int reactionTime = 10;
        Point controllerPos;

        public SimplePacmanEnemyController(SharpNeatExperiments.Pacman.SimplePacman gameState) {
            pos = new Point(0, 0);
            this.gameState = gameState;
        }

        public void Move() {
            timer ++;
            if (isEdible || isSleeping) return;
            if (timer>reactionTime) {
                controllerPos = gameState.controller.pos;
                // check if it's faster to go through the edge.
                if (Math.Abs(controllerPos.Y+gameState.height - pos.Y)<Math.Abs(controllerPos.Y - pos.Y)) {
                    controllerPos.Y = controllerPos.Y + gameState.height;
                }
                if (Math.Abs(controllerPos.Y-gameState.height - pos.Y)<Math.Abs(controllerPos.Y - pos.Y)) {
                    controllerPos.Y = controllerPos.Y - gameState.height;
                }
                if (Math.Abs(controllerPos.X+gameState.width - pos.X)<Math.Abs(controllerPos.X - pos.X)) {
                    controllerPos.X = controllerPos.X + gameState.width;
                }
                if (Math.Abs(controllerPos.X-gameState.width - pos.X)<Math.Abs(controllerPos.X - pos.X)) {
                    controllerPos.X = controllerPos.X - gameState.width;
                }
                //var v = new Point(controllerPos.X - pos.X, controllerPos.Y - pos.Y);
                timer = 0;
            }
            var dirV = new Point(controllerPos.X - pos.X, controllerPos.Y - pos.Y);
            if (Math.Abs(dirV.X)<Math.Abs(dirV.Y)) {
                if (dirV.Y<0) {
                    SetDirection(Direction.Up);
                } else {
                    SetDirection(Direction.Down);
                }
            } else {
                if (dirV.X<0) {
                    SetDirection(Direction.Left);
                } else {
                    SetDirection(Direction.Right);
                }
            }
        }

        private void SetInsideEdgesControllerPos() {
            controllerPos = new Point(gameState.controller.pos.X%gameState.width,gameState.controller.pos.Y%gameState.height);
        }

        public void Sleep() {
            isSleeping = true;
            Random random = new Random();
            int randomX = random.Next(0, gameState.width);
            int randomY = random.Next(0, gameState.height);
            pos = new Point(randomX, randomY);
        }

        public void SetDirection(Direction dir) {
            int newX, newY;
            switch (dir) {
                case Direction.Up:
                    newY = pos.Y - 1;
                    if (newY<0) {
                        newY = gameState.height;
                        SetInsideEdgesControllerPos();
                    }
                    pos = new Point(pos.X, newY);
                    break;
                case Direction.Down:
                    newY = pos.Y + 1;
                    if (newY > gameState.height) {
                        newY = 0;
                        SetInsideEdgesControllerPos();
                    }
                    pos = new Point(pos.X, newY);
                    break;
                case Direction.Left:
                    newX = pos.X - 1;
                    if (newX < 0) {
                        newX = gameState.width;
                        SetInsideEdgesControllerPos();
                    }
                    pos = new Point(newX, pos.Y);
                    break;
                case Direction.Right:
                    newX = pos.X + 1;
                    if (newX > gameState.width) {
                        newX = 0;
                        SetInsideEdgesControllerPos();
                    }
                    pos = new Point(newX, pos.Y);
                    break;
            }
        }
    }
}
