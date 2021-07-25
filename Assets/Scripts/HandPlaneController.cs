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
    public float snapDistThreshold = 0.6f;
    public List<GameObject> removablePeices = new List<GameObject>();


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
        ComputeSnapping();
        Debug.Log($"Checking if {removablePeices} has peices");
        foreach (var piece in removablePeices) {
            if (piece != null) {
                Debug.Log($"Removing Peice {piece.name}");
                RemovePieceFromActive(piece);
                Destroy(piece);
                Debug.Log("Removed peice");
            }
        }
    }

    void ComputeSnapping() {
        foreach (GameObject piece in enabledPieces) {
            if (piece == null) {
                continue;
            }
            
            List<((int, int, int),Vector3)> peaks = gridManager.GetPeaks();
            
            // (blockTransform,chosenCell,dist)
            List<(Transform, (int,int,int), float)> bestBlocksInPiece = new List<(Transform, (int,int,int), float)>();
            
            foreach (Transform blockTransform in piece.transform) {
                // Find distance from piece to every Peak
                List<((int,int,int), float)> cellMagnitudes = new List<((int,int,int), float)>();
                foreach (((int cellZ, int cellY, int cellX),Vector3 cellWorldPos) in peaks) {
                    // Vector3 topOfPiece = piece.transform.position + piece.transform.TransformPoint(piece.transform.up*0.5f);
                    float dist = Vector3.Distance(piece.transform.position,cellWorldPos);
                    cellMagnitudes.Add(((cellX, cellY, cellZ),dist));  
                    Debug.DrawRay(piece.transform.position, cellWorldPos-piece.transform.position, Color.green);
                }
                // Order Cell peaks tops by distance magnitude
                cellMagnitudes.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            
                // Chose cell to magnet to
                bestBlocksInPiece.Add((blockTransform,cellMagnitudes[0].Item1,cellMagnitudes[0].Item2));

            }

            if (bestBlocksInPiece.Count == 0) {
                return;
            }
            
            // Sort for the best
            bestBlocksInPiece.Sort((x, y) => x.Item3.CompareTo(y.Item3));

            if (bestBlocksInPiece[0].Item3 <= snapDistThreshold) {
                // the close block
                // (blockTransform,chosenCell,dist)
                (Transform, (int,int,int), float) closeBlock = bestBlocksInPiece[0];

                // Sort for the worst
                bestBlocksInPiece.Sort((x, y) => y.Item3.CompareTo(x.Item3));
            
                // the far block
                // (blockTransform,chosenCell,dist)
                (Transform, (int,int,int), float) farBlock = bestBlocksInPiece[0];

                Vector3 rotationOfPiece =
                    Vector3.RotateTowards(closeBlock.Item1.position, farBlock.Item1.position, 6.28319f, 0.0f);

                bool closeEnoughToSnap = true;
                foreach (float rotationAxis in new float[] {rotationOfPiece.x,rotationOfPiece.y,rotationOfPiece.z}) {
                    double radianQuarterTurn = Math.PI / 2;
                    double normalizedRotation = rotationAxis % radianQuarterTurn;
                    if (normalizedRotation > radianQuarterTurn*0.25 && normalizedRotation < radianQuarterTurn*0.75) {
                        closeEnoughToSnap = false;
                    }
                }

                if (closeEnoughToSnap) {

                    var closeBlockLocalPos = closeBlock.Item1.localPosition;
                    var objs = new List<GameObject>();
                    
                    foreach (Transform block in piece.transform) {
                        block.localPosition -= closeBlockLocalPos;
                        objs.Add(block.gameObject);
                    }
                    
                    // the block snapped to
                    (int, int, int) snappingPosition = closeBlock.Item2;

                    Debug.Log($"Adding to grid");
                    List<GameObject> blocksMissed = gridManager.AddBlocksToGrid(objs, snappingPosition.Item1, snappingPosition.Item2,
                        snappingPosition.Item3);
                    Debug.Log($"Added blocks minus {blocksMissed.Count}");
                    if (blocksMissed.Count == 0) {
                        Debug.Log($"Added PEICE {piece.name}");
                        removablePeices.Add(piece);
                    }

                    // Time to snap the blocks to the new positions
                    /*List<(GameObject, (int, int, int))> snappedBlocks = new List<(GameObject, (int, int, int))>();
                    // (Grid ID, World Space)
                    List<Tuple<(int,int,int), Vector3>> emptyCells = gridManager.GetEmptySpaces();
                    foreach (Transform blockTransform in piece.transform) {
                        Tuple<(int,int,int), float> minDist = new Tuple<(int,int,int), float>((-1,-1,-1), float.MaxValue); 
                        foreach (Tuple<(int,int,int), Vector3> emptyCell in emptyCells) {
                            float dist = Vector3.Distance(emptyCell.Item2, blockTransform.transform.position);
                            if (dist < minDist.Item2) {
                                minDist = new Tuple<(int, int, int), float>(emptyCell.Item1, dist);
                            }
                        }
                        snappedBlocks.Add((blockTransform.gameObject,minDist.Item1));
                    }
                    
                    // Check that block placement is valid
                    bool oneBlockConnected = false;
                    bool allBlocksVaild = true;
                    
                    foreach ((GameObject _,(int x, int y, int z)) in snappedBlocks) {
                        switch (gridManager.ValidBlockPosition(x,y,z)) {
                            case BlockPositionValidity.Connected:
                                oneBlockConnected = true;
                                break;
                            case BlockPositionValidity.SpaceTaken:
                                allBlocksVaild = false;
                                
                                break;
                            case BlockPositionValidity.OutOfBounds:
                                // To implement Splitting, for now, don't snap
                                allBlocksVaild = false;
                                break;
                        }
                    }
                    
                    if (oneBlockConnected && allBlocksVaild) {
                        Debug.Log("Time to Insert into grid");
                        foreach ((GameObject innerBlock,(int x, int y, int z)) in snappedBlocks) {
                            gridManager.AddBlockToGrid(innerBlock,x,y,z);
                        }
                    }
                    else {
                        Debug.Log("Whelp can't do the snap because not vield");
                    }*/
                }
                else {
                    Debug.Log("NOOOOOOOOO");
                    break;
                }

                // Workout angle between close block and far block
                
            }
            else {
                Debug.Log($"FAIL YES ???? {bestBlocksInPiece[0].Item3.ToString()}, {snapDistThreshold.ToString()}");
                foreach( var x in bestBlocksInPiece) {
                    Debug.Log( x.ToString());
                }
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