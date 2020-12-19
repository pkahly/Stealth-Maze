using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour {
    [Range(1,10)]
    public int numAIs = 1;
    public Transform aiPrefab;

    public void SpawnAIs(Vector3 center, int range) {     
        for (int i = 0; i < numAIs; i++) {
            // Create new instance of prefab
            var AI = Instantiate(aiPrefab, transform) as Transform;

            // Pick random point
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            randomPoint.y = 0;

            // Get Nearest Point on NavMesh
			NavMeshHit hit;
			if (NavMesh.SamplePosition(center, out hit, 5.0f, NavMesh.AllAreas)) {
				// Move to the given position, (in unity coordinates)
                AI.position = hit.position;
			} else {
                Debug.Log("Failed to find point on NavMesh");
            }           
        }
    }
}