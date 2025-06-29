using UnityEngine;

public class Bootloader : MonoBehaviour
{
    [SerializeField] private DungeonGenerator _generator;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _generator.Generate();
    }
}