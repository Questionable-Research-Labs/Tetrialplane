using System;
using System.Collections;
using System.Collections.Generic;
using Scripts;
using UnityEngine;

public class HandPlaneController : MonoBehaviour
{
    public Transform transform;
    public GridManager gridManager;
    public float planeHeight;

    // Start is called before the first frame update
    private void Start()
    {
        transform.localScale = new Vector3(0.1f * GridManager.PlaneWidth, planeHeight, 0.1f * GridManager.PlaneHeight);
    }


    private void OnTriggerEnter(Collider other)
    {
        var root = GetRoot(other.gameObject);
        root.transform.rotation = transform.rotation;
        var rigidBody = root.GetComponentInChildren<Rigidbody>();
        rigidBody.velocity = Vector3.zero;
        Snap(root);
    }


    private void Snap(GameObject root)
    {
        foreach (Transform child in root.transform)
        {
            child.SetParent(transform);
            var position = GetGridPosition(child.position);
            child.position = position;
        }
    }

    private Vector3 GetGridPosition(Vector3 position)
    {
        position -= transform.position;
        position.x = Mathf.Round(position.x / GridManager.PlaneWidth);
        position.z = Mathf.Round(position.x / GridManager.PlaneHeight);
        return position;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private static GameObject GetRoot(GameObject gameObject)
    {
        return gameObject.transform.root.gameObject;
    }
}