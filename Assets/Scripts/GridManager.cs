using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts {
    public class GridManager : MonoBehaviour {
        /**
         * <summary>
         * The score manager
         * </summary>
         */
        public ScoreManager scoreManager;

        /** <summary>
         * The width of each plane
         * </summary>
         */
        public const int PlaneWidth = 10;

        /** <summary>
         * The height of each plane
         * </summary>
         */
        public const int PlaneHeight = 10;

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
            var blocksMissed = 0;
            foreach (var block in blocks) {
                var blockPosition = block.transform.position;
                var blockZ = (int) Math.Floor(blockPosition.z) + z;

                if (blockZ >= _grid.Count) {
                    for (var i = 0; i < blockZ + 1 - _grid.Count; i++) {
                        AddPlane();
                    }
                }

                if (x + 1 > PlaneWidth || y + 1 > PlaneHeight) {
                    yield return block;
                    blocksMissed++;
                    continue;
                }

                block.transform.SetParent(gridPlane);

                var parentTransform = _grid[blockZ][y + (int) blockPosition.y][x + (int) blockPosition.x].transform;
                
                block.transform.SetParent(parentTransform);
            }

            if (blocksMissed != 0) {
                scoreManager.PieceMissed(blocksMissed);
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

            // Create a variable to store how many planes where cleared
            var cleared = 0;

            // Loop through all planes
            foreach (var plane in _grid) {
                // Variable to store if an empty space was found
                var foundEmptyTile = false;

                // Loop through all the rows
                foreach (var row in plane) {
                    // Loop through all the tiles
                    if (row.Any(tile => tile.transform.childCount > 0)) {
                        foundEmptyTile = true;
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
                    cleared++;
                    foreach (var row in plane) {
                        foreach (var tile in row) {
                            Destroy(tile);
                        }
                    }
                }
            }

            if (cleared != 0) {
                scoreManager.PlanesCleared(cleared);
            }

            _grid = tempGrid;

            // Loop through all the planes
            for (var z = 0; z < _grid.Count; z++) {
                // Get the current plane
                var plane = _grid[z];

                // Iterate through the rows
                for (var y = 0; y < plane.Length; y++) {
                    // Get the current row
                    var row = plane[y];

                    // Iterate through all the tiles
                    for (var x = 0; x < row.Length; x++) {
                        // Get the current tile
                        var tile = row[x];
                        // Set the position of the object
                        var tileTransform = tile.transform;
                        tileTransform.localPosition = new Vector3(x, y, z);
                    }
                }
            }

            yield return null;
        }
        
        /**
         * <summary>
         * Finds all of the empty spaces then returns them in a tuple, with the first element is the index of the element in the grid,
         * and the second is the location in world space
         * </summary>
         */
        public IEnumerator<Tuple<Vector3, Vector3>> GetEmptySpaces() {
            // Loop through all the planes
            for (var z = 0; z < _grid.Count; z++) {
                // Get the current plane
                var plane = _grid[z];

                // Iterate through the rows
                for (var y = 0; y < plane.Length; y++) {
                    // Get the current row
                    var row = plane[y];

                    // Iterate through all the tiles
                    for (var x = 0; x < row.Length; x++) {
                        // Get the current tile
                        var tile = row[x];
                        // Check to see if the tile is empty
                        if (tile.transform.childCount == 0) {
                            yield return new Tuple<Vector3, Vector3>(new Vector3(z,y,x), tile.transform.position);
                        }
                    }
                }
            }
        }

        /** <summary>
         * Creates an empty plane
         * </summary>
         */
        private void AddPlane() {
            var columns = new GameObject[PlaneHeight][];

            for (var i = 0; i < PlaneHeight; i++) {
                var row = new GameObject[PlaneWidth]; 
                
                for (var ii = 0; ii < PlaneWidth; ii++) {
                    row[ii] = new GameObject();
                }

                columns[i] = row;
            }
            
            _grid.Add(columns);
        }

        private void Start() {
            AddPlane();
        }
    }
}