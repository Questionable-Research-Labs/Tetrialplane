using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnappingEnabler : MonoBehaviour {

    public HandPlaneController sappingController;
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Piece")) {
            sappingController.AddPieceToActive(other.gameObject);
        }
    }
    
    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Piece")) {
            sappingController.RemovePieceFromActive(other.gameObject);
        }
    }
}