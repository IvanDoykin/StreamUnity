using UnityEngine;
using UnityEngine.Events;

public class MovementSound : MonoBehaviour
{
    [SerializeField] private UnityEvent _hasSound;
    [SerializeField] private float _needMovementTime = 0.5f;

    private IMovement _movement;
    private float _movementTime = 0f;

    private void Start()
    {
        _movement = GetComponent<IMovement>();
    }

    private void Update()
    {
        if (_movement.Velocity.sqrMagnitude > 0f)
        {
            _movementTime += Time.deltaTime;
        }
        else
        {
            _movementTime = 0f;
        }

        if (_movementTime > _needMovementTime)
        {
            _movementTime = 0f;
            _hasSound?.Invoke();
        }
    }
}
