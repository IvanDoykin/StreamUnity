using System.Collections.Generic;
using UnityEngine;

public class NineSidesSFXPlayer : MonoBehaviour
{
    public static NineSidesSFXPlayer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NineSidesSFXPlayer>();
            }
            return _instance;
        }
    }
    private static NineSidesSFXPlayer _instance;

    private const float _ninthSideDistance = 0f;
    private const float _degreesInOneSide = 360f / 8f;

    [SerializeField] private List<List<QuequedSFXPlayer>> _queuedPlayers;
    [SerializeField] private SFXPlayer[] _sfxPlayers;
    [SerializeField] private Transform _player;
    [SerializeField] private List<Transform> _enemies;

    public void Initialize(PlayerMovement player)
    {
        _enemies = new List<Transform>();
        _player = player.transform;
        _sfxPlayers = player.GetComponentInChildren<NineSidesSFXPlayer>().GetComponentsInChildren<SFXPlayer>();
        _queuedPlayers = new List<List<QuequedSFXPlayer>>();
        foreach (var sfxPlayer in _sfxPlayers)
        {
            _queuedPlayers.Add(new List<QuequedSFXPlayer>());
        }
    }

    public void Dispose()
    {
        _player = null;
        _sfxPlayers = null;
        _enemies = null;
        _queuedPlayers = null;
    }

    public void AddEnemy(Enemy enemy)
    {
        _enemies.Add(enemy.transform);
    }

    public void RemoveEnemy(Enemy enemy)
    {
        _enemies.Remove(enemy.transform);
    }

    public void Enqueue(QuequedSFXPlayer quequedSFXPlayer)
    {
        if (Vector2.Distance(quequedSFXPlayer.transform.position, _player.position) < _ninthSideDistance)
        {
            Debug.Break();
            if (_queuedPlayers[8].Count == 0)
            {
                _queuedPlayers[8].Add(quequedSFXPlayer);
                _sfxPlayers[8].Play(quequedSFXPlayer.Source.clip);
            }
        }
        else
        {
            float degree = Vector2.Angle(_player.transform.up, quequedSFXPlayer.transform.position - _player.transform.position) - _degreesInOneSide / 2f;
            Debug.Log(degree);
            if (degree < 0)
            {
                degree = 360f + degree;
            }
            int sfxIndex = Mathf.CeilToInt(degree / _degreesInOneSide);

            if (_queuedPlayers[sfxIndex].Count == 0)
            {
                _queuedPlayers[sfxIndex].Add(quequedSFXPlayer);
                _sfxPlayers[sfxIndex].Play(quequedSFXPlayer.Source.clip);
            }
        }
    }

    public void Dequeue(QuequedSFXPlayer quequedSFXPlayer)
    {
        foreach (var list in _queuedPlayers)
        {
            list.Remove(quequedSFXPlayer);
        }
    }
}