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
    [Range(0,1)] public float outlinePercent;
    
    public Dictionary<Vector2, Cell> cellDict;
    public int[,] cellArray;
    string holderName = "Generated Map";

    public void Initialize()
    {
        cellDict = new Dictionary<Vector2, Cell>();
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
            Vector2 neighborCoordinates = coordinates + CellDirectionExtensions.Offset(direction);
            PairCells(coordinates, neighborCoordinates, direction);
        }
    }

    /// <summary>
    /// This function will create the neighborhood relationships when the boarders are wrapping around the board.
    /// This means, going over the edge on the east will put you all the way on the west boarder, just as in Snake (TM).
    /// </summary>
    /// <param name="coordinates">The coordinates of the current cell.</param>
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

        if (coordinates.x == 0)
        {
            float northWestY = (coordinates.y + 1) % mapSize.y;
            float southWestY = coordinates.y - 1;
            if (southWestY < 0)
            {
                southWestY += mapSize.y;
            }

            PairCells(coordinates, new Vector2(mapSizeMinor.x, coordinates.y), CellDirection.W);
            PairCells(coordinates, new Vector2(mapSizeMinor.x, northWestY), CellDirection.NW);
            PairCells(coordinates, new Vector2(mapSizeMinor.x, southWestY), CellDirection.SW);
            directions.Remove(CellDirection.W);
            directions.Remove(CellDirection.NW);
            directions.Remove(CellDirection.SW);
        }
        else if (coordinates.x == mapSizeMinor.x)
        {
            float northEastY = (coordinates.y + 1) % mapSize.y;
            float southEastY = coordinates.y - 1;
            if (southEastY < 0)
            {
                southEastY += mapSize.y;
            }
            
            PairCells(coordinates, new Vector2(0, coordinates.y), CellDirection.E);
            PairCells(coordinates, new Vector2(0, northEastY), CellDirection.NE);
            PairCells(coordinates, new Vector2(0, southEastY), CellDirection.SE);
            directions.Remove(CellDirection.E);
            directions.Remove(CellDirection.NE);
            directions.Remove(CellDirection.SE);
        }

        if (coordinates.y == 0)
        {
            float southEastX = (coordinates.x + 1) % mapSize.x;
            float southWestX = coordinates.x - 1;
            if (southWestX < 0)
            {
                southWestX += mapSize.x;
            }

            PairCells(coordinates, new Vector2(coordinates.x, mapSizeMinor.y), CellDirection.S);
            PairCells(coordinates, new Vector2(southWestX, mapSizeMinor.y), CellDirection.SW);
            PairCells(coordinates, new Vector2(southEastX, mapSizeMinor.y), CellDirection.SE);
            directions.Remove(CellDirection.S);
            directions.Remove(CellDirection.SW);
            directions.Remove(CellDirection.SE);
        }
        else if (coordinates.y == mapSizeMinor.y)
        {
            float northEastX = (coordinates.x + 1) % mapSize.x;
            float northWestX = coordinates.x - 1;
            if (northWestX < 0)
            {
                northWestX += mapSize.x;
            }

            PairCells(coordinates, new Vector2(coordinates.x, 0), CellDirection.N);
            PairCells(coordinates, new Vector2(northWestX, 0), CellDirection.NW);
            PairCells(coordinates, new Vector2(northEastX, 0), CellDirection.NE);
            directions.Remove(CellDirection.N); 
            directions.Remove(CellDirection.NW);
            directions.Remove(CellDirection.NE);
        }

        //Now we have a list of all remaining ("normal") neighbors which have to be mated. Do the mating!
        foreach (CellDirection direction in directions)
        {
            Vector2 neighborCoordinates = coordinates + CellDirectionExtensions.Offset(direction);
            PairCells(coordinates, neighborCoordinates, direction);
        }
    }

    void CreateCell(Vector2 coordinates, Transform mapHolder)
    {
        Vector3 cellPosition = new Vector3(-mapSize.x / 2 + 0.5f + coordinates.x, -mapSize.y / 2 + 0.5f + coordinates.y, 0);

        Cell currentCell = Instantiate(cellPrefab, cellPosition, Quaternion.identity, mapHolder);
        currentCell.Initialize(coordinates);
        currentCell.transform.localScale = Vector3.one * (1 - outlinePercent);

        cellDict.Add(coordinates, currentCell);
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
        //Debug.Log(currentCoordinates);
        //Debug.Log(neighborCoordinates);
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
