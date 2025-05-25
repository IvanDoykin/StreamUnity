using UnityEditor;
using UnityEngine;

public class MatrixWindow : EditorWindow
{
    private DungeonGenerator dungeonGenerator;
    private Vector2 scrollPosition;

    // ������� ���� ����� ����
    [MenuItem("Tools/Matrix Viewer")]
    public static void ShowWindow()
    {
        var window = GetWindow<MatrixWindow>("Matrix Viewer");
        window.minSize = new Vector2(400, 400);
    }

    private void OnGUI()
    {
        // ���� ��� �������������� ������� DungeonGenerator
        dungeonGenerator = (DungeonGenerator)EditorGUILayout.ObjectField(
            "Dungeon Generator",
            dungeonGenerator,
            typeof(DungeonGenerator),
            true
        );

        if (dungeonGenerator == null || dungeonGenerator.Map == null)
        {
            EditorGUILayout.HelpBox("No DungeonGenerator selected or matrix is empty.", MessageType.Info);
            return;
        }

        // ���������, ���� ������� �������
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField($"Matrix {dungeonGenerator.Map.GetLength(0)} x {dungeonGenerator.Map.GetLength(1)}");

        // ��������� � ���������� �����������
        for (int x = dungeonGenerator.Map.GetLength(1) - 1; x >= 0; x--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int y = 0; y < dungeonGenerator.Map.GetLength(0); y++)
            {
                // ���������� [y,x] ����� ��������� ����������
                EditorGUILayout.Toggle(dungeonGenerator.Map[y, x], GUILayout.Width(15));
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // ������ ��� ����������
        if (GUILayout.Button("Refresh"))
        {
            Repaint(); // ������������ ����
        }
    }
}