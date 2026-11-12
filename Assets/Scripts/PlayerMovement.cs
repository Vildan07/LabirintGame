
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float backwardSpeed = 4f;
    [SerializeField] private float rotationSpeed = 240f;

    [Header("Jump and Gravity")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private CharacterController controller;

    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector2 moveInput;

    private float verticalVelocity;
    private float targetYaw;

    private bool wasMovingBackward;

    private static readonly int SpeedHash =
        Animator.StringToHash("Run");

    private static readonly int JumpHash =
        Animator.StringToHash("Jump");

    private static readonly int GroundedHash =
        Animator.StringToHash("IsGrounded");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        targetYaw = transform.eulerAngles.y;

        CreateInputActions();
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
    }

    private void OnDestroy()
    {
        moveAction?.Dispose();
        jumpAction?.Dispose();
    }

    private void Update()
    {
        ReadInput();
        HandleRotation();
        HandleMovement();
        UpdateAnimations();
    }

    private void CreateInputActions()
    {
        moveAction = new InputAction(
            name: "Move",
            type: InputActionType.Value
        );

        // WASD
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // Arrow keys
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        // Gamepad and On-Screen Stick
        moveAction.AddBinding("<Gamepad>/leftStick");

        jumpAction = new InputAction(
            name: "Jump",
            type: InputActionType.Button
        );

        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");
    }

    private void ReadInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();
    }

    private void HandleRotation()
    {
        float forwardInput = moveInput.y;
        float turnInput = moveInput.x;

        bool movingBackward = forwardInput < -0.1f;

        // Turn 180 degrees only when S is first pressed.
        if (movingBackward && !wasMovingBackward)
        {
            targetYaw += 180f;
        }

        wasMovingBackward = movingBackward;

        // A and D turn the player even while standing.
        targetYaw +=
            turnInput * rotationSpeed * Time.deltaTime;

        Quaternion desiredRotation =
            Quaternion.Euler(0f, targetYaw, 0f);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desiredRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void HandleMovement()
    {
        bool grounded = controller.isGrounded;

        if (grounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (grounded && jumpAction.WasPressedThisFrame())
        {
            verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
            {
                animator.ResetTrigger(JumpHash);
                animator.SetTrigger(JumpHash);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        float forwardInput = moveInput.y;

        float currentSpeed = forwardInput < -0.1f
            ? backwardSpeed
            : moveSpeed;

        // Always move in the direction the player faces.
        float movementAmount = Mathf.Abs(forwardInput);

        // Slow movement during the automatic 180-degree turn.
        // Prevents moving too far before facing the destination.
        float angleDifference = Mathf.Abs(
            Mathf.DeltaAngle(
                transform.eulerAngles.y,
                targetYaw
            )
        );

        if (angleDifference > 45f)
        {
            movementAmount = 0f;
        }

        Vector3 horizontalMovement =
            transform.forward *
            movementAmount *
            currentSpeed;

        Vector3 totalMovement =
            horizontalMovement +
            Vector3.up * verticalVelocity;

        controller.Move(totalMovement * Time.deltaTime);
    }

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;

        float movementAmount =
            horizontalVelocity.magnitude > 0.1f
                ? Mathf.Clamp01(
                    horizontalVelocity.magnitude /
                    Mathf.Max(0.01f, moveSpeed)
                )
                : 0f;

        animator.SetFloat(
            SpeedHash,
            movementAmount,
            0.1f,
            Time.deltaTime
        );

        animator.SetBool(
            GroundedHash,
            controller.isGrounded
        );
    }

    // Called after the player passes through a portal.
    public void SyncRotationAfterTeleport()
    {
        targetYaw = transform.eulerAngles.y;

        // Avoid an additional automatic 180-degree turn
        // if the player is holding S.
        wasMovingBackward = moveInput.y < -0.1f;
    }
}
