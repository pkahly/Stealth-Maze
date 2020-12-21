using System.Collections;
using System.Collections.Generic;

class WorldSpace {
    public enum Type {
        floor = 0,
        wall = 1,
        finish = 2,
    }

/*
    public enum Biome {
        brick = 0,
        grass = 1,
        brambles = 2,
    }
*/

    public Type type = Type.floor;
    //public Biome biome = Biome.brick;

    public WorldSpace(Type type) {
        this.type = type;
    }
}