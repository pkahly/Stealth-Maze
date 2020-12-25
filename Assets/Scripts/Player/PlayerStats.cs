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

    public void TakeDamage(int damage) {
        health = Mathf.Max(0, health - damage);
        healthText.text = "Health: " + health;

        if (health <= 0) {
            SceneManager.LoadScene("GameLose");
        }
    }
}