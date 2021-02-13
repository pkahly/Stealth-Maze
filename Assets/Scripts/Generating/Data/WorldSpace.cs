using System.Collections;
using System.Collections.Generic;

class WorldSpace {
    public enum Type {
        floor = 0,
        wall = 1,
        finish = 2,
        ground = 3,
    }

    public Type type;

    public WorldSpace(Type type) {
        this.type = type;
    }
}