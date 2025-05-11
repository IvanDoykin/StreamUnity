using System;
using System.Collections.Generic;
using UnityEngine;

public class WallConfig : MonoBehaviour
{
    public static Dictionary<(int, int), SpriteRenderer> Walls = new Dictionary<(int, int), SpriteRenderer>();

    public void Initialize(int x, int y)
    {
        Walls.Add((x, y), GetComponent<SpriteRenderer>());
    }
}