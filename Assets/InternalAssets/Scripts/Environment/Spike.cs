using UnityEngine;

public class Spike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var health = collision.GetComponent<Health>();

        if (health != null)
        {
            health.TakeDamage(1);
        }
    }
}
