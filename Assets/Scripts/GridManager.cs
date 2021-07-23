using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts {
    class GridManager : MonoBehaviour {
        public List<GameObject[][]> Grid;
        public int planeWidth = 10, planeHeight = 10;

        public IEnumerator UpdateGrid() {
            var tempGrid = new List<GameObject[][]>();

            foreach (var plane in Grid) {
                var foundEmptyTile = false;

                foreach (var row in plane) {
                    foreach (var tile in row) {
                        if (tile != null) {
                            foundEmptyTile = true;
                            break;
                        }
                    }

                    if (foundEmptyTile) {
                        break;
                    }
                }

                if (foundEmptyTile) {
                    tempGrid.Add(plane);
                }
                else {
                    foreach (var row in plane) {
                        foreach (var tile in row) {
                            Destroy(tile);
                        }
                    }
                }
            }

            Grid = tempGrid;
            
            // Loop through all the planes
            for (int z = 0; z < Grid.Count; z++) {
                // Get the current plane
                var plane = Grid[z];
                
                // Iterate through the rows
                for (int y = 0; y < plane.Length; y++) {
                    // Get the current row
                    var row = plane[y];
                    
                    // Iterate through all the tiles
                    for (int x = 0; x < row.Length; x++) {
                        // Get the current tile
                        var tile = row[x];
                        // Set the position of the object
                        var tileTransform = tile.transform;
                        tileTransform.position = new Vector3(x, y, z);
                    }
                }
            }
            
            yield return null;
        }
    }
}