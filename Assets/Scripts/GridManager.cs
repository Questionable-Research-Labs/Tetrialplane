using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
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
        private List<GameObject[][]> _grid = new List<GameObject[][]>();

        private (int, GameObject)[][] _peaks;

        private Vector3 _scalerFromGridToLocal;

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
        public List<GameObject> AddBlocksToGrid(IEnumerable<GameObject> blocks, int x, int y, int z) {
            // The number of blocks that did not land on the grid
            var blocksMissedCount = 0;

            // The missed blockes
            var missedBlocks = new List<GameObject>();

            // Loop through all the blocks being added to the grid
            foreach (var block in blocks) {
                // Get the position of the block
                var blockPosition = block.transform.localPosition;

                // Get the z position of the block
                var blockZ = (int) Math.Floor(blockPosition.z) + z;

                // Add any necessary planes to account for new blocks 
                if (blockZ >= _grid.Count) {
                    for (var i = 0; i < blockZ - _grid.Count; i++) {
                        AddPlane();
                    }
                }

                // Check to make sure that the block has landed in the grid
                if (x + 1 > PlaneWidth || y + 1 > PlaneHeight) {
                    // Return the block
                    missedBlocks.Add(block);
                    // Increment the number of missed blocks
                    blocksMissedCount++;
                    continue;
                }

                // Get the transform of the gameobject in the grid to use as transform parent
                var parentTransform = _grid[blockZ][y + (int) blockPosition.y][x + (int) blockPosition.x].transform;

                // Parent the block
                block.transform.SetParent(parentTransform);

                // Set it's local position to 0
                block.transform.localPosition = Vector3.zero;
            }

            // Deduct the points from the player
            if (blocksMissedCount != 0) {
                scoreManager.PieceMissed(blocksMissedCount);
            }

            // Update the grid
            StartCoroutine(UpdateGrid());

            // Return the missed blocks
            return missedBlocks;
        }

        public void AddBlockToGrid(GameObject block,int x, int y, int z) {
            var tform = _grid[z][y][x].transform;
            block.transform.SetParent(tform);
            block.transform.localPosition = Vector3.zero;
            block.transform.rotation = Quaternion.identity;
        }

        /**
         * <summary>
         * Finds all of the empty spaces then returns them in a tuple, with the first element is the index of the element in the grid,
         * and the second is the location in world space
         * </summary>
         */
        public List<((int, int, int) localPosition, Vector3 position)> GetPeaks() {
            var peakPos = new List<((int,int,int), Vector3)>();

            if (_grid.Count == 1) {
                for(var y = 0; y < _grid[0].Length; y++) {
                    var row = _grid[0][y];
                    for (var x = 0; x < row.Length; x++) {
                        var tile = row[x];
                        Vector3 virtualLayerLocalPosition = tile.transform.localPosition + Vector3.down;
                        Vector3 virtualLayerWorldPosition = gridPlane.TransformPoint(virtualLayerLocalPosition);
                        peakPos.Add(((0, y, x), virtualLayerWorldPosition));
                    }
                }
            }
            else {
                for (var y = 0; y < _peaks.Length; y++) {
                    var row = _peaks[y];
                    for (var x = 0; x < row.Length; x++) {
                        var (z, tile) = row[x];
                        peakPos.Add(((z, y, x), tile.transform.position));
                    }
                }    
            }

            return peakPos;
        }
        
        public BlockPositionValidity ValidBlockPosition(int x, int y, int z) {
            if (_grid[z][y][x].transform.childCount > 0) {
                return BlockPositionValidity.SpaceTaken;
            }

            if (x > PlaneWidth || y > PlaneHeight) {
                return BlockPositionValidity.OutOfBounds;
            }

            if (z == 0) {
                return BlockPositionValidity.Connected;
            }
            
            return _grid[z - 1][y][x].transform.childCount > 0 
                ? BlockPositionValidity.Connected 
                : BlockPositionValidity.Floating;
        }
        
        /**
         * <summary>
         * Finds all of the empty spaces then returns them in a tuple, with the first element is the index of the element in the grid,
         * and the second is the location in world space
         * </summary>
         */
        public List<Tuple<(int,int,int), Vector3>> GetEmptySpaces() {
            var emptySpaces = new List<Tuple<(int,int,int), Vector3>>();
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
                            //Returns Grid ID and World Transform
                            emptySpaces.Add(new Tuple<(int,int,int), Vector3>((x, y, z), tile.transform.position));
                        }
                    }
                }
            }

            return emptySpaces;
        }

        /** <summary>
         * Removes any full planes, and updates the grid GameObject positions
         * </summary>
         * <returns></returns>
         */
        private IEnumerator UpdateGrid() {
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

            RecalculatePeaks();

            yield return null;
        }

        private void MoveTiles() {
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

        }

        /**
         * <summary>
         * Find all the peaks for any given index
         * </summary>
         */
        private void RecalculatePeaks() {
            MoveTiles();
            
            for (int y = 0; y < PlaneHeight; y++) {
                for (int x = 0; x < PlaneWidth; x++) {

                    for (int z = _grid.Count - 1; z >= 0; z--) {
                        if (_grid[z][y][x].transform.childCount > 0) {
                            Debug.Log($"Find grid piece for ({x},{y})");
                            _peaks[y][x] = (z, _grid[z][y][x]);
                            break;
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
            var z = _grid.Count;

            for (var y = 0; y < PlaneHeight; y++) {
                var row = new GameObject[PlaneWidth];

                for (var x = 0; x < PlaneWidth; x++) {
                    var newObject = new GameObject($"X: {x}, Y: {y}, Z: {z}");
                    newObject.transform.SetPositionAndRotation(ConvertFromGridIDToLocalSpace(x,y,z),Quaternion.identity);
                    newObject.transform.SetParent(gridPlane);
                    row[x] = newObject;
                }

                columns[y] = row;
            }
            
            MoveTiles();

            _grid.Add(columns);
        }

        private void Awake() {
            _peaks = new(int, GameObject)[PlaneHeight][];
            _scalerFromGridToLocal = new Vector3(transform.localScale.x / PlaneWidth,transform.localScale.z / PlaneHeight,0);
            _scalerFromGridToLocal.z = (_scalerFromGridToLocal.x + _scalerFromGridToLocal.y) / 2;

            for (var y = 0; y < PlaneHeight; y++) {
                _peaks[y] = new (int, GameObject)[PlaneWidth];
            }
            
            RecalculatePeaks();
        }

        private Vector3 ConvertFromGridIDToLocalSpace(int x, int y,int z) {
            return new Vector3(x * _scalerFromGridToLocal.x-transform.localScale.x/2f,z*_scalerFromGridToLocal.z,y *_scalerFromGridToLocal.y-transform.localScale.z/2f);
        }

        private void Start() {
            // Add an initial plane
            AddPlane();
        }
    }
}