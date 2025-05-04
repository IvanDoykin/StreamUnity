using UnityEngine;

public class Bootstrap : MonoBehaviour
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