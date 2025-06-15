using System;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public event Action<int> HasChanged;
    public event Action HasDied;

    [SerializeField] private UnityEvent _hasDamaged;
    [SerializeField] private int _value;
    public int Value => _value;

    public void TakeDamage(int damage)
    {
        _value -= damage;
        _hasDamaged?.Invoke();

        if (_value <= 0)
        {
            HasChanged?.Invoke(0);
            HasDied?.Invoke();
        }
        else
        {
            HasChanged?.Invoke(_value);
        }
    }
}