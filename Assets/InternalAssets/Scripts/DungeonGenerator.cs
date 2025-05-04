using UnityEngine;
using System.Collections.Generic;

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
            Vector2 prevCenter = _rooms[i - 1].center;
            Vector2 currCenter = _rooms[i].center;

            int dirX = (int)Mathf.Sign(currCenter.x - prevCenter.x);
            for (int x = (int)prevCenter.x; x != (int)currCenter.x; x += dirX)
                _map[x, (int)prevCenter.y] = false;

            int dirY = (int)Mathf.Sign(currCenter.y - prevCenter.y);
            for (int y = (int)prevCenter.y; y != (int)currCenter.y; y += dirY)
                _map[(int)currCenter.x, y] = false;
        }

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_map[x, y])
                {
                    if (IsNeed(_map, x, y))
                    {
                        var wall = Instantiate(_wallPrefab, new Vector3(x, y, 0), Quaternion.identity, _walls.transform);
                        wall.GetComponent<SpriteRenderer>().sprite = _wallSprites[GetWallIndex(_map, x, y)];
                    }
                }
                else
                {
                    Instantiate(_floorPrefab, new Vector3(x, y, 0), Quaternion.identity, _floors.transform);
                }
            }
        }
    }

    private int GetWallIndex(bool[,] map, int x, int y)
    {
        // Если это не стена — возвращаем -1 (не должно вызываться для пола)
        if (!map[x, y]) return -1;

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        // Проверяем 8 соседних клеток (по часовой стрелке, начиная с верхнего левого)
        bool[] neighbors = new bool[8] {
        /* 0 */ x > 0      && y < height-1 && !map[x-1, y+1],  // Верхний левый (пол?)
        /* 1 */                y < height-1 && !map[x,   y+1],  // Верхний
        /* 2 */ x < width-1 && y < height-1 && !map[x+1, y+1],  // Верхний правый
        /* 3 */ x < width-1                && !map[x+1,   y],  // Правый
        /* 4 */ x < width-1 && y > 0      && !map[x+1, y-1],  // Нижний правый
        /* 5 */                y > 0      && !map[x,   y-1],  // Нижний
        /* 6 */ x > 0      && y > 0      && !map[x-1, y-1],  // Нижний левый
        /* 7 */ x > 0                     && !map[x-1,   y]   // Левый
    };

        // Строим битовую маску (каждый бит — есть ли пол в направлении)
        int bitmask = 0;
        for (int i = 0; i < 8; i++)
        {
            if (neighbors[i]) bitmask |= (1 << i);
        }

        // Определяем тип стены по битовой маске
        switch (bitmask)
        {
            // Одиночные направления
            case 0b00000010: return 0;  // Только верх (↑)
            case 0b00001000: return 1;  // Только право (→)
            case 0b00100000: return 2;  // Только низ (↓)
            case 0b10000000: return 3;  // Только лево (←)

            // Углы
            case 0b00000011: return 4;  // Верх + верх-лево (┛)
            case 0b00001100: return 5;  // Право + верх-право (┗)
            case 0b00110000: return 6;  // Низ + низ-право (┏)
            case 0b11000000: return 7;  // Лево + низ-лево (┓)

            // Т-образные
            case 0b00001010: return 8;  // Верх + право (┣)
            case 0b00101000: return 9;  // Право + низ (┻)
            case 0b10100000: return 10; // Низ + лево (┫)
            case 0b10000010: return 11; // Лево + верх (┳)

            // Перекрестки
            case 0b00101010: return 12; // Верх + право + низ (┃)
            case 0b10001000: return 13; // Право + лево + верх (━)
            case 0b10101010: return 14; // Все 4 стороны (╋)

            // Внутренние углы (для "выступов")
            case 0b11000010: return 15; // Лево + верх + верх-лево (┛ + выступ)
            case 0b00001110: return 16; // Право + верх + верх-право (┗ + выступ)
            case 0b01110000: return 17; // Право + низ + низ-право (┏ + выступ)
            case 0b11100000: return 18; // Лево + низ + низ-лево (┓ + выступ)

            default: return 0; // Дефолтный вариант (не должно возникать при IsNeed)
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
            Vector2 spawnPos = _rooms[0].center;
            var player = Instantiate(_playerPrefab, new Vector3(spawnPos.x, spawnPos.y, 0), Quaternion.identity, _dungeon.transform);

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
        Vector2 spikePosition = new Vector2((int)x, (int)y);

        if (Vector2.Distance(spikePosition, _rooms[0].center) <= _noTrapsRadius)
        {
            CreateTrap();
            return;
        }
        Instantiate(_trapPrefab, new Vector3((int)x, (int)y, 0), Quaternion.identity, _traps.transform);
    }
}