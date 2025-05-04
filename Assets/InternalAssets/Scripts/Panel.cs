using UnityEngine;

public class Panel : MonoBehaviour
{
    private IPanelAction[] _actions;

    private void Start()
    {
        _actions = GetComponentsInChildren<IPanelAction>();
        foreach (IPanelAction action in _actions)
        {
            action.Initialize(this);
        }
    }
}
