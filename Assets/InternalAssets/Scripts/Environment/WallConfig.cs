using System;
using System.Collections.Generic;
using UnityEngine;

public class WallConfig : MonoBehaviour
{
    public static IReadOnlyDictionary<(int, int), SpriteRenderer> Walls => _walls;
    private static Dictionary<(int, int), SpriteRenderer> _walls = new Dictionary<(int, int), SpriteRenderer>();

    private (int, int) _coordinates;

    public void Initialize(int x, int y)
    {
        _coordinates = (x, y);
        _walls.Add(_coordinates, GetComponent<SpriteRenderer>());
    }

    public static void Dispose()
    {
        _walls.Clear();
    }
}