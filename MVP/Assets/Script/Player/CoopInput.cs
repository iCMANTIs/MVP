using UnityEngine;
using UnityEngine.InputSystem;

public class CoopInput : MonoBehaviour
{
    public PlayerInput player1Input;
    public PlayerInput player2Input;

    void Start()
    {
        var gamepads = Gamepad.all;

        if (gamepads.Count >= 1)
        {
            player1Input.SwitchCurrentControlScheme(
                "Gamepad",
                gamepads[0]
            );

            Debug.Log("Player 1 assigned to Gamepad 1");
        }

        if (gamepads.Count >= 2)
        {
            player2Input.SwitchCurrentControlScheme(
                "Gamepad",
                gamepads[1]
            );

            Debug.Log("Player 2 assigned to Gamepad 2");
        }
    }
}
