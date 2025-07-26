using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    private const float _noDangerRadius = 2f;

    [SerializeField] private int _width = 20;
    [SerializeField] private int _height = 20;
    [SerializeField] private int _roomCount = 5;
    [SerializeField] private int _minRoomSize = 3;
    [SerializeField] private int _maxRoomSize = 8;
    [SerializeField] private int _trapCount = 10;
    [SerializeField] private int _enemiesCount = 1;

    [SerializeField] private GameObject _floorPrefab;
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private Sprite[] _wallSprites;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _trapPrefab;
    [SerializeField] private GameObject _enemyPrefab;

    private GameObject _dungeon;
    private GameObject _walls;
    private GameObject _floors;
    private GameObject _traps;
    private GameObject _enemies;

    private Vector2Int _playerTestTarget;

    private List<Rect> _rooms = new List<Rect>();

    public bool[,] Map { get; private set; }
    public int[,] NavigationMap { get; private set; }

    public void Generate()
    {
        if (_dungeon != null)
        {
            NineSidesSFXPlayer.Instance.Dispose();

            _rooms.Clear();
            Destroy(_dungeon);
            WallConfig.Dispose();
        }
        _dungeon = new GameObject("Dungeon");
        _dungeon.transform.SetParent(transform);

        _walls = new GameObject("Walls");
        _walls.transform.SetParent(_dungeon.transform);

        _floors = new GameObject("Floors");
        _floors.transform.SetParent(_dungeon.transform);

        _traps = new GameObject("Traps");
        _traps.transform.SetParent(_dungeon.transform);

        _enemies = new GameObject("Enemies");
        _enemies.transform.SetParent(_dungeon.transform);

        InitializeMap();
        CreateRooms();
        CreatePlayer();
        CreateTraps();

        var enemies = CreateEnemies();
        var arena = new Arena(this, enemies);
    }

    private Enemy[] CreateEnemies()
    {
        Enemy[] enemies = new Enemy[_enemiesCount];
        var navigator = new AStarNavigator(NavigationMap);

        for (int i = 0; i < _enemiesCount; i++)
        {
            if (_rooms.Count == 0) break;
            enemies[i] = CreateEnemy(navigator);
        }

        return enemies;
    }

    private Enemy CreateEnemy(AStarNavigator navigator)
    {
        Rect room = _rooms[Random.Range(0, _rooms.Count)];
        float x = Random.Range(room.x + 1, room.x + room.width - 1);
        float y = Random.Range(room.y + 1, room.y + room.height - 1);
        UnityEngine.Vector2 enemyPosition = new UnityEngine.Vector2((int)x, (int)y);

        if (UnityEngine.Vector2.Distance(enemyPosition, _rooms[0].center) <= _noDangerRadius)
        {
            return CreateEnemy(navigator);
        }
        Enemy enemy = Instantiate(_enemyPrefab, new UnityEngine.Vector3((int)x, (int)y, 0), UnityEngine.Quaternion.identity, _enemies.transform).GetComponent<Enemy>();
        enemy.Initialize(navigator, new UnityEngine.Vector2Int((int)x, (int)y));
        enemy.Navigator.SetTarget(_playerTestTarget);
        return enemy;
    }

    private void InitializeMap()
    {
        Map = new bool[_width, _height];
        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                Map[x, y] = true;

        NavigationMap = new int[_width, _height];
        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                NavigationMap[x, y] = 1;
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
                {
                    for (int y = (int)newRoom.y; y < newRoom.y + newRoom.height; y++)
                    {
                        Map[x, y] = false;
                        NavigationMap[x, y] = 0;
                    }
                }
            }
        }

        for (int i = 1; i < _rooms.Count; i++)
        {
            UnityEngine.Vector2 previousCenter = _rooms[i - 1].center;
            UnityEngine.Vector2 currentCenter = _rooms[i].center;

            int dirX = (int)Mathf.Sign(currentCenter.x - previousCenter.x);
            for (int x = (int)previousCenter.x; x != (int)currentCenter.x; x += dirX)
            {
                Map[x, (int)previousCenter.y] = false;
                NavigationMap[x, (int)previousCenter.y] = 0;
            }

            int dirY = (int)Mathf.Sign(currentCenter.y - previousCenter.y);
            for (int y = (int)previousCenter.y; y != (int)currentCenter.y; y += dirY)
            {
                Map[(int)currentCenter.x, y] = false;
                NavigationMap[(int)currentCenter.x, y] = 0;
            }
        }

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (Map[x, y])
                {
                    if (IsNeed(Map, x, y))
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

            var wallIndex = GetWallInfo(Map, coordinates.Item1, coordinates.Item2);
            sprite.gameObject.name = wallIndex.ToString();
            sprite.sprite = _wallSprites[wallIndex];
        }
    }

    private int GetWallInfo(bool[,] map, int x, int y)
    {
        // Анализ матрицы для определения типа стены
        bool top = y > 0 && map[x, y - 1];
        bool bottom = y < map.GetLength(1) - 1 && map[x, y + 1];
        bool left = x > 0 && map[x - 1, y];
        bool right = x < map.GetLength(0) - 1 && map[x + 1, y];

        bool topLeft = x > 0 && y > 0 && map[x - 1, y - 1];
        bool topRight = x < map.GetLength(0) - 1 && y > 0 && map[x + 1, y - 1];
        bool bottomLeft = x > 0 && y < map.GetLength(1) - 1 && map[x - 1, y + 1];
        bool bottomRight = x < map.GetLength(0) - 1 && y < map.GetLength(1) - 1 && map[x + 1, y + 1];

        // Основные правила (приоритет важнее порядка проверок)

        // Одиночная стена (нет соседей)
        if (!top && !bottom && !left && !right) return 13; // Новый тип для одиночных стен

        // Углы (проверяем сначала самые характерные случаи)
        if (!top && !left && (right || bottom)) return 3;  // Левый верхний
        if (!top && !right && (left || bottom)) return 4;  // Правый верхний
        if (!bottom && !left && (right || top)) return 5;  // Левый нижний
        if (!bottom && !right && (left || top)) return 6;  // Правый нижний

        // Т-образные соединения
        if (top && bottom && !left && right) return 7;  // Нет левой
        if (top && bottom && left && !right) return 8;  // Нет правой
        if (left && right && !top && bottom) return 9;  // Нет верха
        if (left && right && top && !bottom) return 10; // Нет низа

        // Прямые участки
        if (!top && !bottom && left && right) return 2; // Горизонтальная
        if (!left && !right && top && bottom) return 1; // Вертикальная

        // Внутренние углы (для двойных стен)
        if (top && left && !topLeft) return 14; // Внутренний угол верх-лево
        if (top && right && !topRight) return 15; // Внутренний угол верх-право
        if (bottom && left && !bottomLeft) return 16; // Внутренний угол низ-лево
        if (bottom && right && !bottomRight) return 17; // Внутренний угол низ-право

        // Дефолтный случай (внутренняя стена)
        return 0;
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
            _playerTestTarget = new Vector2Int((int)spawnPos.x, (int)spawnPos.y);

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

        if (UnityEngine.Vector2.Distance(spikePosition, _rooms[0].center) <= _noDangerRadius)
        {
            CreateTrap();
            return;
        }
        Instantiate(_trapPrefab, new UnityEngine.Vector3((int)x, (int)y, 0), UnityEngine.Quaternion.identity, _traps.transform);

        NavigationMap[(int)x, (int)y] = 1;
    }
}