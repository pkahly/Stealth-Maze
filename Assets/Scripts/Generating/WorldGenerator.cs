using System.Collections;
using System.Collections.Generic;


class WorldGenerator {
    // Size of Maze Array
    private int mazeXLength;
    private int mazeZLength;

    // Size of world (mazeSize * 2 + 1)
    private int xLength;
    private int zLength;

    public WorldGenerator(int mazeXLength, int mazeZLength) {
        this.mazeXLength = mazeXLength;
        this.mazeZLength = mazeZLength;

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

                // Set cell's coordinates to ground type
                world[worldX, worldZ].type = WorldSpace.Type.ground;

                // Knock down walls
                if (!cell.HasFlag(WallState.UP))
                {
                    int wallX = worldX;
                    int wallZ = worldZ + 1;
                    world[wallX, wallZ].type = WorldSpace.Type.ground;
                }

                if (!cell.HasFlag(WallState.LEFT))
                {
                    int wallX = worldX - 1;
                    int wallZ = worldZ;
                    world[wallX, wallZ].type = WorldSpace.Type.ground;
                }

                // Only the last column needs to change the right walls
                if (mazeX == mazeXLength - 1)
                {
                    if (!cell.HasFlag(WallState.RIGHT))
                    {
                        int wallX = worldX + 1;
                        int wallZ = worldZ;
                        world[wallX, wallZ].type = WorldSpace.Type.ground;
                    }
                }

                // Only the first row needs to change the down walls
                if (mazeZ == 0)
                {
                    if (!cell.HasFlag(WallState.DOWN))
                    {
                        int wallX = worldX;
                        int wallZ = worldZ - 1;
                        world[wallX, wallZ].type = WorldSpace.Type.ground;
                    }
                }
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