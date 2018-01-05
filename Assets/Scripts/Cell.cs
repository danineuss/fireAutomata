using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell : MonoBehaviour {

    public Color defaultCellColor = new Color(255, 165, 0);
    public Color activatedCellColor;
    
    public Vector2 coordinates;
    public Dictionary<CellDirection, Cell> neighbors;
    
    bool inert = false;
    float currentValue;
    float nextValue;

    //float lerpDuration = 1.0f;
    //float lerpDelay = 0.2f;

    public float CurrentValue
    {
        get
        {
            return currentValue;
        }
        set
        {
            currentValue = value;
            if (value > 0.0f)
            {
                ChangeColor(activatedCellColor);
            }
            else
            {
                ChangeColor(defaultCellColor);
            }
        }
    }

    public float NextValue
    {
        get
        {
            return nextValue;
        }
        set
        {
            nextValue = value;
        }
    }

    void Start()
    {
        ChangeColor(defaultCellColor);
    }

    /// <summary>
    /// Initialize the cell.
    /// This function is called after the cell is instantiated.
    /// Here the position and the properties of the cell are set.
    /// </summary>
    /// <param name="coordinates">hex coordinates of the cell</param>
    public void Initialize(Vector2 coordinates)
    {
        // set cell position and coordinates
        this.coordinates = coordinates;
        neighbors = new Dictionary<CellDirection, Cell>();
        
        currentValue = 0.0f;
        nextValue = 0.0f;
    }

    public void AddNeighbor(CellDirection direction, Cell cell)
    {
        // If the cell is valid and not yet in the neighbor dictionary, then add it.
        if (cell != null && !neighbors.ContainsKey(direction))
        {
            neighbors.Add(direction, cell);
        }
    }

    public int NeighborSum ()
    {
        int neighborSum = 0;
        foreach (Cell neighbor in neighbors.Values)
        {
            if (neighbor.CurrentValue > 0.0f)
            {
                neighborSum += 1;
            }
        }
        return neighborSum;
    }

    public void UpdateCell()
    {
        if (!inert)
        {
            currentValue = nextValue;
            if (currentValue == 1.0f)
            {
                ChangeColor(activatedCellColor);
            }
            else
            {
                ChangeColor(defaultCellColor);
            }
        }
    }
    
    void OnMouseDown ()
    {
        if (currentValue == 1.0f)
        {
            currentValue = 0.0f;
            ChangeColor(defaultCellColor);
        }
        else
        {
            currentValue = 1.0f;
            ChangeColor(activatedCellColor);
        }
        foreach (Cell neighbor in neighbors.Values)
        {
            neighbor.ChangeColor(Color.red);
        }
    }

    public void ChangeColor (Color newColor)
    {
        GetComponent<Renderer>().material.SetColor("_Color", newColor);
    }
    
    //public IEnumerator LerpColor (Color beginColor, Color targetColor, float duration, float delay=0.0f)
    //{
    //    yield return new WaitForSeconds(delay);
    //    float time = 0.0f;
    //    while (time < duration)
    //    {
    //        Color color = Color.Lerp(beginColor, targetColor, time / duration);
    //        ChangeColor(color);
    //        time += Time.deltaTime;

    //        yield return null;
    //    }
    //}
}
