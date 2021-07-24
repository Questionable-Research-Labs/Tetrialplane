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
    
    // Start is called before the first frame update
    private void Start() {
        StartCoroutine(SlowlyCreatePieceObjects());
    }

    private IEnumerator SlowlyCreatePieceObjects() {
        while (true) {
            CreatePieceObject();
            yield return new WaitForSeconds(3f);
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
        Destroy(root);
        if (root == null) return;
        CreatePieceObject();

        // TODO: LOOSE POINT OUT OF REGION
    }

    private void CreatePieceObject() {
        int random = Random.Range(0, pieces.Length - 1);
        GameObject originalObject = pieces[random];

        Bounds bounds = gameArea.bounds;
        Vector3 boundsMin = bounds.min;
        Vector3 boundsMax = bounds.max;

        /*var position = new Vector3(
            Random.Range(boundsMin.x, boundsMax.x),
            Random.Range(boundsMin.y, boundsMax.y),
            Random.Range(boundsMin.z, boundsMax.z)
        );
        */
        Vector3 position = new Vector3(0, 3, 0);
        // Quaternion rotation = Random.rotation;
        Quaternion rotation = new Quaternion(0, 0, 0, 0);


        GameObject newObject = Instantiate(originalObject, position, rotation);
        newObject.transform.localScale = pieceScale;
        Rigidbody rigidBody = newObject.GetComponentInChildren<Rigidbody>();

        // var Vector3 = new Vector3(
        //     Random.Range(minVelocity.x, maxVelocity.x),
        //     Random.Range(minVelocity.y, maxVelocity.y),
        //     Random.Range(minVelocity.z, maxVelocity.z)
        // );
        Vector3 velocity = new Vector3(0, -5, 0);

        rigidBody.velocity = velocity;
    }
}