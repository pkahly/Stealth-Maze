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
    private List<InventoryItem> inventory = new List<InventoryItem>();

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

        // Initial Inventory
        inventory.Add(new FoodItem("Apple", 25));
        inventory.Add(new FoodItem("Apple", 25));
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
            UpdateOverlay();
            yield return new WaitForSeconds(.2f);
        }
    }

    IEnumerator RefillHealth() {
        while(true) {
            if (health < 100 && food > 50) {
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

            UpdateOverlay();
            yield return new WaitForSeconds(10);
        }
    }

    IEnumerator Hunger() {
        // Free 2 mins
        yield return new WaitForSeconds(180);

        while(true) {
            food = Mathf.Max(0, food - 5);
            UpdateOverlay();
            yield return new WaitForSeconds(180);
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
        UpdateOverlay();
        
        if (health <= 0) {
            SceneManager.LoadScene("GameLose");
        }
    }

    public List<InventoryItem> GetInventory() {
        return inventory;
    }

    public void UseItem(int index) {
        // Remove item
        InventoryItem item = inventory[index];
        inventory.RemoveAt(index);

        // Apply all status effects
        health = Mathf.Min(100, health + item.healthEffect);
        food = Mathf.Min(100, food + item.foodEffect);

        // Redraw overlay
        UpdateOverlay();
    }

    void UpdateOverlay() {
        healthText.text = "Health: " + health;
        foodText.text = "Food: " + food;
        visibilityText.text = "Visibility: " + visibilityLevel;

        if (health <= 50) {
            healthText.color = Color.red;
        } else {
            healthText.color = Color.white;
        }

        if (food <= 50) {
            foodText.color = Color.red;
        } else {
            foodText.color = Color.white;
        }
    }

    void OnTriggerEnter(Collider hitCollider) {
        if (hitCollider.tag == "Finish") {
            SceneManager.LoadScene("GameWin");
        } else if (hitCollider.tag == "Item") {
            // TODO - get the item parameters from the GameObject we hit
            inventory.Add(new FoodItem("Apple", 25));
            Destroy(hitCollider.gameObject);
        }
    }
}