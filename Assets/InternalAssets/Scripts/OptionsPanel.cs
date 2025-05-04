using UnityEngine;

public class OptionsPanel : MonoBehaviour, IPanelAction
{
    private Panel _panel;

    public void Initialize(Panel panel)
    {
        _panel = panel;
    }

    public void StartAction()
    {
        Debug.Log("Options");
    }
}
