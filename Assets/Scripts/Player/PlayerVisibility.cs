using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

class PlayerVisibility: MonoBehaviour {
    [SerializeField] private LayerMask visibilityMask;
    [SerializeField] private Text visibilityText;

    public bool isVisible;

    public void Start() {
        StartCoroutine(DetectVisibility());
    }

    IEnumerator DetectVisibility() {
        while (true) {
            Vector3 center = new Vector3(transform.position.x, 0, transform.position.z);
            Collider[] intersecting = Physics.OverlapSphere(center, 0.1f, visibilityMask, QueryTriggerInteraction.Collide);

            if (intersecting.Length != 0) {
                isVisible = false;
                visibilityText.text = "Hidden";
            } else {
                isVisible = true;
                visibilityText.text = "Not Hidden";
            }

            yield return new WaitForSeconds(.2f);
        }
    }
}