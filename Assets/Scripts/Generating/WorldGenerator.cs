using System.Collections;
using System.Collections.Generic;


class WorldGenerator {
    public int xLength = 20;
    public int zLength = 20;

    public WorldSpace[,] GenerateWorld() {
        WorldSpace[,] world = new WorldSpace[xLength, zLength];

        for (int x = 0; x < xLength; x++) {
            for (int z = 0; z < zLength; z++) {
                // Testing
                if ((x+z) % 8 == 0) {
                    world[x, z] = new WorldSpace(WorldSpace.Type.wall);
                } else {
                    world[x, z] = new WorldSpace(WorldSpace.Type.ground);
                }
            }
        }
    
        return world;
    }
}