using UnityEngine;
using UnityEngine.InputSystem;

public class CoopInput : MonoBehaviour
{
    public PlayerInput player1Input;
    public PlayerInput player2Input;

    void Start()
    {
        var gamepads = Gamepad.all;
        Debug.Log("Gamepad count: " + gamepads.Count);

        for (int i = 0; i < gamepads.Count; i++)
        {
            Gamepad pad = Gamepad.all[i];

            if (pad.buttonSouth.wasPressedThisFrame)
            {
                Debug.Log(
                    $"Button pressed on Gamepad [{i}], " +
                    $"Device ID: {pad.deviceId}, " +
                    $"Name: {pad.name}"
                );
            }
        }


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
