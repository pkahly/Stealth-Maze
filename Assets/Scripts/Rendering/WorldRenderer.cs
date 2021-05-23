using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FirstPersonController = UnityStandardAssets.Characters.FirstPerson.FirstPersonController;

public class WorldRenderer : MonoBehaviour
{
    private static readonly System.Random rand = new System.Random();

    // Prefabs
    public Transform wallPrefab = null;
    public Transform brickFloorPrefab = null;
    public Transform finishPrefab = null;
    public Transform groundPrefab = null;
    public Transform wildernessPrefab = null;
    
    // Props
    public Transform[] rarePropPrefabs = null;
    public Transform[] grassPropPrefabs = null;
    public Transform[] itemPrefabs = null;

    public AIController aIController;
    public FirstPersonController player;
    private WorldGenerator generator;
    
    void Start() {
        // Get Configuration options
        Config config = Config.GetInstance();

        // Generate World
        generator = new WorldGenerator(config.totalMazesXLength, config.totalMazesZLength);
        var world = generator.GenerateWorld(config.mazeSpecs);

        // Render World
        Draw(world, config);

        // Generate Wilderness area around the edges
        int unityXSize = generator.getXLength() * config.size; 
        int unityZSize = generator.getZLength() * config.size;
        int unityWildernessWidth = config.wildernessWidth * config.size;
        if (unityWildernessWidth > 0) {
            DrawWilderness(unityXSize, unityZSize, unityWildernessWidth, config);
        }

        // Bake NavMesh (Requires NaveMeshComponents package)
        NavMeshSurface nm = GameObject.FindObjectOfType<NavMeshSurface>();
        nm.BuildNavMesh();

        // Spawn AIs
        aIController.SetSpawnArea(unityXSize, unityZSize);
        aIController.SetTotalArea(-unityWildernessWidth, -unityWildernessWidth, unityXSize + unityWildernessWidth, unityZSize + unityWildernessWidth);

        // Set player spawn point
        Vector3 spawnPos = generator.GetRandomPosition(config.mazeSpecs[0]);
        player.SetSpawnPoint(spawnPos.x * config.size, spawnPos.z * config.size);

        // Launch Item Spawner
        ItemSpawner spawner = gameObject.AddComponent<ItemSpawner>();
        spawner.Run(itemPrefabs, config.mazeSpecs, config.size);
    }

    private void Draw(WorldSpace[,] world, Config config) {
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
                int unityX = worldX * config.size;
                int unityZ = worldZ * config.size;

                if (space.type == WorldSpace.Type.floor) {
                    DrawSpaceAndScale(unityX, 0, unityZ, config.size, brickFloorPrefab);

                    // Randomly place grass prop
                    if (rand.Next(config.grassPropChance) == 0) {
                        PlaceRandomProp(unityX, unityZ, config.size, grassPropPrefabs);
                    }

                    // Randomly place rare prop
                    if (rand.Next(config.rarePropChance) == 0) {
                        PlaceRandomProp(unityX, unityZ, config.size, rarePropPrefabs);
                    }

                } else if (space.type == WorldSpace.Type.ground) {
                    DrawSpaceAndScale(unityX, 0, unityZ, config.size, groundPrefab);

                    // Randomly place grass prop
                    if (rand.Next(config.grassPropChance) == 0) {
                        PlaceRandomProp(unityX, unityZ, config.size, grassPropPrefabs);
                    }
                } else if (space.type == WorldSpace.Type.wall) {
                    DrawSpaceAndScale(unityX, 5, unityZ, config.size, wallPrefab);
                } else if (space.type == WorldSpace.Type.finish) {
                    DrawSpaceAndScale(unityX, 0, unityZ, config.size, finishPrefab);
                }
            }
        }

    }

    private void DrawWilderness(int unityXSize, int unityZSize, int wildernessWidth, Config config) {
        float offset = config.size / 2.0f;
        float topX = wildernessWidth + unityXSize - offset;
        float topZ = wildernessWidth + unityZSize - offset;
        float bottomX = -(wildernessWidth + offset);
        float bottomZ = -(wildernessWidth + offset);

        // Bottom
        DrawPlane(new Vector3(-offset, 0, -offset), new Vector3(topX, 0, bottomZ), wildernessPrefab);

        // Top
        DrawPlane(new Vector3(-offset, 0, unityZSize - offset), new Vector3(topX, 0, topZ), wildernessPrefab);

        // Left
        DrawPlane(new Vector3(bottomX, 0, bottomZ), new Vector3(-offset, 0, topZ), wildernessPrefab);

        // Left
        DrawPlane(new Vector3(unityXSize - offset, 0, -offset), new Vector3(topX, 0, unityZSize), wildernessPrefab);

        // Randomly place grass props
        for (float x = bottomX + offset; x < topX; x += offset) {
            for (float z = bottomZ + offset; z < topZ; z += offset) {
                // Skip interior
                if (x >= 0 && x <= unityXSize && z >= 0 && z <= unityZSize) {
                    continue;
                }

                // 50% chance to place grass prop
                if (rand.Next(1) == 0) {
                    PlaceRandomProp(x, z, config.size, grassPropPrefabs);
                }
            }
        }
    }

    private void DrawPlane(Vector3 startPoint, Vector3 endPoint, Transform prefab) {
        Vector3 between = endPoint - startPoint;
        float xDistance = between.x;
        float zDistance = between.z;

        var obj = DrawObj(startPoint.x, 0, startPoint.z, prefab);
        obj.localScale = new Vector3(obj.localScale.x * xDistance, obj.localScale.z, obj.localScale.z * zDistance);
        obj.position = startPoint + (between * 0.5f);
    }

    private void PlaceRandomProp(float unityX, float unityZ, int size, Transform[] propPrefabs) {
        if (propPrefabs != null && propPrefabs.Length > 0) {
            // Pick random prop
            var prop = propPrefabs[rand.Next(propPrefabs.Length)];

            // Pick random place inside plane
            int radius = size / 2;
            int propX = rand.Next((int)unityX - radius, (int)unityX + radius);
            int propZ = rand.Next((int)unityZ - radius, (int)unityZ + radius);

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

    private Transform DrawObj(float unityX, int yOffset, float unityZ, Transform prefab) {
        // Create new instance of prefab
        var obj = Instantiate(prefab, transform) as Transform;

        // Move to the given position, (in unity coordinates)
        obj.position = new Vector3(unityX, yOffset, unityZ);

        return obj;
    }

    private void DrawSpaceAndScale(int unityX, int yOffset, int unityZ, int size, Transform prefab) {
        // Draw Prefab
         var obj = DrawObj(unityX, yOffset, unityZ, prefab);

        // Scale the object to fill the spot
        // Multiply through by size because planes need their scale set to 0.1 instead of 1
        obj.localScale = new Vector3(obj.localScale.x * size, obj.localScale.y, obj.localScale.z * size);
    }
}
