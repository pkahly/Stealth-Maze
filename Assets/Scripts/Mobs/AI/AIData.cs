using System;
using UnityEngine;
using UnityEngine.AI;

enum AIType {
    PATROL,
    RESERVE
}

// Represents one AI agent
class AIData {
    // Unity objects
    public Transform transform;
    public NavMeshAgent agent;
    public Light spotLight;

    // Type
    public AIType type;

    // Patrol
    public int patrolIndex;
    public Vector3[] patrolPath;
    public Vector3 startPoint;

    // Attack
    public float attackCooldown;

    public void UpdateDestination() {
        if (type == AIType.PATROL) {
            // Update Patrol Path
            if (Mathf.Abs(agent.velocity.x) == 0 && Mathf.Abs(agent.velocity.z) == 0) {
                patrolIndex += 1;
                if (patrolIndex >= patrolPath.Length) {
                    patrolIndex = 0;
                }

                agent.SetDestination(patrolPath[patrolIndex]);
            }
        } else if (type == AIType.RESERVE) {
            if (Vector3.Distance(transform.position, startPoint) > 5) {
                agent.SetDestination(startPoint);
            }
        }
    }
}