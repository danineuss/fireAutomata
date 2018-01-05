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
    public bool wrappingBoarders;
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
        
        // First, create all the cells according to the map size (x,y).
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector2 coordinates = new Vector2(x, y);
                CreateCell(coordinates, mapHolder);
            }
        }
        
        // Then create all the neighborhood relationships.
        if (wrappingBoarders)
        {
            foreach (Vector2 coordinates in cellDict.Keys)
            {
                SetNeighborsWrapped(coordinates);
            }
        }
        else
        {
            foreach (Vector2 coordinates in cellDict.Keys)
            {
                SetNeighbors(coordinates);
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

    /// <summary>
    /// This sets the neighbors without taking wrapped boarders into account. Simply the direct neighbors.
    /// </summary>
    /// <param name="coordinates">The coordinates of the current cell, for which the neighbors are set.</param>
    void SetNeighbors(Vector2 coordinates)
    {
        List<CellDirection> possibleNeighbors = new List<CellDirection>();
        possibleNeighbors.Add(CellDirection.N);
        possibleNeighbors.Add(CellDirection.E);
        possibleNeighbors.Add(CellDirection.S);
        possibleNeighbors.Add(CellDirection.W);

        if (coordinates.x == 0)
        {
            possibleNeighbors.Remove(CellDirection.W);
        }
        else if (coordinates.x == mapSize.x - 1)
        {
            possibleNeighbors.Remove(CellDirection.E);
        }

        if (coordinates.y == 0)
        {
            possibleNeighbors.Remove(CellDirection.S);
        }
        else if (coordinates.y == mapSize.y - 1)
        {
            possibleNeighbors.Remove(CellDirection.N);
        }

        // If we have N or S in the mix, then we can combine it with E and W.
        // The other way around would create wrong duplicates.
        if (possibleNeighbors.Contains(CellDirection.N))
        {
            if (possibleNeighbors.Contains(CellDirection.E))
            {
                possibleNeighbors.Add(CellDirection.NE);
            }
            if (possibleNeighbors.Contains(CellDirection.W))
            {
                possibleNeighbors.Add(CellDirection.NW);
            }
        }

        if (possibleNeighbors.Contains(CellDirection.S))
        {
            if (possibleNeighbors.Contains(CellDirection.E))
            {
                possibleNeighbors.Add(CellDirection.SE);
            }
            if (possibleNeighbors.Contains(CellDirection.W))
            {
                possibleNeighbors.Add(CellDirection.SW);
            }
        }
        
        //Now we have a list of all neighbors which have to be mated. Now you can do the mating!
        foreach (CellDirection direction in possibleNeighbors)
        {
            AddNeighbor(coordinates, direction);
        }
    }

    void SetNeighborsWrapped (Vector2 coordinates)
    {
        // Adjusting the map size to not always subtract one.
        Vector2 mapSizeMinor = mapSize - new Vector2(1, 1);

        // Here every cell has eight neighbors. We simply have to figure out the boarder cases.
        List<CellDirection> directions = new List<CellDirection>();
        for (int i = 0; i <= 7; i++)
        {
            directions.Add((CellDirection)i);
        }

        // East or West
        if (coordinates.x == 0)
        {
            PairCells(coordinates, new Vector2(mapSizeMinor.x, coordinates.y), CellDirection.W);
            directions.Remove(CellDirection.W);
        }
        else if (coordinates.x == mapSize.x - 1)
        {
            PairCells(coordinates, new Vector2(0, coordinates.y), CellDirection.E);
            directions.Remove(CellDirection.E);
        }

        // North or South
        if (coordinates.y == 0)
        {
            PairCells(coordinates, new Vector2(coordinates.x, mapSizeMinor.y), CellDirection.S);
            directions.Remove(CellDirection.S);
        }
        else if (coordinates.y == mapSize.y - 1)
        {
            PairCells(coordinates, new Vector2(coordinates.x, 0), CellDirection.N);
            directions.Remove(CellDirection.N);
        }

        // Corner cases:
        if (coordinates == new Vector2(0, 0))
        {
            PairCells(coordinates, mapSizeMinor, CellDirection.SW);
            directions.Remove(CellDirection.SW);
        }
        else if (coordinates == new Vector2(0, mapSizeMinor.y))
        {
            PairCells(coordinates, new Vector2(mapSizeMinor.x, 0), CellDirection.NW);
            directions.Remove(CellDirection.NW);
        }
        else if (coordinates == mapSizeMinor)
        {
            PairCells(coordinates, new Vector2(0, 0), CellDirection.NE);
            directions.Remove(CellDirection.NE);
        }
        else if (coordinates == new Vector2(mapSizeMinor.x, 0))
        {
            PairCells(coordinates, new Vector2(0, mapSizeMinor.y), CellDirection.SE);
            directions.Remove(CellDirection.SE);
        }

        //Now we have a list of all remaining ("normal") neighbors which have to be mated. Do the mating!
        foreach (CellDirection direction in directions)
        {
            AddNeighbor(coordinates, direction);
        }
    }

    /// <summary>
    /// This function will actually set up the neighborhood relationships between two cells by adding them to each others dictionaries.
    /// </summary>
    /// <param name="currentCoordinates">The cell which is examined now.</param>
    /// <param name="toNeighbor">The vector from the current to the previous cell.</param>
    void AddNeighbor(Vector2 currentCoordinates, CellDirection toNeighbor)
    {
        Vector2 neighborCoordinates = currentCoordinates + CellDirectionExtensions.Offset(toNeighbor);
        PairCells(currentCoordinates, neighborCoordinates, toNeighbor);
    }

    /// <summary>
    /// This function does the actual pairing of two cells. This function is required separately for the wrapped coordinate system,
    /// since there the two coordinates and the direction don't have a direct connection.
    /// </summary>
    /// <param name="currentCoordinates">This cells coordinates.</param>
    /// <param name="neighborCoordinates">The neighboring cell's coordinates.</param>
    /// <param name="toNeighbor">The direction towards the neighboring cell.</param>
    void PairCells(Vector2 currentCoordinates, Vector2 neighborCoordinates, CellDirection toNeighbor)
    {
        GetCell(currentCoordinates).AddNeighbor(toNeighbor, GetCell(neighborCoordinates));
        GetCell(neighborCoordinates).AddNeighbor(toNeighbor.Opposite(), GetCell(currentCoordinates));
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
