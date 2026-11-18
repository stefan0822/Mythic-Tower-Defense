
using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 30f;
    public float rotationSpeed = 80f;

    [Header("Zoom")]
    public float zoomSpeed = 100f;
    public float minHeight = 10f;
    public float maxHeight = 150f;

    private void Update()
    {
        // WASD - deplasare pe orizontala
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 direction =
            right * horizontal + forward * vertical;

        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        transform.position +=
            direction * moveSpeed * Time.deltaTime;

        // Q / E - rotire
        if (Input.GetKey(KeyCode.Q))
            transform.Rotate(
                Vector3.up,
                -rotationSpeed * Time.deltaTime,
                Space.World
            );

        if (Input.GetKey(KeyCode.E))
            transform.Rotate(
                Vector3.up,
                rotationSpeed * Time.deltaTime,
                Space.World
            );

        // Rotița mouse-ului - zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        Vector3 newPosition =
            transform.position +
            transform.forward * scroll *
            zoomSpeed * Time.deltaTime * 60f;

        newPosition.y = Mathf.Clamp(
            newPosition.y,
            minHeight,
            maxHeight
        );

        transform.position = newPosition;
    }
}
