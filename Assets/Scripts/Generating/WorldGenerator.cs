using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class WorldGenerator {
    private static System.Random rand = new System.Random();

    // Size of world (mazeSize * 2 + 1)
    private int xLength;
    private int zLength;

    public WorldGenerator(int totalMazesXLength, int totalMazesZLength) {
        this.xLength = ConvertToWorldCoord(totalMazesXLength);
        this.zLength = ConvertToWorldCoord(totalMazesZLength);
    }

    public WorldSpace[,] GenerateWorld(MazeSpec[] mazeSpecs) {
        // Create Empty World
        WorldSpace[,] world = new WorldSpace[xLength, zLength];

        for (int x = 0; x < xLength; x++) {
            for (int z = 0; z < zLength; z++) {
                world[x, z] = new WorldSpace(WorldSpace.Type.ground);
            }
        }

        // Generate all mazes
        foreach (MazeSpec mazeSpec in mazeSpecs) {
            // Validate Maze Specs
            if (mazeSpec.mazeStartX > xLength) {
                throw new ArgumentException("Maze StartX is out of bounds: " + mazeSpec.mazeStartX);
            }
            if (mazeSpec.mazeStartZ > zLength) {
                throw new ArgumentException("Maze StartZ is out of bounds: " + mazeSpec.mazeStartZ);
            }
            if (mazeSpec.mazeStartX + mazeSpec.mazeXLength > xLength) {
                throw new ArgumentException("Maze X Length is out of bounds: " + mazeSpec.mazeXLength);
            }
            if (mazeSpec.mazeStartZ + mazeSpec.mazeZLength > zLength) {
                throw new ArgumentException("Maze Z Length is out of bounds: " + mazeSpec.mazeZLength);
            }

            // Generate Maze
            var maze = MazeGenerator.Generate(mazeSpec);

            // Add Maze to the World
            ApplyMazeToWorld(world, mazeSpec, maze);

            // TODO Add Finish Zone?
            //AddFinishZone(world, GetRandomPosition(mazeSpec));
        }
    
        return world;
    }

    public Vector3 GetRandomPosition(MazeSpec mazeSpec) {
        int xPos = ConvertToWorldCoord(mazeSpec.mazeStartX + rand.Next(mazeSpec.mazeXLength));
        int zPos = ConvertToWorldCoord(mazeSpec.mazeStartZ + rand.Next(mazeSpec.mazeZLength));

        return new Vector3(xPos, 0, zPos);
    }

    private void ApplyMazeToWorld(WorldSpace[,] world, MazeSpec mazeSpec, MazeCell[,] maze) {
        int worldStartX = ConvertToWorldCoord(mazeSpec.mazeStartX) - 1;
        int worldStartZ = ConvertToWorldCoord(mazeSpec.mazeStartZ) - 1;
        int worldEndX = ConvertToWorldCoord(mazeSpec.mazeStartX + mazeSpec.mazeXLength);
        int worldEndZ = ConvertToWorldCoord(mazeSpec.mazeStartZ + mazeSpec.mazeZLength);
        
        // Fill the maze area with walls
        for (int x = worldStartX; x < worldEndX; x++) {
            for (int z = worldStartZ; z < worldEndZ; z++) {
                world[x, z] = new WorldSpace(WorldSpace.Type.wall);
            }
        }

        for (int mazeX = 0; mazeX < mazeSpec.mazeXLength; mazeX++)
        {
            for (int mazeZ = 0; mazeZ < mazeSpec.mazeZLength; mazeZ++)
            {
                var cell = maze[mazeX, mazeZ];
                int worldX = worldStartX + ConvertToWorldCoord(mazeX);
                int worldZ = worldStartZ + ConvertToWorldCoord(mazeZ);

                // Set cell's coordinates to floor type
                world[worldX, worldZ].type = WorldSpace.Type.floor;

                // Knock down walls
                if (!cell.isWallUp(Wall.UP))
                {
                    int wallX = worldX;
                    int wallZ = worldZ + 1;
                    world[wallX, wallZ].type = WorldSpace.Type.floor;
                }

                if (!cell.isWallUp(Wall.LEFT))
                {
                    int wallX = worldX - 1;
                    int wallZ = worldZ;
                    world[wallX, wallZ].type = WorldSpace.Type.floor;
                }

                if (!cell.isWallUp(Wall.RIGHT))
                {
                    int wallX = worldX + 1;
                    int wallZ = worldZ;
                    world[wallX, wallZ].type = WorldSpace.Type.floor;
                }

                if (!cell.isWallUp(Wall.DOWN))
                {
                    int wallX = worldX;
                    int wallZ = worldZ - 1;
                    world[wallX, wallZ].type = WorldSpace.Type.floor;
                }

                if (!cell.isWallUp(Wall.UP_LEFT_CORNER))
                {
                    int wallX = worldX - 1;
                    int wallZ = worldZ + 1;
                    world[wallX, wallZ].type = WorldSpace.Type.floor;
                }
            }
        }
    }

    public void AddFinishZone(WorldSpace[,] world, Vector3 pos) {
        world[(int)pos.x, (int)pos.z].type = WorldSpace.Type.finish;
    }

    public int getXLength() {
        return xLength;
    }

    public int getZLength() {
        return zLength;
    }

    private int ConvertToWorldCoord(int coord) {
        return coord * 2 + 1;
    }
}