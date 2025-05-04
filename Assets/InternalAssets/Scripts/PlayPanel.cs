using UnityEngine;

public class PlayPanel : MonoBehaviour, IPanelAction
{
    private Panel _panel;

    public void Initialize(Panel panel)
    {
        _panel = panel;
    }

    public void StartAction()
    {
        Debug.Log("Play");   
    }
}