using UnityEngine;

public class PlayerMovement : MonoBehaviour, IMovement
{
    [Range(20f, 50f)]
    [SerializeField] private float _speed = 25f;

    private IPlayerInput _input;
    private Rigidbody2D _body;

    public Vector2 Velocity => _movement;
    private Vector2 _movement = Vector2.zero;

    private void Awake()
    {
        _input = new PlayerKeyboardInput();
        _body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _movement.x = _input.GetHorizontal();
        _movement.y = _input.GetVertical();
    }

    private void FixedUpdate()
    {
        _body.velocity = Time.fixedDeltaTime * _speed * 10f * _movement.normalized;
    }
}
