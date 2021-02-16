using System;
using System.Collections;
using System.Collections.Generic;

public class Config {
    private static Config instance;

    // Maze Options
    public List<MazeSpec> mazeSpecs;
    public int totalMazesXLength;
    public int totalMazesZLength;
    public int wildernessWidth;

    // AI Options
    public int numAIs;

    private Config() {
        mazeSpecs = new List<MazeSpec>();
        
        mazeSpecs.Add(new MazeSpec(1, 1, 5, 5, 0, 1));
        mazeSpecs.Add(new MazeSpec(8, 1, 5, 5, 1, 1));
        mazeSpecs.Add(new MazeSpec(1, 8, 5, 5, 1, 1));
        mazeSpecs.Add(new MazeSpec(8, 8, 5, 5, 0, 1));

        totalMazesXLength = 14;
        totalMazesZLength = 14;

        wildernessWidth = 3;

        /*
        mazeSpecs.Add(new MazeSpec(0, 0, 30, 30, 0, 0));

        totalMazesXLength = 30;
        totalMazesZLength = 30;

        wildernessWidth = 0;
        */

        numAIs = 10;
    }

    public static Config GetInstance() {
        if (instance == null) {
            instance = new Config();
        }

        return instance;
    }
}