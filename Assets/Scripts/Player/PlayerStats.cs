using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

class PlayerStats: MonoBehaviour {
    // UI Overlay
    [SerializeField] private Text visibilityText;
    [SerializeField] private Text healthText;

    // Visibility
    private LayerMask lightCoverMask;
    private LayerMask heavyCoverMask;
    private int visibilityLevel;
    private DayNightCycle dayNightScript;

    // Health
    private int health = 100;

    public void Start() {
        lightCoverMask = LayerMask.GetMask("Cover");
        heavyCoverMask = LayerMask.GetMask("HeavyCover");

        dayNightScript = FindObjectOfType<DayNightCycle>();

        StartCoroutine(DetectVisibility());
        StartCoroutine(RefillHealth());
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
            health = Mathf.Min(100, health + 5);
            healthText.text = "Health: " + health;

            yield return new WaitForSeconds(30);
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
}