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
                        Instantiate(_floorPrefab, new UnityEngine.Vector3(x, y, 0), UnityEngine.Quaternion.identity, _floors.transform);
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

        // Заполняем матрицу соседями (1 - стена, 0 - пол или отсутствие)
        matrix.M12 = (y > 0 && map[x, y - 1]) ? 1 : 0;          // Верх
        matrix.M32 = (y < map.GetLength(1) - 1 && map[x, y + 1]) ? 1 : 0; // Низ
        matrix.M21 = (x > 0 && map[x - 1, y]) ? 1 : 0;          // Лево
        matrix.M23 = (x < map.GetLength(0) - 1 && map[x + 1, y]) ? 1 : 0; // Право

        // Углы
        matrix.M11 = (x > 0 && y > 0 && map[x - 1, y - 1]) ? 1 : 0;          // Верх-лево
        matrix.M13 = (x < map.GetLength(0) - 1 && y > 0 && map[x + 1, y - 1]) ? 1 : 0; // Верх-право
        matrix.M31 = (x > 0 && y < map.GetLength(1) - 1 && map[x - 1, y + 1]) ? 1 : 0; // Низ-лево
        matrix.M33 = (x < map.GetLength(0) - 1 && y < map.GetLength(1) - 1 && map[x + 1, y + 1]) ? 1 : 0; // Низ-право

        // Анализ матрицы для определения типа стены
        bool top = matrix.M12 == 1;
        bool bottom = matrix.M32 == 1;
        bool left = matrix.M21 == 1;
        bool right = matrix.M23 == 1;

        bool topLeft = matrix.M11 == 1;
        bool topRight = matrix.M13 == 1;
        bool bottomLeft = matrix.M31 == 1;
        bool bottomRight = matrix.M33 == 1;

        // Основные правила (приоритет важнее порядка проверок)

        // Одиночная стена (нет соседей)
        if (!top && !bottom && !left && !right) return (13, matrix); // Новый тип для одиночных стен

        // Углы (проверяем сначала самые характерные случаи)
        if (!top && !left && (right || bottom)) return (3, matrix);  // Левый верхний
        if (!top && !right && (left || bottom)) return (4, matrix);  // Правый верхний
        if (!bottom && !left && (right || top)) return (5, matrix);  // Левый нижний
        if (!bottom && !right && (left || top)) return (6, matrix);  // Правый нижний

        // Т-образные соединения
        if (top && bottom && !left && right) return (7, matrix);  // Нет левой
        if (top && bottom && left && !right) return (8, matrix);  // Нет правой
        if (left && right && !top && bottom) return (9, matrix);  // Нет верха
        if (left && right && top && !bottom) return (10, matrix); // Нет низа

        // Прямые участки
        if (!top && !bottom && left && right) return (2, matrix); // Горизонтальная
        if (!left && !right && top && bottom) return (1, matrix); // Вертикальная

        // Внутренние углы (для двойных стен)
        if (top && left && !topLeft) return (14, matrix); // Внутренний угол верх-лево
        if (top && right && !topRight) return (15, matrix); // Внутренний угол верх-право
        if (bottom && left && !bottomLeft) return (16, matrix); // Внутренний угол низ-лево
        if (bottom && right && !bottomRight) return (17, matrix); // Внутренний угол низ-право

        // Дефолтный случай (внутренняя стена)
        return (0, matrix);
    }

    private bool IsNeed(bool[,] map, int x, int y)
    {
        // Стена не нужна, если она полностью окружена другими стенами
        if (x > 0 && !map[x - 1, y]) return true;
        if (x < map.GetLength(0) - 1 && !map[x + 1, y]) return true;
        if (y > 0 && !map[x, y - 1]) return true;
        if (y < map.GetLength(1) - 1 && !map[x, y + 1]) return true;

        // Диагональные проверки для сложных случаев
        if (x > 0 && y > 0 && !map[x - 1, y - 1]) return true;
        if (x > 0 && y < map.GetLength(1) - 1 && !map[x - 1, y + 1]) return true;
        if (x < map.GetLength(0) - 1 && y > 0 && !map[x + 1, y - 1]) return true;
        if (x < map.GetLength(0) - 1 && y < map.GetLength(1) - 1 && !map[x + 1, y + 1]) return true;

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