using System.Collections;
using UnityEngine;

public class Sword : MonoBehaviour, IEquipment
{
    private const float _startDegree = -30f;
    private const float _upPhaseTime = 0.1f;
    private const float _downPhaseTime = 0.05f;
    private const float _returnPhaseTime = 0.1f;

    [SerializeField] private Collider2D _collider;

    private bool _isAttack = false;

    private void Start()
    {
        transform.localEulerAngles = new Vector3(0f, 0f, _startDegree);
        _collider.enabled = false;
    }

    public void Activate()
    {
        if (_isAttack)
        {
            return;
        }

        _isAttack = true;
        StartCoroutine(ActivateInTime());
    }

    private IEnumerator ActivateInTime()
    {
        _collider.enabled = true;

        float degreePerSecond = -_startDegree / _upPhaseTime;
        float timer = 0f;

        while (timer < _upPhaseTime)
        {
            transform.localEulerAngles += new Vector3(0f, 0f, degreePerSecond * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localEulerAngles = new Vector3(0f, 0f, 0f);

        degreePerSecond = -180f / _downPhaseTime;
        timer = 0f;

        while (timer < _downPhaseTime)
        {
            transform.localEulerAngles += new Vector3(0f, 0f, degreePerSecond * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localEulerAngles = new Vector3(0f, 0f, -180f);

        degreePerSecond = (_startDegree + 180f) / _returnPhaseTime;
        timer = 0f;

        while (timer < _returnPhaseTime)
        {
            transform.localEulerAngles += new Vector3(0f, 0f, degreePerSecond * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localEulerAngles = new Vector3(0f, 0f, _startDegree);

        _collider.enabled = false;
        _isAttack = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var health = collision.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(5);
        }
    }
}