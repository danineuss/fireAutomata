using UnityEngine;

public enum CellDirection
{
    // North is in Y+, East in X+, South in Y-, West in X-. The rest accordingly.
    N, NE, E, SE, S, SW, W, NW
}

public static class CellDirectionExtensions
{
    /// <summary>
    /// Get the oppposite of a given direction .
    /// </summary>
    /// <param name="direction">direction one wants the opposite of</param>
    /// <returns>the opposite direction</returns>
    public static CellDirection Opposite(this CellDirection direction)
    {
        // make sure to not overflow
        if ((int)direction < 4)
        {
            return direction + 4;
        }
        else
        {
            return direction - 4;
        }
    }

    /// <summary>
    /// This function will rotate a given direction in mathematical positive direction (Rechte-Hand Regel).
    /// </summary>
    /// <param name="direction">The initial direction.</param>
    /// <param name="angle">The angle of rotation in 45° steps.</param>
    /// <returns></returns>
    public static CellDirection Rotated(this CellDirection direction, float angle)
    {
        CellDirection newDirection = direction - (int)(angle / 45.0f);
        if ((int)newDirection < 0)
        {
            newDirection += 8;
        }
        else if ((int)newDirection > 7)
        {
            newDirection -= 8;
        }
        return newDirection;
    }

    
    public static Vector2 Offset(CellDirection[] directions)
    {
        Vector2 accumulatedOffset = new Vector2();
        foreach (CellDirection direction in directions)
        {
            accumulatedOffset += Offset(direction);
        }
        return accumulatedOffset;
    }

    public static Vector2 Offset(CellDirection direction)
    {
        if (direction == CellDirection.N)
        {
            return new Vector2(0, 1);
        }
        else if (direction == CellDirection.NE)
        {
            return new Vector2(1, 1);
        }
        else if (direction == CellDirection.E)
        {
            return new Vector2(1, 0);
        }
        else if (direction == CellDirection.SE)
        {
            return new Vector2(1, -1);
        }
        else if (direction == CellDirection.S)
        {
            return new Vector2(0, -1);
        }
        else if (direction == CellDirection.SW)
        {
            return new Vector2(-1, -1);
        }
        else if (direction == CellDirection.W)
        {
            return new Vector2(-1, 0);
        }
        else
        {
            return new Vector2(-1, 1);
        }
    }
}