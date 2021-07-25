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

        public String scoreManagerName;
        private ScoreManager scoreManager;

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
        
        public GameObject emptyTileGameObjectPrefab;


        /** <summary>
         * The planes on the players hand
         * Indexed by [z][y][x]
         * Each tile is stored in rows, each row is stored in a plane 
         * </summary>
         */
        private List<GameObject[][]> _grid = new List<GameObject[][]>();

        private (int, GameObject)?[][] _peaks;

        private Vector3 _scalerFromGridToLocal;

        public int countAddPlane = 0;


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
            
            // The blocks that landed in the grid
            var validBlocks = new List<(GameObject, (int, int, int))>();
            
            // Weather or not a connected block was found
            var foundConnected = false;

            // Check to make sure all the blocks are valid
            var blocksArray = blocks as GameObject[] ?? blocks.ToArray();
            
            foreach (var block in blocksArray) {
                // Get the local position
                var localPosition = block.transform.localPosition;
                
                // Calculate the block positions in the grid
                int blockX = Mathf.FloorToInt(localPosition.x) + x;
                int blockY = Mathf.FloorToInt(localPosition.y) + y;
                int blockZ = Mathf.FloorToInt(localPosition.z) + z;

                while (blockZ + 1 >= _grid.Count) {
                    Debug.Log("Adding Plane");
                    AddPlane();
                }

                // Check whether the block is in a valid position
                switch (ValidBlockPosition(blockX, blockY, blockZ)) {
                    case BlockPositionValidity.Connected:
                        foundConnected = true;
                        validBlocks.Add((block, (blockX, blockY, blockZ)));
                        break;
                    case BlockPositionValidity.Floating:
                        validBlocks.Add((block, (blockX, blockY, blockZ)));
                        break;
                    case BlockPositionValidity.OutOfBounds:
                        missedBlocks.Add(block);
                        break;
                    case BlockPositionValidity.SpaceTaken:
                        Debug.Log("Did the overlap");
                        return blocksArray.ToList();
                }
            }
            
            // Return the entire array if no valid blocks were found
            if (!foundConnected) {
                return blocksArray.ToList();
            }

            // Loop through all the blocks being added to the grid
            foreach (var (block, (blockX, blockY, blockZ)) in validBlocks) {
                // Add any necessary planes to account for new blocks 

                
                // Get the transform of the gameobject in the grid to use as transform parent
                var parentTransform = _grid[blockZ][blockY][blockX].transform;

                // Parent the block
                block.transform.SetParent(parentTransform);

                // Set it's local position to 0
                block.transform.localPosition = Vector3.zero;
                
                // Set the blocks rotation to 0
                block.transform.rotation = Quaternion.identity;
            }

            // Deduct the points from the player
            if (blocksMissedCount != 0) {
                scoreManager.PieceMissed(blocksMissedCount);
            }

            // Update the grid
            UpdateGrid();

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
                    (int, GameObject)?[] row = _peaks[y];
                    for (var x = 0; x < row.Length; x++) {
                        var (z, tile) = row[x] ?? (0, _grid[0][y][x]);
                        Vector3 virtualLayerLocalPosition = tile.transform.localPosition + Vector3.down;
                        Vector3 virtualLayerWorldPosition = gridPlane.TransformPoint(virtualLayerLocalPosition);
                        peakPos.Add(((z, y, x), virtualLayerWorldPosition));
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
        private void UpdateGrid() {
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
                        tile.transform.localPosition = ConvertFromGridIDToLocalSpace(x,y,z+1);
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
            countAddPlane += 1;

            for (var y = 0; y < PlaneHeight; y++) {
                var row = new GameObject[PlaneWidth];

                for (var x = 0; x < PlaneWidth; x++) {
                    // var newObject = new GameObject($"X: {x}, Y: {y}, Z: {z}");
                    GameObject newObject = Instantiate(emptyTileGameObjectPrefab,ConvertFromGridIDToLocalSpace(x,y,z),Quaternion.identity);
                    
                    // newObject.transform.SetPositionAndRotation(ConvertFromGridIDToLocalSpace(x,y,z),Quaternion.identity);
                    newObject.transform.SetParent(gridPlane);
                    newObject.name = $"X: {x}, Y: {y}, Z: {z}";
                    if (z > 0) {
                        Debug.Log("YESYESYSYEYSEYSEYD");
                    }
                    row[x] = newObject;
                }

                columns[y] = row;
            }
            
            MoveTiles();

            _grid.Add(columns);
        }

        private void Awake() {
            scoreManager = GameObject.Find(scoreManagerName).GetComponent<ScoreManager>();
            
            _peaks = new(int, GameObject)?[PlaneHeight][];
            _scalerFromGridToLocal = new Vector3(transform.localScale.x / PlaneWidth,transform.localScale.z / PlaneHeight,0);
            _scalerFromGridToLocal.z = (_scalerFromGridToLocal.x + _scalerFromGridToLocal.y) / 2;

            for (var y = 0; y < PlaneHeight; y++) {
                _peaks[y] = new (int, GameObject)?[PlaneWidth];
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