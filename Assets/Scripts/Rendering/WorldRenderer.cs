using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRenderer : MonoBehaviour
{
    // Prefabs
    public Transform wallPrefab = null;
    public Transform groundPrefab = null;

    // Amplify the x,z values by this much
    [Range(1, 20)]
    public int size = 1;

    private WorldGenerator generator;
    
    void Start()
    {
        generator = new WorldGenerator();
        var world = generator.GenerateWorld();
        Draw(world);
    }

    private void Draw(WorldSpace[,] world)
    {
        for (int worldX = 0; worldX < generator.xLength; worldX++)
        {
            for (int worldZ = 0; worldZ < generator.zLength; worldZ++)
            {
                WorldSpace space = world[worldX, worldZ];
                if (space == null) {
                    Debug.Log(String.Format("Null WorldSpace at {0},{1}", worldX, worldZ));
                    continue;
                }

                // Convert from data coordinates to unity coordinates
                int unityX = worldX * size;
                int unityZ = worldZ * size;

                // TODO: Create map from Type -> Prefab
                if (space.type == WorldSpace.Type.ground) {
                    DrawSpace(unityX, unityZ, groundPrefab);
                } else if (space.type == WorldSpace.Type.wall) {
                    DrawSpace(unityX, unityZ, wallPrefab);
                }
            }
        }

    }

    private void DrawSpace(int unityX, int unityZ, Transform prefab) {
        // Create new instance of prefab
        var obj = Instantiate(prefab, transform) as Transform;

        // Move to the given position, (in unity coordinates)
        obj.position = new Vector3(unityX, 0, unityZ);

        // Scale the object to fill the spot
        // Multiply through by size because planes need their scale set to 0.1 instead of 1
        obj.localScale = new Vector3(obj.localScale.x * size, obj.localScale.y, obj.localScale.z * size);
    }
}
