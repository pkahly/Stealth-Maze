using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

struct AIData {
    public Transform transform;
    public NavMeshAgent agent;
    public int patrolIndex;
    public Vector3[] patrolPath;
}

public class AIController : MonoBehaviour {
    [Range(1,20)]
    public int numAIs = 5;
    public Transform aiPrefab;

    private static System.Random rand = new System.Random();

    private AIData[] aiData;
    private int xSize;
    private int zSize;
    private int patrolPathSize = 4;

    public void SetSpawnArea(int xSize, int zSize) {     
        aiData = new AIData[numAIs];
        this.xSize = xSize;
        this.zSize = zSize;
    }

    public void Start() {
        if (xSize <= 0 || zSize <= 0) {
            throw new ArgumentException("Call SetSpawnArea First");
        }

        for (int i = 0; i < numAIs; i++) {
            // Create Patrol Path
            Vector3[] patrolPath = GetPatrolPath();

            // Create new instance of prefab at first patrol point
            var AI = Instantiate(aiPrefab, patrolPath[0], transform.rotation) as Transform;

            // Save AI for later use
            aiData[i] = new AIData();
            aiData[i].transform = AI;
            aiData[i].agent = AI.GetComponent<NavMeshAgent>();
            aiData[i].patrolIndex = 0;
            aiData[i].patrolPath = patrolPath;
        }

        // Start AI Movement
        StartCoroutine(Patrol());
    }

    private Vector3 GetNavPosition(Vector3 center) {
        // Get Nearest Point on NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(center, out hit, 50.0f, NavMesh.AllAreas)) {
            return hit.position;
        }
            
        throw new ArgumentException("Failed to Get NavMesh Point");
    }

    Vector3[] GetPatrolPath() {
        Vector3[] patrolPath = new Vector3[patrolPathSize];
        for (int i = 0; i < patrolPathSize; i++) {
            patrolPath[i] = GetNavPosition(new Vector3(rand.Next(xSize), 0, rand.Next(zSize)));
        }

        return patrolPath;
    }

    IEnumerator Patrol() {
        while (true) {
            for (int i = 0; i < numAIs; i++) {
                AIData ai = aiData[i];

                if (Vector3.Distance(ai.transform.position, ai.patrolPath[ai.patrolIndex]) < 2) {
                    ai.patrolIndex++;
                }

                ai.agent.SetDestination(ai.patrolPath[ai.patrolIndex]);
            }

            yield return new WaitForSeconds(30);
        }
    }
}