using System;
using System.Collections;
using System.Collections.Generic;
using Scripts;
using TMPro;
using UnityEngine;

public class HandPlaneController : MonoBehaviour {
    public Transform transform;
    public GridManager gridManager;
    public float planeHeight;
    private List<GameObject> enabledPieces = new List<GameObject>();
    public ScoreManager scoreManager;

    // Start is called before the first frame update
    private void Start() {
    }


    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Piece")) return;
        scoreManager.IncreaseScore();
        Destroy(other.gameObject);
    }


    public void AddPieceToActive(GameObject piece) {
        enabledPieces.Add(piece);
    }

    public void RemovePieceFromActive(GameObject piece) {
        enabledPieces.Remove(piece);
    }

}