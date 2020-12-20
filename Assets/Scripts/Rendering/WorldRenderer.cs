using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WorldRenderer : MonoBehaviour
{
    private static readonly System.Random rand = new System.Random();

    // Prefabs
    public Transform wallPrefab = null;
    public Transform brickFloorPrefab = null;
    public Transform grassFloorPrefab = null;
    
    // Props
    public Transform[] rarePropPrefabs = null;
    public Transform[] grassPropPrefabs = null;
    public Transform[] bramblePropPrefabs = null;
    [Range(0, 50)]
    public int rarePropChance = 10;

    // Size of Maze Array
    [Range(10, 100)]
    public int mazeXLength = 50;
    [Range(10, 100)]
    public int mazeZLength = 50;
    public bool addLoops = true;

    // Amplify the x,z values by this much
    [Range(1, 20)]
    public int size = 1;

    public AIController aIController;
    private WorldGenerator generator;
    
    void Start()
    {
        // Generate World
        generator = new WorldGenerator(mazeXLength, mazeZLength, addLoops);
        var world = generator.GenerateWorld();

        // Render World
        Draw(world);

        // Bake NavMesh (Requires NaveMeshComponents package)
        NavMeshSurface nm = GameObject.FindObjectOfType<NavMeshSurface>();
        nm.BuildNavMesh();

        // Spawn AIs
        int unityXSize = generator.getXLength() * size; 
        int unityZSize = generator.getZLength() * size;
        aIController.SetSpawnArea(unityXSize, unityZSize);
    }

    private void Draw(WorldSpace[,] world)
    {
        for (int worldX = 0; worldX < generator.getXLength(); worldX++)
        {
            for (int worldZ = 0; worldZ < generator.getZLength(); worldZ++)
            {
                WorldSpace space = world[worldX, worldZ];
                if (space == null) {
                    Debug.Log(String.Format("Null WorldSpace at {0},{1}", worldX, worldZ));
                    continue;
                }

                // Convert from data coordinates to unity coordinates
                int unityX = worldX * size;
                int unityZ = worldZ * size;

                if (space.type == WorldSpace.Type.floor) {
                    if (space.biome == WorldSpace.Biome.brick) {
                        DrawSpaceAndScale(unityX, 0, unityZ, brickFloorPrefab);

                        // Place rare prop on bricks
                        if (rand.Next(rarePropChance) == 0) {
                            PlaceRandomProp(unityX, unityZ, rarePropPrefabs);
                        }
                    } else if (space.biome == WorldSpace.Biome.grass) {
                        DrawSpaceAndScale(unityX, 0, unityZ, grassFloorPrefab);

                        // Place grass props
                        FillWithRandomProp(unityX, unityZ, size, grassPropPrefabs);
                    } else if (space.biome == WorldSpace.Biome.brambles) {
                        DrawSpaceAndScale(unityX, 0, unityZ, grassFloorPrefab);

                        // Place bramble props
                        FillWithRandomProp(unityX, unityZ, size, bramblePropPrefabs);
                    }
                } else if (space.type == WorldSpace.Type.wall) {
                    DrawSpaceAndScale(unityX, 5, unityZ, wallPrefab);
                }
            }
        }

    }

    private void PlaceRandomProp(int unityX, int unityZ, Transform[] propPrefabs) {
        if (propPrefabs != null && propPrefabs.Length > 0) {
            // Pick random prop
            var prop = propPrefabs[rand.Next(propPrefabs.Length)];

            // Pick random place inside plane
            int radius = size / 2;
            int propX = rand.Next(unityX - radius, unityX + radius);
            int propZ = rand.Next(unityZ - radius, unityZ + radius);

            // Draw
            DrawObj(propX, 0, propZ, prop);
        }
    }

    private void FillWithRandomProp(int unityX, int unityZ, int size, Transform[] propPrefabs) {
        int radius = size / 2;

        for (int propX = unityX - radius; propX < unityX + radius; propX++) {
            for (int propZ = unityZ - radius; propZ < unityZ + radius; propZ++) {
                // Pick random prop
                var prop = propPrefabs[rand.Next(propPrefabs.Length)];

                // Draw
                DrawObj(propX, 0, propZ, prop);
            }
        }
    }

    private Transform DrawObj(int unityX, int yOffset, int unityZ, Transform prefab) {
        // Create new instance of prefab
        var obj = Instantiate(prefab, transform) as Transform;

        // Move to the given position, (in unity coordinates)
        obj.position = new Vector3(unityX, yOffset, unityZ);

        return obj;
    }

    private void DrawSpaceAndScale(int unityX, int yOffset, int unityZ, Transform prefab) {
        // Draw Prefab
         var obj = DrawObj(unityX, yOffset, unityZ, prefab);

        // Scale the object to fill the spot
        // Multiply through by size because planes need their scale set to 0.1 instead of 1
        obj.localScale = new Vector3(obj.localScale.x * size, obj.localScale.y, obj.localScale.z * size);
    }
}
