using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class Inventory : MonoBehaviour {
    public Transform itemPrefab;

    private GameObject panel;
    private FirstPersonController player;
    private Transform contentPanel;

    void Start() {
        contentPanel = GameObject.FindWithTag("InventoryContent").transform;
        Instantiate(itemPrefab, contentPanel);
        Instantiate(itemPrefab, contentPanel);
        Instantiate(itemPrefab, contentPanel);
        Instantiate(itemPrefab, contentPanel);

        panel = GameObject.Find("Panel");
        panel.SetActive(false);

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.E)) {
            // Show/Hide panel
            panel.SetActive(!panel.activeSelf);

            // Deactivate player controls
            player.SetEnabled(!panel.activeSelf);
        }
    }
}
