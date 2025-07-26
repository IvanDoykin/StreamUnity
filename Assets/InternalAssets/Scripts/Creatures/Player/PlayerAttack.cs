using UnityEngine;

public class PlayerAttack : MonoBehaviour, IAttack
{
    private IEquipment _weapon;
    private IPlayerInput _input;

    private void Awake()
    {
        _weapon = GetComponentInChildren<IEquipment>();
        _input = new PlayerKeyboardInput();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    public void Attack()
    {
        _weapon.Activate();
    }
}