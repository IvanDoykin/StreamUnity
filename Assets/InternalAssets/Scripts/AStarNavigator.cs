using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarNavigator
{
    private Vector2Int _target;
    private bool[,] _map;

    public AStarNavigator(bool[,] map)
    {
        _map = new bool[map.GetLength(0), map.GetLength(1)];
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                _map[i, j] = map[i, j];
            }
        }
    }

    public Vector2Int GetNextStep(Vector2Int currentPosition)
    {
        if (currentPosition == _target) return currentPosition;

        var openSet = new List<Node>();
        var closedSet = new HashSet<Node>();
        var startNode = new Node(currentPosition, 0, CalculateHeuristic(currentPosition, _target), null);

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            var currentNode = openSet.OrderBy(n => n.FCost).First();

            if (currentNode.Position == _target)
            {
                return GetFirstStepInPath(currentNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (var neighbor in GetNeighbors(currentNode))
            {
                if (closedSet.Any(n => n.Position == neighbor.Position)) continue;

                var existingNode = openSet.FirstOrDefault(n => n.Position == neighbor.Position);
                if (existingNode == null || neighbor.GCost < existingNode.GCost)
                {
                    if (existingNode != null) openSet.Remove(existingNode);
                    openSet.Add(neighbor);
                }
            }
        }

        // Если путь не найден, возвращаем текущую позицию
        return currentPosition;
    }

    private Vector2Int GetFirstStepInPath(Node targetNode)
    {
        var path = new List<Node>();
        var currentNode = targetNode;

        while (currentNode.Parent != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path.Count > 0 ? path[0].Position : targetNode.Position;
    }

    private IEnumerable<Node> GetNeighbors(Node node)
    {
        var directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        foreach (var direction in directions)
        {
            var neighborPos = node.Position + direction;

            if (IsPositionValid(neighborPos) && !_map[neighborPos.x, neighborPos.y])
            {
                var gCost = node.GCost + 1;
                var hCost = CalculateHeuristic(neighborPos, _target);
                yield return new Node(neighborPos, gCost, hCost, node);
            }
        }
    }

    private bool IsPositionValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < _map.GetLength(0) &&
               position.y >= 0 && position.y < _map.GetLength(1);
    }

    private int CalculateHeuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Манхэттенское расстояние
    }

    public void SetTarget(Vector2Int target)
    {
        _target = target;
    }

    private class Node
    {
        public Vector2Int Position { get; }
        public int GCost { get; } // Расстояние от старта
        public int HCost { get; } // Эвристическое расстояние до цели
        public int FCost => GCost + HCost; // Общая стоимость
        public Node Parent { get; }

        public Node(Vector2Int position, int gCost, int hCost, Node parent)
        {
            Position = position;
            GCost = gCost;
            HCost = hCost;
            Parent = parent;
        }
    }
}