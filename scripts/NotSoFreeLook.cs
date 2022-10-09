using UnityEngine;
using Cinemachine;

public class NotSoFreeLook : MonoBehaviour, AxisState.IInputAxisProvider
{
    public string HorizontalInput = "Mouse X";
    public string VerticalInput = "Mouse Y";

    public float GetAxisValue(int axis)
    {
        if (SimpleGameManager.Instance.GetGameState() == SimpleGameState.Playing)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                Cursor.lockState = CursorLockMode.None;
                return 0;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        switch (axis)
        {
            case 0: return Input.GetAxis(HorizontalInput);
            case 1: return Input.GetAxis(VerticalInput);
            default: return 0;
        }
    }
}