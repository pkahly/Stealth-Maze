using System;
using UnityEngine;

[System.Serializable]
public class MazeSpec : MonoBehaviour {
    [Min(0)]
    public int mazeStartX = 0;
    [Min(0)]
    public int mazeStartZ = 0;
    
    [Range(5, 100)]
    public int mazeXLength = 50;
    [Range(5, 100)]
    public int mazeZLength = 50;
    [Range(0, 5)]
    public int courtyardSize = 4;

    [Min(1)]
    public int numExits = 1;
}