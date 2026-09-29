using UnityEngine;

public class Movimiento : MonoBehaviour
{
    public float speed = 5f;
    public Transform cameraTransform;
    private Rigidbody rb;
    private Vector3 checkpointActual;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        checkpointActual = transform.position;
    }

    void Update()
    {
        Seguimiento();
    }

    void Seguimiento()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 movement = (camForward * moveZ + camRight * moveX).normalized * speed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
    }

    public void EstablecerCheckpoint(Vector3 posicion)
    {
        checkpointActual = posicion;
    }

    public void ReiniciarEnCheckpoint()
    {
        rb.linearVelocity = Vector3.zero;
        transform.position = checkpointActual;
        Debug.Log("Jugador regresado al Checkpoint");
    }
}