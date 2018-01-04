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
            PairCells(coordinates, direction);
        }
    }

    void SetNeighborsWrapped (Vector2 coordinates)
    {

    }

    /// <summary>
    /// This function will actually set up the neighborhood relationships between two cells by adding them to each others dictionaries.
    /// </summary>
    /// <param name="currentCoordinates">The cell which is examined now.</param>
    /// <param name="toNeighbor">The vector from the current to the previous cell.</param>
    void PairCells(Vector2 currentCoordinates, CellDirection toNeighbor)
    {
        Vector2 neighborCoordinates = currentCoordinates + CellDirectionExtensions.Offset(toNeighbor);
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
