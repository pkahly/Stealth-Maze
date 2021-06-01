using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

public class NavigationPath : MonoBehaviour {
    public LineRenderer lineRenderer;
    private NavMeshPath path;
    private Boolean isActive;

    public void Start() {
        path = new NavMeshPath();
        isActive = false;
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.N)) {
            if (isActive) {
                // If active, stop navigating
                StopAllCoroutines();
                lineRenderer.positionCount = 0;
                isActive = false;
            } else {
                // Else, start navigating to current position
                Vector3 targetPosition = GetNavPosition(transform.position);
                Debug.Log("Navigate to: " + targetPosition);
                StartCoroutine(UpdatePath(targetPosition));
                isActive = true;
            }
        }
    }

    IEnumerator UpdatePath(Vector3 targetPosition) {
        // Update the way to the goal every second.
        while (true) {
            if (!NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path)) {
                throw new ArgumentException("Failed to get path from " + transform.position + " to " + targetPosition);
            }

            lineRenderer.positionCount = path.corners.Length;
            lineRenderer.SetPositions(path.corners);
            yield return new WaitForSeconds(1);
        }
    }

    private Vector3 GetNavPosition(Vector3 center) {
        // Get Nearest Point on NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(center, out hit, 20.0f, NavMesh.AllAreas)) {
            return hit.position;
        }
            
        throw new ArgumentException("Failed to Get NavMesh Point");
    }
}