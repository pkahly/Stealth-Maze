using System;
using UnityEngine;
using UnityEngine.SceneManagement;

class UI: MonoBehaviour {
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            SceneManager.LoadScene("Maze");
        }
    }
}