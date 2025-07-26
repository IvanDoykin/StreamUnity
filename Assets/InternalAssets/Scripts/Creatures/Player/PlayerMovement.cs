using UnityEngine;

public class PlayerMovement : MonoBehaviour, IMovement
{
    [SerializeField] private Transform _hand;

    [Range(20f, 50f)]
    [SerializeField] private float _speed = 25f;

    private IPlayerInput _input;
    private Rigidbody2D _body;

    public Vector2 Velocity => _movement;
    private Vector2 _movement = Vector2.zero;

    private void Awake()
    {
        NineSidesSFXPlayer.Instance.Initialize(this);

        _input = new PlayerKeyboardInput();
        _body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_movement.x > 0)
        {
            _hand.transform.localPosition = new Vector3(Mathf.Abs(_hand.transform.localPosition.x), _hand.transform.localPosition.y, _hand.transform.localPosition.z);
            _hand.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        }
        else if (_movement.x != 0)
        {
            _hand.transform.localPosition = new Vector3(-Mathf.Abs(_hand.transform.localPosition.x), _hand.transform.localPosition.y, _hand.transform.localPosition.z);
            _hand.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        }

        _movement.x = _input.GetHorizontal();
        _movement.y = _input.GetVertical();
    }

    private void FixedUpdate()
    {
        _body.velocity = Time.fixedDeltaTime * _speed * 10f * _movement.normalized;
    }
}