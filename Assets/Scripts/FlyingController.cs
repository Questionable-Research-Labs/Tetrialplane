using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlyingController : MonoBehaviour
{
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

    // The current game score
    public int score = 0;

    // The amount to decrease the score by when a piece
    public int clearedScore = 100;

    // Start is called before the first frame update
    private void Start()
    {
        var i = 0;
        while (i < 2)
        {
            CreatePieceObject();
            i++;
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnTriggerExit(Collider other)
    {
        GameObjectLeave(other.gameObject);
    }

    private void GameObjectLeave(GameObject gameObject)
    {
        Destroy(gameObject);
        CreatePieceObject();
    }

    private void CreatePieceObject()
    {
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

        var newObject = Instantiate(originalObject, position, Random.rotation);
        newObject.transform.localScale = pieceScale;
        var rigidBody = newObject.GetComponentInChildren<Rigidbody>();

        var velocity = new Vector3(
            Random.Range(minVelocity.x, maxVelocity.x),
            Random.Range(minVelocity.y, maxVelocity.y),
            Random.Range(minVelocity.z, maxVelocity.z)
        );

        rigidBody.velocity = velocity;
    }
}