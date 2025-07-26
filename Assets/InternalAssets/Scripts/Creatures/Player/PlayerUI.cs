using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Health _health;
    [SerializeField] private Sprite _enabledHealth;
    [SerializeField] private Sprite _disabledHealth;
    [SerializeField] private Image[] _healthSprites;

    private void Start()
    {
        _health.HasChanged += SetHealth;
    }

    private void OnDestroy()
    {
        _health.HasChanged -= SetHealth;
    }

    private void SetHealth(int health)
    {
        for (int i = 0; i < _healthSprites.Length; i++)
        {
            if (i < health)
            {
                _healthSprites[i].sprite = _enabledHealth;
            }
            else
            {
                _healthSprites[i].sprite = _disabledHealth;
            }
        }
    }
}