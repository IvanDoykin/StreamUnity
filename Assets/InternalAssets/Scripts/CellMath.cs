using System;
using UnityEngine;

public static class CellMath
{
    public const float CellSize = 1f;

    public static Vector2 ConvertCellToWorldPosition(Vector2Int cellPosition)
    {
        return new Vector2(cellPosition.x * CellSize, cellPosition.y * CellSize);
    }

    public static Vector2Int ConvertWorldToCellPosition(Vector2 position)
    {
        return new Vector2Int((int)Math.Round(position.x / CellSize), Mathf.FloorToInt(position.y / CellSize));
    }
}
