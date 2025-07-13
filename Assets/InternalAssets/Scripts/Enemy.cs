using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    private const float MoveInterval = 0.35f;
    public AStarNavigator Navigator { get; private set; }

    [SerializeField] private UnityEvent _hasMoved;

    [SerializeField] private PlayerFinder _playerFinder;
    [SerializeField] private float _snapSharpness = 10f; // –езкость "прит€гивани€"
    [SerializeField] private Health _health;

    private Vector2Int _currentGridPosition;
    private Vector3 _targetWorldPosition;
    private Vector3 _startMovePosition;
    private float _moveTimer = 0f;
    private float _moveProgress = 1f; // 1 = завершено

    private void Start()
    {
        NineSidesSFXPlayer.Instance.AddEnemy(this);

        _health.HasDied += () => Destroy(gameObject);

        _playerFinder.PlayerHasLocated += Navigator.SetTarget;
        _targetWorldPosition = transform.position;
        _startMovePosition = transform.position;
    }

    private void OnDestroy()
    {
        NineSidesSFXPlayer.Instance.RemoveEnemy(this);

        _playerFinder.PlayerHasLocated -= Navigator.SetTarget;
    }

    public void Initialize(AStarNavigator navigator, Vector2Int currentGridPosition)
    {
        Navigator = navigator;
        _currentGridPosition = currentGridPosition;
    }

    private void Update()
    {
        _moveTimer += Time.deltaTime;
        if (_moveTimer >= MoveInterval && _moveProgress >= 1f)
        {
            _moveTimer = 0f;
            CalculateNextStep();
        }

        if (_moveProgress < 1f)
        {
            _moveProgress = Mathf.Clamp01(_moveProgress + Time.deltaTime / MoveInterval);
            float snapProgress = SnapInterpolation(_moveProgress, _snapSharpness);
            transform.position = Vector3.Lerp(_startMovePosition, _targetWorldPosition, snapProgress);
        }
    }

    private void CalculateNextStep()
    {
        _startMovePosition = _targetWorldPosition;
        Vector2Int nextStep = Navigator.GetNextStep(_currentGridPosition);

        if (nextStep != _currentGridPosition)
        {
            _hasMoved?.Invoke();
            _currentGridPosition = nextStep;
            _targetWorldPosition = CellMath.ConvertCellToWorldPosition(_currentGridPosition);
            _moveProgress = 0f;
        }

        else
        {
            var objects = Physics2D.OverlapCircleAll(transform.position, 2f);
            foreach (var obj in objects)
            {
                var player = obj.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    player.GetComponent<Health>().TakeDamage(1);
                    break;
                }
            }
        }
    }

    private float SnapInterpolation(float t, float sharpness)
    {
        // Snap-интерпол€ци€: быстро "прит€гиваетс€" к цели в конце движени€
        return 1f - Mathf.Pow(1f - t, sharpness);
    }
}