using System;
using System.Collections;
using System.Collections.Generic;


class WorldGenerator {
    // Size of Maze Array
    private int mazeXLength;
    private int mazeZLength;
    
    private bool addLoops;
    private int numLoops = 10;

    // Size of world (mazeSize * 2 + 1)
    private int xLength;
    private int zLength;

    public WorldGenerator(int mazeXLength, int mazeZLength, bool addLoops) {
        this.mazeXLength = mazeXLength;
        this.mazeZLength = mazeZLength;
        this.addLoops = addLoops;

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
        var maze = MazeGenerator.Generate(mazeXLength, mazeZLength);

        // Add Loops
        if (addLoops) {
            AddLoops(maze);
        }

        // Add Maze to the World
        ApplyMazeToWorld(world, maze);
    
        return world;
    }

    private void ApplyMazeToWorld(WorldSpace[,] world, WallState[,] maze) {
        for (int mazeX = 0; mazeX < mazeXLength; mazeX++)
        {
            for (int mazeZ = 0; mazeZ < mazeZLength; mazeZ++)
            {
                var cell = maze[mazeX, mazeZ];
                int worldX = mazeX * 2 + 1;
                int worldZ = mazeZ * 2 + 1;

                // Count number of walls that are up
                WallState[] walls = new WallState[4] {WallState.UP, WallState.DOWN, WallState.LEFT, WallState.RIGHT};
                int numWalls = 0;
                foreach (WallState state in walls) {
                    if (cell.HasFlag(state)) {
                        numWalls++;
                    }
                }

                // Set cell's coordinates to floor type
                world[worldX, worldZ].type = WorldSpace.Type.floor;

                // Choose biome based on numWalls
                // Dead ends will be brambles, and their wall space will be grass
                WorldSpace.Biome wallBiome;
                if (numWalls == 3) {
                    world[worldX, worldZ].biome = WorldSpace.Biome.brambles;
                    wallBiome = WorldSpace.Biome.grass;
                } else {
                    world[worldX, worldZ].biome = WorldSpace.Biome.brick;
                    wallBiome = WorldSpace.Biome.brick;
                }

                

                // Knock down walls
                if (!cell.HasFlag(WallState.UP))
                {
                    int wallX = worldX;
                    int wallZ = worldZ + 1;
                    world[wallX, wallZ].type = WorldSpace.Type.floor;
                    world[wallX, wallZ].biome = wallBiome;
                }

                if (!cell.HasFlag(WallState.LEFT))
                {
                    int wallX = worldX - 1;
                    int wallZ = worldZ;
                    world[wallX, wallZ].type = WorldSpace.Type.floor;
                    world[wallX, wallZ].biome = wallBiome;
                }

                // Only the last column needs to change the right walls
                if (mazeX == mazeXLength - 1)
                {
                    if (!cell.HasFlag(WallState.RIGHT))
                    {
                        int wallX = worldX + 1;
                        int wallZ = worldZ;
                        world[wallX, wallZ].type = WorldSpace.Type.floor;
                        world[wallX, wallZ].biome = wallBiome;
                    }
                }

                // Only the first row needs to change the down walls
                if (mazeZ == 0)
                {
                    if (!cell.HasFlag(WallState.DOWN))
                    {
                        int wallX = worldX;
                        int wallZ = worldZ - 1;
                        world[wallX, wallZ].type = WorldSpace.Type.floor;
                        world[wallX, wallZ].biome = wallBiome;
                    }
                }
            }

        }
    }

    public void AddLoops(WallState[,] maze) {
        System.Random rand = new System.Random();

        for (int i = 0; i < numLoops; i++) {
            // Select an interior cell
            int x = rand.Next(1, mazeXLength- 2);
            int z = rand.Next(1, mazeZLength - 2);

            // Knock down one wall
            if (maze[x, z].HasFlag(WallState.UP)) {
                maze[x, z] &= ~WallState.UP;
            } else if (maze[x, z].HasFlag(WallState.DOWN)) {
                maze[x, z] &= ~WallState.DOWN;
            } else if (maze[x, z].HasFlag(WallState.RIGHT)) {
                maze[x, z] &= ~WallState.RIGHT;
            } else if (maze[x, z].HasFlag(WallState.LEFT)) {
                maze[x, z] &= ~WallState.LEFT;
            }

        }
    }

    public int getXLength() {
        return xLength;
    }

    public int getZLength() {
        return zLength;
    }
}