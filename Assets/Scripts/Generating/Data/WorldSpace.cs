using System.Collections;
using System.Collections.Generic;

class WorldSpace {
    public enum Type {
        ground = 0,
        wall = 1,
    }

    public Type type = Type.ground;

    public WorldSpace(Type type) {
        this.type = type;
    }
}