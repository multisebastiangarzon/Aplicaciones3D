using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraOrbita : MonoBehaviour
{
    public Transform target;
    public float distance = 8f;
    public float height = 2f;
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 60f;
    public float smoothSpeed = 30f;

    private const float factorMouse = 0.1f;

    private float currentYaw = 0f;
    private float currentPitch = 20f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Cursor.lockState = CursorLockMode.None;

        
        if (Cursor.lockState != CursorLockMode.Locked &&
            Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            currentYaw += delta.x * mouseSensitivity * factorMouse;
            currentPitch -= delta.y * mouseSensitivity * factorMouse;
            currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        }

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desiredPosition = target.position - (rotation * Vector3.forward * distance) + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * (height * 0.5f));
    }
}