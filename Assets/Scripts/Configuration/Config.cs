using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Config {
    private static string CONFIG_FILENAME = "Config/Config.json";

    private static Config instance;

    // Maze Options
    public List<MazeSpec> mazeSpecs;
    public int totalMazesXLength;
    public int totalMazesZLength;
    public int wildernessWidth;

    // Amplify the maze x,z values by this much
    public int size;

    // AI Options
    public int numAIs;
    public float aiAttackCooldown;
    public int attackDamage;

    // Prop Options
    public int rarePropChance;
    public int grassPropChance;

    // Day/Night Options
    public bool enableDayNightCycle;
    public bool showClock;
    public int sunrise;
    public int sunset;

    // Initialize Default Config
    private Config() {
        mazeSpecs = new List<MazeSpec>();
        mazeSpecs.Add(new MazeSpec(1, 1, 5, 5, 0, 1));
        mazeSpecs.Add(new MazeSpec(8, 1, 5, 5, 1, 1));
        mazeSpecs.Add(new MazeSpec(1, 8, 5, 5, 1, 1));
        mazeSpecs.Add(new MazeSpec(8, 8, 5, 5, 0, 1));

        totalMazesXLength = 14;
        totalMazesZLength = 14;
        wildernessWidth = 3;
        size = 10;

        numAIs = 10;
        aiAttackCooldown = 4;
        attackDamage = 10;

        rarePropChance = 15;
        grassPropChance = 5;

        enableDayNightCycle = true;
        sunrise = 8;
        sunset = 20;
    }

    private static Config ReadConfigFile() {
        StreamReader reader = new StreamReader(CONFIG_FILENAME); 
        string json = reader.ReadToEnd();
        Debug.Log(json);
        Config config = JsonUtility.FromJson<Config>(json);
        reader.Close();
        return config;
    }

    private static void WriteConfigFile(Config config) {
        Directory.CreateDirectory(Path.GetDirectoryName(CONFIG_FILENAME));

        StreamWriter writer = new StreamWriter(CONFIG_FILENAME, true);
        writer.WriteLine(JsonUtility.ToJson(config));
        writer.Close();
    }

    public static Config GetInstance() {
        if (instance == null) {
            // Try to load config file
            if (File.Exists(CONFIG_FILENAME)) {
                Debug.Log("Loading Config File From: " + CONFIG_FILENAME);
                instance = ReadConfigFile();
            } 
            // Create default config file
            else {
                Debug.Log("Creating New Config File At: " + CONFIG_FILENAME);
                instance = new Config();
                WriteConfigFile(instance);
            }
        }

        return instance;
    }
}