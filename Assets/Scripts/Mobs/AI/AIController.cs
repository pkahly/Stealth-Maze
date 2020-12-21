using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

struct AIData {
    public Transform transform;
    public NavMeshAgent agent;
    public Light spotLight;
    public int patrolIndex;
    public Vector3[] patrolPath;
}

public class AIController : MonoBehaviour {
    public Transform player;
    [Range(1,20)]
    public int numAIs = 5;
    public Transform aiPrefab;
    public float viewAngle = 90;
    public float turnSpeed = 90;
    public float attackDistance = 8;
    public float timeToLosePlayer = 5;
    public AudioSource alarmSound;
    public AudioSource attackSound;
    public int attackDamage = 10;

    private static System.Random rand = new System.Random();

    private AIData[] aiData;
    private int xSize;
    private int zSize;
    private int patrolPathSize = 6;
    private PlayerStats playerStats;
    private LayerMask obstacleMask;
    private LayerMask heavyCoverMask;

    private Dictionary<int, float> visibilityLevelToViewDistanceMap = new Dictionary<int, float>()
    {
        [5] = 100,
        [4] = 40,
        [3] = 20,
        [2] = 10,
        [1] = 5,
        [0] = 2,
    };

    public void SetSpawnArea(int xSize, int zSize) {     
        aiData = new AIData[numAIs];
        this.xSize = xSize;
        this.zSize = zSize;
    }

    public void Start() {
        if (xSize <= 0 || zSize <= 0) {
            throw new ArgumentException("Call SetSpawnArea First");
        }

        obstacleMask = LayerMask.GetMask("Obstacle");
        heavyCoverMask = LayerMask.GetMask("HeavyCover");

        for (int i = 0; i < numAIs; i++) {
            // Create Patrol Path
            Vector3[] patrolPath = GetPatrolPath();

            // Create new instance of prefab at first patrol point
            var AI = Instantiate(aiPrefab, patrolPath[0], transform.rotation) as Transform;

            // Save AI for later use
            aiData[i] = new AIData();
            aiData[i].transform = AI;
            aiData[i].spotLight = AI.Find("Spotlight").GetComponent<Light>();
            aiData[i].agent = AI.GetComponent<NavMeshAgent>();
            aiData[i].patrolIndex = 0;
            aiData[i].patrolPath = patrolPath;
        }

        // Get reference to Player's stats
        playerStats = player.GetComponent<PlayerStats>();

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
        // Give AI's their initial orders
        for (int i = 0; i < numAIs; i++) {
            AIData ai = aiData[i];

            ai.spotLight.color = Color.green;
            ai.agent.SetDestination(ai.patrolPath[ai.patrolIndex]);
        }
        Debug.Log("Started Patrolling");
        yield return new WaitForSeconds(1);

        // Update path and look for player
        while (true) {
            for (int i = 0; i < numAIs; i++) {
                // Look for player
                if (CanSeePlayer(aiData[i].transform)) {
                    Debug.Log("Found Player at " + player.position);
                    StartCoroutine(Attack(player.position));
                    yield break;
                }

                // Update Patrol Path
                if (Mathf.Abs(aiData[i].agent.velocity.x) < 1 && Mathf.Abs(aiData[i].agent.velocity.z) < 1) {
                    aiData[i].patrolIndex += 1;
                    if (aiData[i].patrolIndex >= aiData[i].patrolPath.Length) {
                        aiData[i].patrolIndex = 0;
                    }

                    aiData[i].agent.SetDestination(aiData[i].patrolPath[aiData[i].patrolIndex]);
                }
            }

            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator Attack(Vector3 lastSeenPosition) {
        alarmSound.Play();
        float waitTime = 0.5f;

        bool canSeePlayer = true;
        float visibleTimer = 0;

        while (visibleTimer < timeToLosePlayer) {
            canSeePlayer = false;
            for (int i = 0; i < numAIs; i++) {
                // Check if anyone can see the player
                if (!canSeePlayer) {
                    canSeePlayer = CanSeePlayer(aiData[i].transform);
                }

                // Attack if close enough
                if (Vector3.Distance(aiData[i].transform.position, player.position) <= attackDistance) {
                    aiData[i].spotLight.color = Color.red;
                    
                    // Stop and face player
                    aiData[i].agent.SetDestination(aiData[i].transform.position);
                    StartCoroutine(TurnToFace(aiData[i].transform, player.position));


                    if (!attackSound.isPlaying) {
                        attackSound.Play();
                    }
                    
                    // Attempt attack (use error margin if we don't have a visual)
                    int errorMargin = (int)Vector3.Distance(lastSeenPosition, player.position);

                    if (canSeePlayer && rand.Next(1) == 0) {
                        playerStats.TakeDamage(attackDamage);
                    } else if (rand.Next(errorMargin) == 0) {
                        playerStats.TakeDamage(attackDamage);
                    }
                }
                // Otherwise move closer
                else {
                    aiData[i].agent.SetDestination(player.position);
                }
            }

            // Will stop if we lose the player for the time limit
            if (canSeePlayer) {
                visibleTimer = 0;
                lastSeenPosition = player.position;
            } else {
                visibleTimer += waitTime;
            }

            yield return new WaitForSeconds(waitTime);
        }

        Debug.Log("Lost Player near " + lastSeenPosition);
        StartCoroutine(Patrol());
        alarmSound.Stop();
    }

    bool CanSeePlayer(Transform ai) {
        // Get View distance from visibility level
        float viewDistance = visibilityLevelToViewDistanceMap[playerStats.GetVisibility()];
        float distanceToPlayer = Vector3.Distance(ai.position, player.position);

        // Within View Distance
        if (distanceToPlayer > viewDistance) {
            return false;
        }

        // Within View Angle
        Vector3 dirToPlayer = (player.position - ai.position).normalized;
        float angleToPlayer = Vector3.Angle(ai.forward, dirToPlayer);
        if (angleToPlayer > viewAngle / 2f) {
            return false;
        }

        // Line of Sight not blocked by obstacles
        if (Physics.Linecast(ai.position, player.position, obstacleMask)) {
            return false;
        } 

        // Line of Sight not blocked by heavy cover
        // Heavy cover blocks vision unless player is within 2 units of guard
        if (Physics.Linecast(ai.position, player.position, heavyCoverMask) && distanceToPlayer > 2) {
            return false;
        }

        return true;
    }

    IEnumerator TurnToFace(Transform ai, Vector3 lookTarget) {
        Vector3 dirToLookTarget = (lookTarget - ai.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        while(Mathf.Abs(Mathf.DeltaAngle(ai.eulerAngles.y, targetAngle)) > 0.05f) {
            float angle = Mathf.MoveTowardsAngle(ai.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            ai.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }
}