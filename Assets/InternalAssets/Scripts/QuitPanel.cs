using UnityEngine;

public class QuitPanel : MonoBehaviour, IPanelAction
{
    private Panel _panel;

    public void Initialize(Panel panel)
    {
        _panel = panel;
    }

    public void StartAction()
    {
        Debug.Log("Quit");
        _panel.gameObject.SetActive(false);
    }
}
