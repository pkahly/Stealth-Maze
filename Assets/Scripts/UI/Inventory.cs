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
    private int selectionIndex = 0;

    void Start() {
        contentPanel = GameObject.FindWithTag("InventoryContent").transform;

        panel = GameObject.Find("Panel");
        panel.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) {
            Debug.Log("Cannot find player");
            return;
        }

        playerController = player.GetComponent<FirstPersonController>();
        playerStats = player.GetComponent<PlayerStats>();
    }

    void Update() {
        if (playerStats == null) {
            return;
        }

        // Open/Close inventory
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.E)) {
            // Show/Hide panel
            panel.SetActive(!panel.activeSelf);

            // Deactivate player controls
            playerController.SetEnabled(!panel.activeSelf);

            // Construct/Destruct inventory items
            if (panel.activeSelf) {
                UpdateInventory();
            }
        }

        // Change selection
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            SelectItem(selectionIndex - 1);
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            SelectItem(selectionIndex + 1);
        }

        // Use item
        if (Input.GetKeyDown(KeyCode.Return) && inventorySlots.Count > 0) {
            playerStats.UseItem(selectionIndex);
            UpdateInventory();
        }
    }

    private void UpdateInventory() {
        // Delete old entries
        inventorySlots.Clear();
        foreach(Transform child in contentPanel) {
            Destroy(child.gameObject);
        }

        // Add new entries
        foreach (InventoryItem item in playerStats.GetInventory()) {
            AddItem(item);
        }

        // Select the first item
        selectionIndex = 0;
        SelectItem(selectionIndex);
    }

    private void AddItem(InventoryItem item) {
        Transform itemTransform = Instantiate(itemPrefab, contentPanel);
        
        InventorySlot slot;
        slot.text = itemTransform.GetChild(0).GetComponent<Text>();
        slot.item = item;

        slot.text.text = item.name;

        inventorySlots.Add(slot);
    }

    private void SelectItem(int index) {
        if (index < 0 || index >= inventorySlots.Count) {
            return;
        }

        // Deselect previous item
        ChangeSelection(selectionIndex, Color.black);

        // Select new item
        selectionIndex = index;
        ChangeSelection(selectionIndex, Color.yellow);
    }

    private void ChangeSelection(int index, Color color) {
        Outline outline = inventorySlots[index].text.GetComponent<Outline>();
        outline.effectColor = color;
    }
}
