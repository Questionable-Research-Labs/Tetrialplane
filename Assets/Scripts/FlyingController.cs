using System;
using System.Collections;
using System.Collections.Generic;
using Scripts;
//begone thot
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class FlyingController : MonoBehaviour {
    // An array of available prefabs for pieces
    public GameObject[] pieces;

    // The game area the pieces must stay inside
    public BoxCollider gameArea;

    // The minimum velocity to apply to spawned pieces
    public Vector3 minVelocity;

    // The maximum velocity to apply to spawned pieces
    public Vector3 maxVelocity;

    // The scale to give each spawned piece
    public Vector3 pieceScale;

    private int maxSpawning = 50;
    private int currentSpawned = 0;


    // Start is called before the first frame update
    private void Start() {
        StartCoroutine(SlowlyCreatePieceObjects());
    }

    private IEnumerator SlowlyCreatePieceObjects() {
        while (true) {
            if (currentSpawned < maxSpawning) {
                CreatePieceObject();
            }

            yield return new WaitForSeconds(Random.Range(0f, 1f));
        }
    }

    // Update is called once per frame
    private void Update() {
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Piece")) return;
        GameObjectLeave(other.gameObject);
    }

    private void GameObjectLeave(Object root) {
        currentSpawned--;
        Destroy(root);
        if (root == null) return;
        // TODO: LOOSE POINT OUT OF REGION
    }

    private void CreatePieceObject() {
        currentSpawned++;
        var random = Random.Range(0, pieces.Length - 1);
        var originalObject = pieces[random];

        var bounds = gameArea.bounds;
        var boundsMin = bounds.min;
        var boundsMax = bounds.max;

        var position = new Vector3(
            Random.Range(boundsMin.x, boundsMax.x),
            Random.Range(boundsMin.y, boundsMax.y),
            Random.Range(boundsMin.z, boundsMax.z)
        );

        var rotation = Random.rotation;


        var newObject = Instantiate(originalObject, position, rotation);
        newObject.transform.localScale = pieceScale;
        var rigidBody = newObject.GetComponent<Rigidbody>();

        var velocity = new Vector3(
            Random.Range(minVelocity.x, maxVelocity.x),
            Random.Range(minVelocity.y, maxVelocity.y),
            Random.Range(minVelocity.z, maxVelocity.z)
        );

        rigidBody.velocity = velocity;
    }
}