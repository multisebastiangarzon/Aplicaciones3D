using UnityEngine;

public class PantallaCierre : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
