using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Sprite[] _idleSprites;
    [SerializeField] private Sprite[] _movementSprites;

    [SerializeField] private float _idleUpdateTime = 0.5f;
    [SerializeField] private float _movementUpdateTime = 0.35f;

    private bool _isIdle = true;
    private IMovement _movement;
    private float _timer = 0f;
    private int _spriteIndex = 0;

    private void Start()
    {
        _renderer.sprite = _idleSprites[0];
        _movement = GetComponent<IMovement>();
    }

    private void Update()
    {
        if (_isIdle)
        {
            if (_movement.Velocity.sqrMagnitude != 0)
            {
                _timer = 0f;
                _spriteIndex = 0;
                _isIdle = false;
            }
            else
            {
                _timer += Time.deltaTime;
                if (_timer > _idleUpdateTime)
                {
                    _timer = 0f;
                    _spriteIndex++;
                    if (_spriteIndex >= _idleSprites.Length)
                    {
                        _spriteIndex = 0;
                    }
                }
            }
        }
        else
        {
            if (_movement.Velocity.sqrMagnitude > 0)
            {
                _timer += Time.deltaTime;
                if (_timer > _movementUpdateTime)
                {
                    _timer = 0f;
                    _spriteIndex++;
                    if (_spriteIndex >= _movementSprites.Length)
                    {
                        _spriteIndex = 0;
                    }
                }
            }
            else
            {
                _timer = 0f;
                _spriteIndex = 0;
                _isIdle = true;
            }
        }

        if (_isIdle)
        {
            _renderer.sprite = _idleSprites[_spriteIndex];
        }
        else
        {
            if (_movement.Velocity.x != 0)
            {
                _renderer.flipX = _movement.Velocity.x < 0;
            }
            _renderer.sprite = _movementSprites[_spriteIndex];
        }
    }
}

// EnemyList = List<Transform>
// Player = Transform
// List<List<Transform>> enemyBySides