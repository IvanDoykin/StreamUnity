using System;
using System.Collections.Generic;
using UnityEngine;

public class WallConfig : MonoBehaviour
{
    public static Dictionary<(int, int), SpriteRenderer> Walls = new Dictionary<(int, int), SpriteRenderer>();
    private (int, int) _coordinates;

    public void Initialize(int x, int y)
    {
        _coordinates = (x, y);
        Walls.Add(_coordinates, GetComponent<SpriteRenderer>());
    }
}