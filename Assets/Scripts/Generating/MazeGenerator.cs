using System.Collections;
using System.Collections.Generic;
using System;

public struct Position
{
    public int x;
    public int y;
}

public struct Neighbour
{
    public Position position;
    public Wall sharedWall;
}

public static class MazeGenerator {

    public static MazeCell[,] Generate(int width, int height) {
        // Create Maze with all walls intact
        MazeCell[,] maze = new MazeCell[width, height];

        for (int i = 0; i < width; ++i) {
            for (int j = 0; j < height; ++j) {
                maze[i, j] = new MazeCell();
            }
        }
        
        // Generate maze
        maze = ApplyRecursiveBacktracker(maze, width, height);
        
        return maze;
    }

    private static MazeCell[,] ApplyRecursiveBacktracker(MazeCell[,] maze, int width, int height) {
        var rng = new System.Random(/*seed*/);
        var positionStack = new Stack<Position>();

        // Pick random start position
        var position = new Position { x = rng.Next(0, width), y = rng.Next(0, height) };
        maze[position.x, position.y].markVisited();
        positionStack.Push(position);

        while (positionStack.Count > 0) {
            var current = positionStack.Pop();
            var neighbours = GetUnvisitedNeighbours(current, maze, width, height);

            if (neighbours.Count > 0) {
                positionStack.Push(current);

                var randIndex = rng.Next(0, neighbours.Count);
                var randomNeighbour = neighbours[randIndex];

                var nPosition = randomNeighbour.position;
                maze[current.x, current.y].removeWall(randomNeighbour.sharedWall);
                maze[nPosition.x, nPosition.y].removeWall(MazeCell.GetOppositeWall(randomNeighbour.sharedWall));
                maze[nPosition.x, nPosition.y].markVisited();

                positionStack.Push(nPosition);
            }
        }

        return maze;
    }

    private static List<Neighbour> GetUnvisitedNeighbours(Position p, MazeCell[,] maze, int width, int height) {
        var list = new List<Neighbour>();

        // Left
        if (p.x > 0) {
            if (!maze[p.x - 1, p.y].isVisited()) {
                list.Add(new Neighbour
                {
                    position = new Position
                    {
                        x = p.x - 1,
                        y = p.y
                    },
                    sharedWall = Wall.LEFT
                });
            }
        }

        // DOWN
        if (p.y > 0) {
            if (!maze[p.x, p.y - 1].isVisited()) {
                list.Add(new Neighbour
                {
                    position = new Position
                    {
                        x = p.x,
                        y = p.y - 1
                    },
                    sharedWall = Wall.DOWN
                });
            }
        }

        // UP
        if (p.y < height - 1) {
            if (!maze[p.x, p.y + 1].isVisited()) {
                list.Add(new Neighbour
                {
                    position = new Position
                    {
                        x = p.x,
                        y = p.y + 1
                    },
                    sharedWall = Wall.UP
                });
            }
        }

        // RIGHT
        if (p.x < width - 1) {
            if (!maze[p.x + 1, p.y].isVisited()) {
                list.Add(new Neighbour
                {
                    position = new Position
                    {
                        x = p.x + 1,
                        y = p.y
                    },
                    sharedWall = Wall.RIGHT
                });
            }
        }

        return list;
    }
}