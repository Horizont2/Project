using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 4.5f;
    public float mouseSensitivity = 15f;
    public float minPitch = -35f;
    public float maxPitch = 70f;
    public Vector3 targetOffset = new Vector3(0, 1.3f, 0);

    private float currentYaw = 0f;
    private float currentPitch = 15f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Mouse.current != null)
        {
            currentYaw += Mouse.current.delta.x.ReadValue() * mouseSensitivity * Time.deltaTime;
            currentPitch -= Mouse.current.delta.y.ReadValue() * mouseSensitivity * Time.deltaTime;
        }

        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        Vector3 targetPosition = target.position + targetOffset;
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 position = targetPosition - (rotation * Vector3.forward * distance);

        transform.position = position;
        transform.rotation = rotation;
    }
}