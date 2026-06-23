using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 8f;
    public float acceleration = 20f;
    public float deceleration = 25f;
    public float turnSmoothTime = 0.05f;

    [Header("Jumping & Gravity")]
    public float jumpHeight = 2.5f;
    public float gravity = -15f;
    public float fallMultiplier = 2.5f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundRadius = 0.3f;
    public LayerMask groundMask;
    public Transform mainCamera;

    private CharacterController controller;
    private Vector3 currentVelocity;
    private Vector3 currentMoveVelocity;
    private float turnSmoothVelocity;

    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (mainCamera == null && Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        HandleGrounding();
        HandleMovement();
        HandleJumpingAndGravity();
    }

    private void HandleGrounding()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);

        if (isGrounded && currentVelocity.y < 0)
        {
            currentVelocity.y = -2f;
        }

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        float x = 0f;
        float z = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) x += 1f;
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.wKey.isPressed) z += 1f;
            if (Keyboard.current.sKey.isPressed) z -= 1f;
        }

        Vector3 inputDirection = new Vector3(x, 0f, z).normalized;

        if (inputDirection.magnitude > 0 && mainCamera != null)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Vector3 targetVelocity = moveDirection * maxSpeed;

            currentMoveVelocity = Vector3.Lerp(currentMoveVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            currentMoveVelocity = Vector3.Lerp(currentMoveVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        controller.Move(currentMoveVelocity * Time.deltaTime);
    }

    private void HandleJumpingAndGravity()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            currentVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        float appliedGravity = gravity;
        if (currentVelocity.y < 0 && !isGrounded)
        {
            appliedGravity *= fallMultiplier;
        }

        currentVelocity.y += appliedGravity * Time.deltaTime;
        controller.Move(currentVelocity * Time.deltaTime);
    }
}