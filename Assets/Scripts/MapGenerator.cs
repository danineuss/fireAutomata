using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The MapGenerator is called in the very beginning to initialize the map and when the "Ctrl" Key is pressed.
/// The latter will update the map to the new values in the EditorGUI.
/// </summary>
public class MapGenerator : MonoBehaviour {

    public Cell cellPrefab;
    public Vector2 mapSize;
    string holderName = "Generated Map";

    [Range(0,1)] public float outlinePercent;
    
    public Dictionary<Vector2, Cell> cellDict;
    Dictionary<Vector2, int> startingValues;
    public int[,] cellArray;

    public void Initialize()
    {
        cellDict = new Dictionary<Vector2, Cell>();
        startingValues = new Dictionary<Vector2, int>();
        GenerateMap();
        GenerateArray();
    }

    public void GenerateMap()
    {
        //// The Generated Map allows for a dynamically updated GUI in Unity. This means that the old one needs to be deleted and redone.
        //if (transform.FindChild(holderName))
        //{
        //    DestroyImmediate(transform.FindChild(holderName).gameObject);
        //    cellDict = new Dictionary<Vector2, Cell>();
        //}
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        
        // Create all the cells according to the map size (x,y).
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector2 coordinates = new Vector2(x, y);
                CreateCell(coordinates, mapHolder);
                SetNeighbors(coordinates);
            }
        }
        //ImprintStartingPattern(startingPatterns.circleOfFire);
    }

    void ImprintStartingPattern (int[,] startingPattern)
    {
        int patternX = startingPattern.GetLength(0);
        int patternY = startingPattern.GetLength(1);

        //The starting pattern should be placed all the way to the right and in the middle of the "height" (Y).
        int startingX = (int)mapSize.x - patternX;
        int endingX = (int)mapSize.x;
        int startingY = ((int)mapSize.y - patternY) / 2;
        int endingY = ((int)mapSize.y + patternY) / 2;

        for (int x = startingX; x < endingX; x++)
        {
            for (int y = startingY; y < endingY; y++)
            {
                cellDict[new Vector2(x, y)].CurrentValue = startingPattern[x - startingX, y - startingY];
            }
        }
    }

    void GenerateArray()
    {
        int arrayX = (int)mapSize.x;
        int arrayY = (int)mapSize.y;
        cellArray = new int[arrayX, arrayY];
    }

    void CreateCell(Vector2 coordinates, Transform mapHolder)
    {
        Vector3 cellPosition = new Vector3(-mapSize.x / 2 + 0.5f + coordinates.x, -mapSize.y / 2 + 0.5f + coordinates.y, 0);

        Cell currentCell = Instantiate(cellPrefab, cellPosition, Quaternion.identity, mapHolder);
        currentCell.Initialize(coordinates);
        currentCell.transform.localScale = Vector3.one * (1 - outlinePercent);

        cellDict.Add(coordinates, currentCell);
        startingValues.Add(coordinates, 0);
    }

    void SetNeighbors(Vector2 coordinates)
    {
        // If we have more than one cell in each direction (x and y) then we add the neighbor
        // in both cells respectively.
        if (coordinates.y > 0)
        {
            Vector2 previousCoordinates = coordinates + CellDirectionExtensions.Offset(CellDirection.S);
            PairCells(coordinates, previousCoordinates, CellDirection.S);

            // On the edge of the map we want to wrap around back to the first row.
            if (coordinates.y == mapSize.y - 1)
            {
                Vector2 firstRow = new Vector2(coordinates.x, 0);
                PairCells(coordinates, firstRow, CellDirection.N);
            }
        }

        if (coordinates.x > 0)
        {
            Vector2 prevCoordinates = coordinates + CellDirectionExtensions.Offset(CellDirection.W);
            PairCells(coordinates, prevCoordinates, CellDirection.W);

            // On the edge of the map we want to wrap around back to the first column.
            if (coordinates.x == mapSize.x - 1)
            {
                // E-W Pairing
                Vector2 firstColumn = new Vector2(0, coordinates.y);
                PairCells(coordinates, firstColumn, CellDirection.E);

                if (coordinates.y == 0)
                {
                    Vector2 topLeft = new Vector2(0, mapSize.y - 1);
                    PairCells(coordinates, topLeft, CellDirection.SE);
                    PairCells(coordinates, new Vector2(0, coordinates.y + 1), CellDirection.NE);
                }
                else if (coordinates.y == mapSize.y - 1)
                {
                    Vector2 bottomLeft = new Vector2(0, 0);
                    PairCells(coordinates, bottomLeft, CellDirection.NE);
                    PairCells(coordinates, new Vector2(0, coordinates.y - 1), CellDirection.SE);
                }
                else
                {
                    PairCells(coordinates, new Vector2(0, coordinates.y + 1), CellDirection.NE);
                    PairCells(coordinates, new Vector2(0, coordinates.y - 1), CellDirection.SE);
                }
            }

            // South-West / North-East pairing
            if (coordinates.y == 0)
            {
                Vector2 topPreviousColumn = new Vector2(coordinates.x - 1, mapSize.y - 1);
                PairCells(coordinates, topPreviousColumn, CellDirection.SW);
            }
            else
            {
                Vector2 southWestCoordinates = coordinates + CellDirectionExtensions.Offset(CellDirection.SW);
                PairCells(coordinates, southWestCoordinates, CellDirection.SW);
            }

            // North-West / South-East pairing
            if (coordinates.y == mapSize.y - 1)
            {
                Vector2 bottomPreviousColumn = new Vector2(coordinates.x - 1, 0);
                PairCells(coordinates, bottomPreviousColumn, CellDirection.NW);
            }
            else
            {
                Vector2 northWestCoordinates = coordinates + CellDirectionExtensions.Offset(CellDirection.NW);
                PairCells(coordinates, northWestCoordinates, CellDirection.NW);
            }
        }
    }

    /// <summary>
    /// This function will actually set up the neighborhood relationships between two cells by adding them to each others dictionaries.
    /// </summary>
    /// <param name="currentCoordinates">The cell which is examined now.</param>
    /// <param name="previousCoordinates">The cell in "previous" succession, meaning one step back in the coordinate system.</param>
    /// <param name="currentToPrevious">The vector from the current to the previous cell.</param>
    void PairCells(Vector2 currentCoordinates, Vector2 previousCoordinates, CellDirection currentToPrevious)
    {
        GetCell(currentCoordinates).AddNeighbor(currentToPrevious, GetCell(previousCoordinates));
        GetCell(previousCoordinates).AddNeighbor(currentToPrevious.Opposite(), GetCell(currentCoordinates));
    }

    Cell GetCell(Vector2 coordinates)
    {
        if (cellDict[coordinates] != null)
        {
            return cellDict[coordinates];
        }
        else
        {
            return null;
        }
    }
}
