using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSoundEmitter : MonoBehaviour
{
    public float whistleRadius = 15f;
    public Key whistleKey = Key.H;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[whistleKey].wasPressedThisFrame)
        {
            SoundManager.Instance.EmitSound(transform.position, whistleRadius);
        }
    }
}