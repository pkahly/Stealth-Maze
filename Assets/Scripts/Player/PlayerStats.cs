using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

class PlayerStats: MonoBehaviour {
    // UI Overlay
    [SerializeField] private Text visibilityText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text foodText;

    // Visibility
    private LayerMask lightCoverMask;
    private LayerMask heavyCoverMask;
    private int visibilityLevel;
    private DayNightCycle dayNightScript;

    // Stats
    private int health = 100;
    private int food = 100;

    public void Start() {
        lightCoverMask = LayerMask.GetMask("Cover");
        heavyCoverMask = LayerMask.GetMask("HeavyCover");

        dayNightScript = FindObjectOfType<DayNightCycle>();

        StartCoroutine(DetectVisibility());
        StartCoroutine(RefillHealth());
        StartCoroutine(Hunger());
    }

    /*
    +------------+--------------+-----------+-------+
    | visibility | stand/crouch | day/night | cover |
    +------------+--------------+-----------+-------+
    |          5 | stand        | day       | open  |
    |          4 | crouch       | day       | open  |
    |          4 | stand        | day       | light |
    |          3 | crouch       | day       | light |
    |          3 | stand        | day       | heavy |
    |          2 | crouch       | day       | heavy |
    |          3 | stand        | night     | open  |
    |          2 | crouch       | night     | open  |
    |          2 | stand        | night     | light |
    |          1 | crouch       | night     | light |
    |          1 | stand        | night     | heavy |
    |          0 | crouch       | night     | heavy |
    +------------+--------------+-----------+-------+
    */
    IEnumerator DetectVisibility() {
        while (true) {
            Vector3 center = new Vector3(transform.position.x, 0, transform.position.z);
            Collider[] lightCover = Physics.OverlapSphere(center, 0.1f, lightCoverMask, QueryTriggerInteraction.Collide);
            Collider[] heavyCover = Physics.OverlapSphere(center, 0.1f, heavyCoverMask, QueryTriggerInteraction.Collide);

            if (dayNightScript.IsDaytime()) {
                visibilityLevel = 5;

                if (heavyCover.Length != 0) {
                    visibilityLevel = 2;
                } else if (lightCover.Length != 0) {
                    visibilityLevel = 3;
                }
            } else {
                visibilityLevel = 3;

                if (heavyCover.Length != 0) {
                    visibilityLevel = 0;
                } else if (lightCover.Length != 0) {
                    visibilityLevel = 1;
                }
            }

            // Update UI
            visibilityText.text = "Visibility: " + visibilityLevel;

            yield return new WaitForSeconds(.2f);
        }
    }

    IEnumerator RefillHealth() {
        while(true) {
            if (food > 50) {
                // Only refill health if food is above 50
                // Reduce food by the same amount
                health = Mathf.Min(100, health + 5);
                food = Mathf.Max(0, food - 5);
            } else if (food <= 0) {
                // Starvation
                health = Mathf.Max(0, health - 5);

                if (health <= 0) {
                    SceneManager.LoadScene("GameLose");
                }
            }

            if (health <= 50) {
                healthText.color = Color.red;
            }

            healthText.text = "Health: " + health;
            foodText.text = "Food: " + food;
            yield return new WaitForSeconds(30);
        }
    }

    IEnumerator Hunger() {
        // Free 2 mins
        yield return new WaitForSeconds(180);

        while(true) {
            food = Mathf.Max(0, food - 5);
            foodText.text = "Food: " + food;

            if (food <= 50) {
                foodText.color = Color.red;
            }

            yield return new WaitForSeconds(60);
        }
    }

    public int GetVisibility() {
        return visibilityLevel;
    }

    // The visibility to use if the AI's vision is blocked by heavy cover
    public int GetObscuredVisibility() {
        if (dayNightScript.IsDaytime()) {
            return 2;
        } else {
            return 0;
        }
    }

    public void TakeDamage(int damage) {
        health = Mathf.Max(0, health - damage);
        healthText.text = "Health: " + health;

        if (health <= 0) {
            SceneManager.LoadScene("GameLose");
        }
    }

    void OnTriggerEnter(Collider hitCollider) {
        if (hitCollider.tag == "Finish") {
            SceneManager.LoadScene("GameWin");
        }
    }
}