using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour {
    private static readonly System.Random rand = new System.Random();

    private Transform[] items;
    private int delay = 180;
    private List<MazeSpec> mazes;
    private int size;

    public void Run(Transform[] items, List<MazeSpec> mazes, int size) {
        this.items = items;
        this.mazes = mazes;
        this.size = size;

        StartCoroutine(SpawnItems());
    }

    IEnumerator SpawnItems() {
        while(true) {
            foreach (MazeSpec mazeSpec in mazes) {
                foreach (Transform item in items) {
                    float x = ConvertToUnityCoord(rand.Next(mazeSpec.mazeStartX, mazeSpec.mazeStartX + mazeSpec.mazeXLength));
                    float z = ConvertToUnityCoord(rand.Next(mazeSpec.mazeStartZ, mazeSpec.mazeStartZ + mazeSpec.mazeZLength));

                    // Create new instance of prefab
                    var obj = Instantiate(item, transform) as Transform;

                    // Move to the given position, (in unity coordinates)
                    obj.position = new Vector3(x, 1, z);
                }
            }

            yield return new WaitForSeconds(delay);
        }
    }

    public float ConvertToUnityCoord(float coord) {
        // Convert to world coords, then amplify to the unity size
        return (coord * 2 + 1) * size;
    }
}