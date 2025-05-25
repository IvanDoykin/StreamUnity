using System;
using UnityEngine;

public class PlayerFinder : MonoBehaviour
{
    public event Action<Vector2Int> PlayerHasLocated;
    private PlayerMovement _player;

    private void Update()
    {
        if (_player != null)
        {
            PlayerHasLocated?.Invoke(CellMath.ConvertWorldToCellPosition(_player.transform.position));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<PlayerMovement>();
        if (player != null)
        {
            _player = player;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var player = collision.GetComponent<PlayerMovement>();
        if (player != null)
        {
            _player = null;
        }
    }
}
