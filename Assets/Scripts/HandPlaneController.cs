using System;
using System.Collections;
using System.Collections.Generic;
using Scripts;
using UnityEngine;

public class HandPlaneController : MonoBehaviour {
    public Transform transform;
    public GridManager gridManager;
    public float planeHeight;
    private List<GameObject> enabledPieces;

    // Start is called before the first frame update
    private void Start() {
        transform.localScale = new Vector3(0.1f * GridManager.PlaneWidth, planeHeight, 0.1f * GridManager.PlaneHeight);
    }


    private void OnTriggerEnter(Collider other) {
        var root = GetRoot(other.gameObject);
        // root.transform.rotation = transform.rotation;
        var rigidBody = root.GetComponentInChildren<Rigidbody>();
        rigidBody.velocity = Vector3.zero;
        Snap(root);
    }


    private void Snap(GameObject root) {
        foreach (Transform child in root.transform) {
            child.SetParent(transform);
            var position = GetGridPosition(child.position);
            child.position = position;
        }
    }

    private Vector3 GetGridPosition(Vector3 position) {
        position -= transform.position;
        position.x = Mathf.Round(position.x / GridManager.PlaneWidth);
        position.z = Mathf.Round(position.x / GridManager.PlaneHeight);
        return position;
    }

    // Update is called once per frame
    void Update() {
        foreach (GameObject piece in enabledPieces) {
            var input = new List<(Vector3,Vector3)>();
            
            // Find distance from piece to every empty cell
            List<(Vector3, float)> cellMagnitudes = new List<(Vector3, float)>();
            foreach ((Vector3 cellGridPos,Vector3 cellWorldPos) in input) {
                cellMagnitudes.Add((cellGridPos,Vector3.Distance(piece.transform.position,cellWorldPos)));
            }
        }
    }

    private static GameObject GetRoot(GameObject gameObject) {
        return gameObject.transform.root.gameObject;
    }

    public void AddPieceToActive(GameObject piece) {
        enabledPieces.Add(piece);
    }

    public void RemovePieceFromActive(GameObject piece) {
        enabledPieces.Remove(piece);
    }

}