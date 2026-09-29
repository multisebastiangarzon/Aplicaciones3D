using UnityEngine;

public class CamaraOrbita : MonoBehaviour
{
    public Transform target;              
    public float distance = 8f;
    public float height = 2f;
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 60f;
    public float smoothSpeed = 30f;

    private float currentYaw = 0f;
    private float currentPitch = 20f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
    }

    void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desiredPosition = target.position - (rotation * Vector3.forward * distance) + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * (height * 0.5f));
    }
}