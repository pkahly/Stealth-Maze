using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour {
    public Transform player;
    public Transform aiPrefab;
    public float turnSpeed = 180;
    public float attackDistance = 10;
    public AudioSource alarmSound;
    public AudioSource attackSound;

    private static System.Random rand = new System.Random();

    private Config config;
    private List<AIData> aiDataList;
    private int spawnXSize;
    private int spawnZSize;
    private int startX;
    private int startZ;
    private int totalXSize;
    private int totalZSize;
    private Vector3 reservePoint;
    private int patrolPathSize = 6;
    private PlayerStats playerStats;
    private LayerMask obstacleMask;
    private LayerMask heavyCoverMask;

    private Dictionary<int, float> visibilityLevelToViewDistanceMap = new Dictionary<int, float>()
    {
        [5] = 100,
        [4] = 60,
        [3] = 20,
        [2] = 10,
        [1] = 5,
        [0] = 2,
    };

    public void SetSpawnArea(int spawnXSize, int spawnZSize) {     
        this.spawnXSize = spawnXSize;
        this.spawnZSize = spawnZSize;
    }

    public void SetTotalArea(int startX, int startZ, int totalXSize, int totalZSize) {     
        this.startX = startX;
        this.startZ = startZ;
        this.totalXSize = totalXSize;
        this.totalZSize = totalZSize;
    }

    public void Start() {
        if (spawnXSize <= 0 || spawnZSize <= 0) {
            throw new ArgumentException("Call SetSpawnArea First");
        }
        if (totalXSize <= 0 || totalZSize <= 0) {
            throw new ArgumentException("Call SetTotalArea First");
        }

        obstacleMask = LayerMask.GetMask("Obstacle");
        heavyCoverMask = LayerMask.GetMask("HeavyCover");
        config = Config.GetInstance();

        // Pick a spot for the reserve units to gather
        reservePoint = GetNavPosition(new Vector3(rand.Next(spawnXSize), 0, rand.Next(spawnZSize)));

        // Create AI's
        aiDataList = new List<AIData>();

        for (int i = 0; i < config.numPatrolAIs; i++) {
            aiDataList.Add(CreateAI(AIType.PATROL));
        }

        for (int i = 0; i < config.numReserveAIs; i++) {
            aiDataList.Add(CreateAI(AIType.RESERVE));
        }

        // Get reference to Player's stats
        playerStats = player.GetComponent<PlayerStats>();

        // Start AI Movement
        StartCoroutine(Patrol());
    }

    private AIData CreateAI(AIType type) {
        AIData aiData = new AIData();
        
        Vector3 startPoint;
        if (type == AIType.PATROL) {
            // Create Patrol Path
            Vector3[] patrolPath = GetPatrolPath();
            aiData.patrolIndex = 0;
            aiData.patrolPath = patrolPath;

            // Spawn at first patrol point
            startPoint = patrolPath[0];
        } else if (type == AIType.RESERVE) {
            startPoint = RandomPointNear(reservePoint, 10);
        } else {
            throw new ArgumentException("Unrecognized type: " + type);
        }

        // Create new instance of prefab at startPoint
        var AI = Instantiate(aiPrefab, startPoint, transform.rotation, transform) as Transform;

        aiData.transform = AI;
        aiData.spotLight = AI.Find("Spotlight").GetComponent<Light>();
        aiData.agent = AI.GetComponent<NavMeshAgent>();
        aiData.type = type;
        aiData.startPoint = startPoint;

        return aiData;
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
            // Generate random patrol point, with retries
            for (int retries = 0; retries < 20; retries++) {
                try {
                    patrolPath[i] = GetNavPosition(new Vector3(rand.Next(spawnXSize), 0, rand.Next(spawnZSize)));
                    break;
                } catch (ArgumentException ex) {
                    Debug.Log(ex);
                }
            }
        }

        return patrolPath;
    }

    Vector3 RandomPointNear(Vector3 oldPos, int distance) {
        int x = rand.Next((int)oldPos.x - distance, (int)oldPos.x + distance);
        int z = rand.Next((int)oldPos.z - distance, (int)oldPos.z + distance);

        return GetNavPosition(new Vector3(x, 0, z));
    }

    IEnumerator Patrol() {
        float waitTime = 0.1f;

        // Initial Setup
        for (int i = 0; i < aiDataList.Count; i++) {
            aiDataList[i].attackCooldown = 0;
            aiDataList[i].spotLight.color = Color.green;
        }

        alarmSound.Stop();
        yield return new WaitForSeconds(1);

        // Update path and look for player
        while (true) {
            for (int i = 0; i < aiDataList.Count; i++) {
                // Look for player
                if (CanSeePlayer(aiDataList[i].transform)) {
                    StartCoroutine(Attack(player.position));
                    yield break;
                }

                aiDataList[i].UpdateDestination();
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator Attack(Vector3 lastSeenPosition) {
        alarmSound.Play();
        float waitTime = 0.1f;

        bool canSeePlayer = true;
        float visibleTimer = 0;

        while (visibleTimer < config.timeToLosePlayer) {
            canSeePlayer = false;
            for (int i = 0; i < aiDataList.Count; i++) {
                aiDataList[i].spotLight.color = Color.red;
                aiDataList[i].attackCooldown -= waitTime;

                // Check if anyone can see the player
                if (!canSeePlayer) {
                    canSeePlayer = CanSeePlayer(aiDataList[i].transform);
                }

                // Attack if close enough
                if (Vector3.Distance(aiDataList[i].transform.position, lastSeenPosition) <= attackDistance) {
                    // Stop and face player
                    aiDataList[i].agent.SetDestination(aiDataList[i].transform.position);
                    StartCoroutine(TurnToFace(aiDataList[i].transform, lastSeenPosition));
                
                    // Attempt attack
                    if (canSeePlayer && rand.Next(1) == 0 && aiDataList[i].attackCooldown <= 0) {
                        if (!attackSound.isPlaying) {
                            attackSound.Play();
                        }

                        playerStats.TakeDamage(config.attackDamage);
                        aiDataList[i].attackCooldown = config.aiAttackCooldown;
                    }
                }
                // Otherwise follow player's actual position
                // (Prevents AI from stopping at the first obstacle)
                else {
                    aiDataList[i].agent.SetDestination(player.position);
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

        // Stop the alarm and begin hunting
        alarmSound.Stop();
        StartCoroutine(Hunt(lastSeenPosition));
    }

    IEnumerator Hunt(Vector3 lastSeenPosition) {
        float waitTime = 0.1f;
        float huntTimer = 0;

        // Search until the time limit
        while (huntTimer < config.huntingTime) {
            for (int i = 0; i < aiDataList.Count; i++) {
                aiDataList[i].spotLight.color = Color.yellow;

                // Look for player
                if (CanSeePlayer(aiDataList[i].transform)) {
                    StartCoroutine(Attack(player.position));
                    yield break;
                }

                // Pick next search point
                if (Mathf.Abs(aiDataList[i].agent.velocity.x) < 1 && Mathf.Abs(aiDataList[i].agent.velocity.z) < 1) {
                    // Random point within hunting step distance of the AI's current location
                    try {
                        Vector3 position = RandomPointNear(aiDataList[i].transform.position, config.huntingStepDistance);
                        aiDataList[i].agent.SetDestination(position);
                        break;
                    } catch(ArgumentException ex) {
                        Debug.Log(ex);
                    }
                }
            }
            
            huntTimer += waitTime;
            yield return new WaitForSeconds(waitTime);
        }

        

        StartCoroutine(Patrol());
    }

    bool CanSeePlayer(Transform ai) {
        // Get View distance from visibility level
        float viewDistance = visibilityLevelToViewDistanceMap[playerStats.GetVisibility()];
        float distanceToPlayer = Vector3.Distance(ai.position, player.position);

        // Within View Distance
        if (distanceToPlayer > viewDistance) {
            return false;
        }

        // Line of Sight not blocked by obstacles
        if (Physics.Linecast(ai.position, player.position, obstacleMask)) {
            return false;
        } 

        // Line of Sight not blocked by heavy cover
        // Treat this the same as the lowest visibility for this time of day
        float lowestViewDistance = visibilityLevelToViewDistanceMap[playerStats.GetObscuredVisibility()];
        if (Physics.Linecast(ai.position, player.position, heavyCoverMask) && distanceToPlayer > lowestViewDistance) {
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