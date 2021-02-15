using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class Inventory : MonoBehaviour {
    public struct InventorySlot {
        public Text text;
        public InventoryItem item;
    }

    public Transform itemPrefab;

    private GameObject panel;
    private FirstPersonController playerController;
    private PlayerStats playerStats;

    private Transform contentPanel;
    private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    void Start() {
        contentPanel = GameObject.FindWithTag("InventoryContent").transform;

        panel = GameObject.Find("Panel");
        panel.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<FirstPersonController>();
        playerStats = player.GetComponent<PlayerStats>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.E)) {
            // Show/Hide panel
            panel.SetActive(!panel.activeSelf);

            // Deactivate player controls
            playerController.SetEnabled(!panel.activeSelf);

            // If inventory was opened, build it
            if (panel.activeSelf) {
                UpdateInventory();
            } else {
                inventorySlots.Clear();

                foreach(Transform child in contentPanel) {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    private void UpdateInventory() {
        foreach (InventoryItem item in playerStats.GetInventory()) {
            AddItem(item);
        }

        // select the first item
        if (inventorySlots.Count > 0) {
            Outline outline = inventorySlots[0].text.GetComponent<Outline>();
            outline.effectColor = Color.yellow;
        }
    }

    private void AddItem(InventoryItem item) {
        Transform itemTransform = Instantiate(itemPrefab, contentPanel);
        
        InventorySlot slot;
        slot.text = itemTransform.GetChild(0).GetComponent<Text>();
        slot.item = item;

        slot.text.text = item.name;

        inventorySlots.Add(slot);
    }
}
