public class Arena
{
    private DungeonGenerator _dungeon;
    private int _counter = 0;

    public Arena(DungeonGenerator dungeon, Enemy[] enemies)
    {
        _dungeon = dungeon;
        _counter = enemies.Length;

        foreach (Enemy enemy in enemies)
        {
            enemy.GetComponent<Health>().HasDied += OnEnemyDie;
        }
    }

    private void OnEnemyDie()
    {
        _counter--;
        if (_counter == 0)
        {
            _dungeon.Generate();
        }
    }
}
