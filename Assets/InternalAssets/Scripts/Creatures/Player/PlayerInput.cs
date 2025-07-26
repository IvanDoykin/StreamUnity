using UnityEngine;

public class PlayerKeyboardInput : IPlayerInput
{
    public float GetHorizontal()
    {
        if (Input.GetKey(KeyCode.A))
        {
            return -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            return 1;
        }

        return 0;
    }

    public float GetVertical()
    {
        if (Input.GetKey(KeyCode.S))
        {
            return -1;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            return 1;
        }

        return 0;
    }
}