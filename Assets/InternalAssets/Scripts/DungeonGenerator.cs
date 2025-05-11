using UnityEngine;
using System.Collections.Generic;
using System.Numerics;

public class DungeonGenerator : MonoBehaviour
{
    private const float _noTrapsRadius = 2f;

    [SerializeField] private int _width = 20;
    [SerializeField] private int _height = 20;
    [SerializeField] private int _roomCount = 5;
    [SerializeField] private int _minRoomSize = 3;
    [SerializeField] private int _maxRoomSize = 8;
    [SerializeField] private int _trapCount = 10;

    [SerializeField] private GameObject _floorPrefab;
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private Sprite[] _wallSprites;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _trapPrefab;

    private GameObject _dungeon;
    private GameObject _walls;
    private GameObject _floors;
    private GameObject _traps;

    private List<Rect> _rooms = new List<Rect>();
    private bool[,] _map;

    public void Generate()
    {
        if (_dungeon != null)
        {
            _rooms.Clear();
            Destroy(_dungeon);
        }
        _dungeon = new GameObject("Dungeon");
        _dungeon.transform.SetParent(transform);

        _walls = new GameObject("Walls");
        _walls.transform.SetParent(_dungeon.transform);

        _floors = new GameObject("Floors");
        _floors.transform.SetParent(_dungeon.transform);

        _traps = new GameObject("Traps");
        _traps.transform.SetParent(_dungeon.transform);

        InitializeMap();
        CreateRooms();
        CreatePlayer();
        CreateTraps();
    }

    private void InitializeMap()
    {
        _map = new bool[_width, _height];
        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                _map[x, y] = true;
    }

    private void CreateRooms()
    {
        for (int i = 0; i < _roomCount; i++)
        {
            int roomWidth = Random.Range(_minRoomSize, _maxRoomSize + 1);
            int roomHeight = Random.Range(_minRoomSize, _maxRoomSize + 1);
            int roomX = Random.Range(1, _width - roomWidth - 1);
            int roomY = Random.Range(1, _height - roomHeight - 1);

            Rect newRoom = new Rect(roomX, roomY, roomWidth, roomHeight);
            bool overlap = false;

            foreach (Rect room in _rooms)
            {
                if (newRoom.Overlaps(room))
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap)
            {
                _rooms.Add(newRoom);
                for (int x = (int)newRoom.x; x < newRoom.x + newRoom.width; x++)
                    for (int y = (int)newRoom.y; y < newRoom.y + newRoom.height; y++)
                        _map[x, y] = false;
            }
        }

        for (int i = 1; i < _rooms.Count; i++)
        {
            UnityEngine.Vector2 previousCenter = _rooms[i - 1].center;
            UnityEngine.Vector2 currentCenter = _rooms[i].center;

            int dirX = (int)Mathf.Sign(currentCenter.x - previousCenter.x);
            for (int x = (int)previousCenter.x; x != (int)currentCenter.x; x += dirX)
                _map[x, (int)previousCenter.y] = false;

            int dirY = (int)Mathf.Sign(currentCenter.y - previousCenter.y);
            for (int y = (int)previousCenter.y; y != (int)currentCenter.y; y += dirY)
                _map[(int)currentCenter.x, y] = false;
        }

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_map[x, y])
                {
                    if (IsNeed(_map, x, y))
                    {
                        var wall = Instantiate(_wallPrefab, new UnityEngine.Vector3(x, y, 0), UnityEngine.Quaternion.identity, _walls.transform);

                        var wallConfig = wall.AddComponent<WallConfig>();
                        wallConfig.Initialize(x, y);
                    }
                }
                else
                {
                    Instantiate(_floorPrefab, new UnityEngine.Vector3(x, y, 0), UnityEngine.Quaternion.identity, _floors.transform);
                }
            }
        }

        foreach (var wall in WallConfig.Walls)
        {
            var coordinates = wall.Key;
            var sprite = wall.Value;

            var wallInfo = GetWallInfo(_map, coordinates.Item1, coordinates.Item2);
            sprite.gameObject.name = wallInfo.Item1.ToString();
            sprite.sprite = _wallSprites[wallInfo.Item1];
        }
    }

    private (int, System.Numerics.Matrix4x4) GetWallInfo(bool[,] map, int x, int y)
    {
        var matrix = new System.Numerics.Matrix4x4(
                0, 0, 0, 0,
                0, -1, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0);
        // Проверяем, что координаты в пределах карты и это стена
        if (x < 0 || y < 0 || x >= map.GetLength(0) || y >= map.GetLength(1) || !map[x, y])
        {
            return (0, matrix); // Дефолтный вариант, если нет стены или координаты вне карты
        }

        // Проверяем наличие стен в соседних клетках (4 направления)
        bool left = x > 0 && map[x - 1, y];
        matrix.M21 = left ? 1 : 0;

        bool right = x < map.GetLength(0) - 1 && map[x + 1, y] && WallConfig.Walls.TryGetValue((x + 1, y), out SpriteRenderer sprite1);
        matrix.M23 = right ? 1 : 0;

        bool top = y > 0 && map[x, y - 1] && WallConfig.Walls.TryGetValue((x, y - 1), out SpriteRenderer sprite2);
        matrix.M12 = top ? 1 : 0;

        bool bottom = y < map.GetLength(1) - 1 && map[x, y + 1] && WallConfig.Walls.TryGetValue((x, y + 1), out SpriteRenderer sprite3);
        matrix.M32 = bottom ? 1 : 0;

        // Проверяем углы (для П-образных и сложных конфигураций)
        bool topLeft = x > 0 && y > 0 && map[x - 1, y - 1] && WallConfig.Walls.TryGetValue((x - 1, y - 1), out SpriteRenderer sprite4);
        matrix.M11 = topLeft ? 1 : 0;

        bool topRight = x < map.GetLength(0) - 1 && y > 0 && map[x + 1, y - 1] && WallConfig.Walls.TryGetValue((x + 1, y - 1), out SpriteRenderer sprite5);
        matrix.M13 = topRight ? 1 : 0;

        bool bottomLeft = x > 0 && y < map.GetLength(1) - 1 && map[x - 1, y + 1] && WallConfig.Walls.TryGetValue((x - 1, y + 1), out SpriteRenderer sprite6);
        matrix.M31 = bottomLeft ? 1 : 0;

        bool bottomRight = x < map.GetLength(0) - 1 && y < map.GetLength(1) - 1 && map[x + 1, y + 1] && WallConfig.Walls.TryGetValue((x + 1, y + 1), out SpriteRenderer sprite7);
        matrix.M33 = bottomRight ? 1 : 0;

        // Определяем тип стены
        if (top && bottom && left && right)
        {
            return (0, matrix); // Внутренняя стена (все соседи — стены)
        }
        else if (top && bottom && !left && !right)
        {
            return (1, matrix); // Вертикальная стена (сверху и снизу стены, слева и справа — пусто)
        }
        else if (!top && !bottom && left && right)
        {
            return (2, matrix); // Горизонтальная стена (слева и справа стены, сверху и снизу — пусто)
        }
        else if (!top && bottom && left && !right)
        {
            return (3, matrix); // Левый верхний угол (нет верха и справа)
        }
        else if (!top && bottom && !left && right)
        {
            return (4, matrix); // Правый верхний угол (нет верха и слева)
        }
        else if (top && !bottom && left && !right)
        {
            return (5, matrix); // Левый нижний угол (нет низа и справа)
        }
        else if (top && !bottom && !left && right)
        {
            return (6, matrix); // Правый нижний угол (нет низа и слева)
        }
        else if (top && bottom && !left && right)
        {
            return (7, matrix); // Т-образное соединение (нет стены слева)
        }
        else if (top && bottom && left && !right)
        {
            return (8, matrix); // Т-образное соединение (нет стены справа)
        }
        // Дополнительные проверки для П-образных конфигураций
        else if (!top && bottom && left && right)
        {
            // П-образная конфигурация сверху (нет верха, но есть слева и справа)
            return (9, matrix); // Можно добавить новый индекс или выбрать подходящий из существующих
        }
        else if (top && !bottom && left && right)
        {
            // П-образная конфигурация снизу (нет низа, но есть слева и справа)
            return (10, matrix);
        }
        else if (left && !right && top && bottom)
        {
            // П-образная конфигурация справа (нет правой стены)
            return (11, matrix);
        }
        else if (!left && right && top && bottom)
        {
            // П-образная конфигурация слева (нет левой стены)
            return (12, matrix);
        }
        else
        {
            return (0, matrix); // Дефолтный вариант
        }
    }

    private bool IsNeed(bool[,] map, int x, int y)
    {
        if (!map[x, y]) return true;

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        if (x > 0 && !map[x - 1, y]) return true;
        if (x < width - 1 && !map[x + 1, y]) return true;
        if (y > 0 && !map[x, y - 1]) return true;
        if (y < height - 1 && !map[x, y + 1]) return true;

        if (x > 0 && y > 0 && !map[x - 1, y - 1]) return true;
        if (x > 0 && y < height - 1 && !map[x - 1, y + 1]) return true;
        if (x < width - 1 && y > 0 && !map[x + 1, y - 1]) return true;
        if (x < width - 1 && y < height - 1 && !map[x + 1, y + 1]) return true;

        _map[x, y] = true;
        return false;
    }

    private void CreatePlayer()
    {
        if (_rooms.Count > 0)
        {
            UnityEngine.Vector2 spawnPos = _rooms[0].center;
            var player = Instantiate(_playerPrefab, new UnityEngine.Vector3(spawnPos.x, spawnPos.y, 0), UnityEngine.Quaternion.identity, _dungeon.transform);

            var playerHealth = player.GetComponentInChildren<Health>();
            playerHealth.HasDied += Generate;
        }
    }

    private void CreateTraps()
    {
        for (int i = 0; i < _trapCount; i++)
        {
            if (_rooms.Count == 0) break;
            CreateTrap();
        }
    }

    private void CreateTrap()
    {
        Rect room = _rooms[Random.Range(0, _rooms.Count)];
        float x = Random.Range(room.x + 1, room.x + room.width - 1);
        float y = Random.Range(room.y + 1, room.y + room.height - 1);
        UnityEngine.Vector2 spikePosition = new UnityEngine.Vector2((int)x, (int)y);

        if (UnityEngine.Vector2.Distance(spikePosition, _rooms[0].center) <= _noTrapsRadius)
        {
            CreateTrap();
            return;
        }
        Instantiate(_trapPrefab, new UnityEngine.Vector3((int)x, (int)y, 0), UnityEngine.Quaternion.identity, _traps.transform);
    }
}