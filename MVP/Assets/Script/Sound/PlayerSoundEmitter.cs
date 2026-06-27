using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSoundEmitter : MonoBehaviour
{
    [Header("Whistle")]
    public float whistleRadius = 15f;

    public void EmitPushAwayWhistle()
    {
        Debug.Log("Emit PushAway Whistle");
        SoundManager.Instance.EmitWhistle(
            transform.position,
            whistleRadius,
            WhistleType.PushAway
        );
    }

    public void EmitLureWhistle()
    {
        Debug.Log("Emit Lure Whistle");
        SoundManager.Instance.EmitWhistle(
            transform.position,
            whistleRadius,
            WhistleType.Lure
        );
    }

    public void EmitAlertWhistle()
    {
        Debug.Log("Emit Alert Whistle");
        SoundManager.Instance.EmitWhistle(
            transform.position,
            whistleRadius,
            WhistleType.Alert
        );
    }
}