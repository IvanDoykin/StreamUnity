using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    private IPlayerInput _input;

    private void Awake()
    {
        _input = new PlayerKeyboardInput();
    }

    private void Update()
    {
        float horizontal = _input.GetHorizontal();
        float vertical = _input.GetVertical();

        transform.position += Time.deltaTime * _speed * new Vector3(horizontal, vertical, 0f);
    }
}
