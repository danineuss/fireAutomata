using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// The PuppetMaster is in control of all the values of the pixels and coordinates their colors.
/// </summary>
public class PuppetMaster : MonoBehaviour
{
    public MapGenerator map;

    float elapsedTime = 0.0f;
    float timeStep = 0.1f;
    public int score;
    bool gameStarted = false;

	// Use this for initialization
	void Awake ()
    {
        Initialize();
    }

    void Initialize()
    {
        map.Initialize();
    }

    void Setup()
    {
        score = 0;
        elapsedTime = 0.0f;
        gameStarted = true;
    }

    void Update()
    {
        if (gameStarted)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= timeStep)
            {
                elapsedTime = elapsedTime % timeStep;
                UpdateCells();
            }
        }
    }

    public void UpdateCells()
    {
        if (gameStarted)
        {
            // First, Game of Life happens.
            foreach (Cell cell in map.cellDict.Values)
            {
                EvolveCell(cell);
            }

            // Here the double buffering is switched and Next become Current. Additionally, we update the cellArray.
            foreach (Cell cell in map.cellDict.Values)
            {
                cell.UpdateCell();
            }
        }
    }

    public void ResetCells()
    {
        foreach (Cell cell in map.cellDict.Values)
        {
            cell.NextValue = 0.0f;
            cell.UpdateCell();
        }
    }

    /// <summary>
    /// This function evaluates the next state of a cell. Currently, Convay's Game of Life is implemented.
    /// </summary>
    /// <param name="cell"></param>
    void EvolveCell(Cell cell)
    {
        int neighborSum = cell.NeighborSum();
        
        // The cell is currently alive.
        if (cell.CurrentValue > 0.0f) 
        {
            if (neighborSum < 2 || neighborSum > 3)
            {
                cell.NextValue = 0.0f;
            }
            else
            {
                cell.NextValue = 1.0f;
            }
        }
        // The cell is currently dead.
        else
        {
            if (neighborSum == 3)
            {
                cell.NextValue = 1.0f;
            }
            else
            {
                cell.NextValue = 0.0f;
            }
        }
    }
}
