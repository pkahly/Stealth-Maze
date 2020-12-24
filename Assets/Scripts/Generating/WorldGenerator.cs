using System;
using System.Collections;
using System.Collections.Generic;


class WorldGenerator {
    private static System.Random rand = new System.Random();

    // Size of Maze Array
    private int mazeXLength;
    private int mazeZLength;
    private int courtyardSize;
    
    // Size of world (mazeSize * 2 + 1)
    private int xLength;
    private int zLength;

    public WorldGenerator(int mazeXLength, int mazeZLength, int courtyardSize = 0) {
        this.mazeXLength = mazeXLength;
        this.mazeZLength = mazeZLength;
        this.courtyardSize = courtyardSize;

        // The World must be bigger to hold the walls
        this.xLength = mazeXLength * 2 + 1;
        this.zLength = mazeZLength * 2 + 1;
    }

    public WorldSpace[,] GenerateWorld() {
        // Create Empty World
        WorldSpace[,] world = new WorldSpace[xLength, zLength];

        for (int x = 0; x < xLength; x++) {
            for (int z = 0; z < zLength; z++) {
                world[x, z] = new WorldSpace(WorldSpace.Type.wall);
            }
        }

        // Generate Maze
        var maze = MazeGenerator.Generate(mazeXLength, mazeZLength, courtyardSize);

        // Add Maze to the World
        ApplyMazeToWorld(world, maze);

        // Add Finish Zone
        AddFinishZone(world, rand.Next(mazeXLength), rand.Next(mazeZLength));
    
        return world;
    }

    private void ApplyMazeToWorld(WorldSpace[,] world, MazeCell[,] maze) {
        for (int mazeX = 0; mazeX < mazeXLength; mazeX++)
        {
            for (int mazeZ = 0; mazeZ < mazeZLength; mazeZ++)
            {
                var cell = maze[mazeX, mazeZ];
                int worldX = mazeX * 2 + 1;
                int worldZ = mazeZ * 2 + 1;

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

    public void AddFinishZone(WorldSpace[,] world, int mazeX, int mazeZ) {
        int worldX = mazeX * 2 + 1;
        int worldZ = mazeZ * 2 + 1;

        world[worldX, worldZ].type = WorldSpace.Type.finish;
    }

    public int getXLength() {
        return xLength;
    }

    public int getZLength() {
        return zLength;
    }
}