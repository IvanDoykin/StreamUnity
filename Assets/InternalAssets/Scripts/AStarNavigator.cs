using UnityEngine;

public class AStarNavigator
{
    private struct PathNode
    {
        public Vector2Int Position;
        public int FCost;
        public int GCost;
        public int HeapIndex;
    }

    private Vector2Int _target;
    private bool[,] _map;
    private PathNode[] _neighborsBuffer = new PathNode[4];
    private static readonly Vector2Int[] _directions = {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    public AStarNavigator(bool[,] map)
    {
        _map = (bool[,])map.Clone();
    }

    public Vector2Int GetNextStep(Vector2Int currentPosition)
    {
        if (currentPosition == _target) return currentPosition;

        int width = _map.GetLength(0);
        int height = _map.GetLength(1);

        var openSet = new Heap(width * height);
        var closedSet = new bool[width, height];
        var gCosts = new int[width, height];
        var cameFrom = new Vector2Int[width, height];
        var inOpenSet = new bool[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                gCosts[x, y] = int.MaxValue;

        gCosts[currentPosition.x, currentPosition.y] = 0;
        cameFrom[currentPosition.x, currentPosition.y] = new Vector2Int(-1, -1);

        openSet.Add(new PathNode
        {
            Position = currentPosition,
            FCost = CalculateHeuristic(currentPosition, _target),
            GCost = 0,
            HeapIndex = 0
        });
        inOpenSet[currentPosition.x, currentPosition.y] = true;

        while (openSet.Count > 0)
        {
            PathNode currentNode = openSet.RemoveFirst();
            inOpenSet[currentNode.Position.x, currentNode.Position.y] = false;
            closedSet[currentNode.Position.x, currentNode.Position.y] = true;

            if (currentNode.Position == _target)
                return GetFirstStep(cameFrom, currentNode.Position);

            int neighborsCount = GetNeighbors(currentNode.Position);

            for (int i = 0; i < neighborsCount; i++)
            {
                var neighborPos = _neighborsBuffer[i].Position;

                if (closedSet[neighborPos.x, neighborPos.y])
                    continue;

                int tentativeGCost = currentNode.GCost + 1;

                if (tentativeGCost < gCosts[neighborPos.x, neighborPos.y])
                {
                    cameFrom[neighborPos.x, neighborPos.y] = currentNode.Position;
                    gCosts[neighborPos.x, neighborPos.y] = tentativeGCost;
                    int fCost = tentativeGCost + CalculateHeuristic(neighborPos, _target);

                    var neighborNode = new PathNode
                    {
                        Position = neighborPos,
                        FCost = fCost,
                        GCost = tentativeGCost
                    };

                    if (!inOpenSet[neighborPos.x, neighborPos.y])
                    {
                        openSet.Add(neighborNode);
                        inOpenSet[neighborPos.x, neighborPos.y] = true;
                    }
                    else
                    {
                        openSet.Update(neighborNode);
                    }
                }
            }
        }

        return currentPosition;
    }

    private int GetNeighbors(Vector2Int position)
    {
        int count = 0;
        for (int i = 0; i < _directions.Length; i++)
        {
            var neighborPos = position + _directions[i];
            if (IsPositionValid(neighborPos) && !_map[neighborPos.x, neighborPos.y])
            {
                _neighborsBuffer[count++].Position = neighborPos;
            }
        }
        return count;
    }

    private Vector2Int GetFirstStep(Vector2Int[,] cameFrom, Vector2Int end)
    {
        Vector2Int current = end;
        Vector2Int next = current;

        while (cameFrom[current.x, current.y].x >= 0)
        {
            next = current;
            current = cameFrom[current.x, current.y];
        }

        return next == end ? end : next;
    }

    private bool IsPositionValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < _map.GetLength(0) &&
               position.y >= 0 && position.y < _map.GetLength(1);
    }

    private int CalculateHeuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public void SetTarget(Vector2Int target)
    {
        _target = target;
    }

    private class Heap
    {
        private PathNode[] _items;
        private int _count;

        public Heap(int capacity) => _items = new PathNode[capacity];
        public int Count => _count;

        public void Add(PathNode item)
        {
            item.HeapIndex = _count;
            _items[_count] = item;
            SortUp(_count);
            _count++;
        }

        public PathNode RemoveFirst()
        {
            PathNode first = _items[0];
            _count--;
            _items[0] = _items[_count];
            _items[0].HeapIndex = 0;
            SortDown(0);
            return first;
        }

        public void Update(PathNode item)
        {
            int index = item.HeapIndex;
            SortUp(index);
            SortDown(index);
        }

        private void SortUp(int index)
        {
            PathNode item = _items[index];
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (item.FCost >= _items[parent].FCost)
                    break;

                _items[index] = _items[parent];
                _items[index].HeapIndex = index;
                index = parent;
            }
            _items[index] = item;
            _items[index].HeapIndex = index;
        }

        private void SortDown(int index)
        {
            PathNode item = _items[index];
            while (true)
            {
                int left = index * 2 + 1;
                int right = index * 2 + 2;

                if (left >= _count)
                    break;

                int smallest = left;
                if (right < _count && _items[right].FCost < _items[left].FCost)
                    smallest = right;

                if (item.FCost <= _items[smallest].FCost)
                    break;

                _items[index] = _items[smallest];
                _items[index].HeapIndex = index;
                index = smallest;
            }
            _items[index] = item;
            _items[index].HeapIndex = index;
        }
    }
}