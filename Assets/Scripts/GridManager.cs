using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts {
    public class GridManager : MonoBehaviour {
        /** <summary>
         * The width of each plane
         * </summary>
         */
        public int planeWidth = 10;

        /** <summary>
         * The height of each plane
         * </summary>
         */
        public int planeHeight = 10;

        /** <summary>
         * The object used to parent the game object
         * </summary>
         */
        public Transform gridPlane;

        /** <summary>
         * The planes on the players hand
         * Indexed by [z][y][x]
         * Each tile is stored in rows, each row is stored in a plane 
         * </summary>
         */
        private List<GameObject[][]> _grid;

        /** <summary>
         * Adds a block to the grid
         * </summary>
         * <param name="blocks">
         * The tetrimeno to be added to the grid, positions are whole numbers,
         * with the origin being the block that collided
         * </param>
         * <param name="x">The X position, in terms of the grid, where the origin block collided</param>
         * <param name="y">The Y position, in terms of the grid, where the origin block collided</param>
         * <param name="z">The Z position, in terms of the grid, where the origin block collided</param>
         */
        public IEnumerator<GameObject> AddBlocksToGrid(GameObject[] blocks, int x, int y, int z) {
            foreach (var block in blocks) {
                var blockPosition = block.transform.position;
                var blockZ = (int) Math.Floor(blockPosition.z) + z;

                if (blockZ >= _grid.Count) {
                    for (var i = 0; i < blockZ + 1 - _grid.Count; i++) {
                        AddPlane();
                    }
                }

                if (x + 1 > planeWidth || y + 1 > planeHeight) {
                    yield return block;
                    continue;
                }

                block.transform.SetParent(gridPlane);

                _grid[blockZ][y + (int) blockPosition.y][x + (int) blockPosition.x] = block;
            }
        }

        /** <summary>
         * Removes any full planes, and updates the grid GameObject positions
         * </summary>
         * <returns></returns>
         */
        public IEnumerator UpdateGrid() {
            // Create a temporary grid to store the updated grid
            var tempGrid = new List<GameObject[][]>();

            // Loop through all planes
            foreach (var plane in _grid) {
                // Variable to store if an empty space was found
                var foundEmptyTile = false;

                // Loop through all the rows
                foreach (var row in plane) {
                    // Loop through all the tiles
                    foreach (var tile in row) {
                        // Check to see if the space is empty, if true, break from the loop
                        if (tile == null) {
                            foundEmptyTile = true;
                            break;
                        }
                    }

                    // If there is an empty tile in the current plane, break out of the loop
                    if (foundEmptyTile) {
                        break;
                    }
                }

                // If an empty tile was found, add it to the temp grid
                if (foundEmptyTile) {
                    tempGrid.Add(plane);
                }
                // Otherwise remove it from the grid, and destroy all the game objects
                else {
                    foreach (var row in plane) {
                        foreach (var tile in row) {
                            Destroy(tile);
                        }
                    }
                }
            }

            _grid = tempGrid;

            // Loop through all the planes
            for (int z = 0; z < _grid.Count; z++) {
                // Get the current plane
                var plane = _grid[z];

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

        /** <summary>
         * Creates an empty plane
         * </summary>
         */
        private void AddPlane() {
            var columns = new GameObject[planeHeight][];

            for (int i = 0; i < planeHeight; i++) {
                columns[i] = new GameObject[planeWidth];
            }
        }
    }
}