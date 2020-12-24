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
    private static System.Random rand = new System.Random();

    public static MazeCell[,] Generate(int width, int height, int courtyardSize = 0) {
        // Create Maze with all walls intact
        MazeCell[,] maze = new MazeCell[width, height];

        for (int i = 0; i < width; ++i) {
            for (int j = 0; j < height; ++j) {
                maze[i, j] = new MazeCell();
            }
        }

        // Add a courtyard by marking the nodes visited and remove the interior walls
        AddCourtyard(maze, width, height, courtyardSize);
        
        // Generate maze
        maze = ApplyRecursiveBacktracker(maze, width, height);
        
        return maze;
    }

    private static MazeCell[,] ApplyRecursiveBacktracker(MazeCell[,] maze, int width, int height) {
        var positionStack = new Stack<Position>();

        // Start at 0,0
        var position = new Position { x = 0, y = 0 };
        maze[position.x, position.y].markVisited();
        positionStack.Push(position);

        while (positionStack.Count > 0) {
            var current = positionStack.Pop();
            var neighbours = GetUnvisitedNeighbours(current, maze, width, height);

            if (neighbours.Count > 0) {
                positionStack.Push(current);

                var randIndex = rand.Next(0, neighbours.Count);
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

    private static void AddCourtyard(MazeCell[,] maze, int width, int height, int courtyardSize) {
        if (courtyardSize > 0) {
            // Place the courtyard at the maze's center
            int xStart = (width / 2) - (courtyardSize / 2);
            int xEnd = xStart + courtyardSize;
            int yStart = (height / 2) - (courtyardSize / 2);
            int yEnd = yStart + courtyardSize;

            for (int i = xStart; i <= xEnd; i++) {
                for (int j = yStart; j <= yEnd; j++) {
                    // Mark the Cell visited so the maze generation will ignore it
                    maze[i,j].markVisited();
                    maze[i,j].setType(CellType.COURTYARD);

                    // Remove all of the interior walls, but leave the outside ones intact
                    if (i != xStart) {
                        maze[i,j].removeWall(Wall.LEFT);
                    } 
                    
                    if (i != xEnd) {
                        maze[i,j].removeWall(Wall.RIGHT);
                    }
                        
                    if (j != yStart) {
                        maze[i,j].removeWall(Wall.DOWN);
                    } 
                    
                    if (j != yEnd ) {
                        maze[i,j].removeWall(Wall.UP);
                    }

                    // Remove corner walls
                    if (i != xStart && j != yEnd) {
                        maze[i,j].removeWall(Wall.UP_LEFT_CORNER);
                    }
                }
            }

            // Knock down a random exterior wall on each side
            maze[xStart, rand.Next(yStart, yEnd)].removeWall(Wall.LEFT);
            maze[xEnd, rand.Next(yStart, yEnd)].removeWall(Wall.RIGHT);
            maze[rand.Next(xStart, xEnd), yStart].removeWall(Wall.DOWN);
            maze[rand.Next(xStart, xEnd), yEnd].removeWall(Wall.UP);
        }
    }
}