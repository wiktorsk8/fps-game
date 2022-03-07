using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header ("references")]
    public Rigidbody rb;
    public Transform camera_pos;
    public Transform orientation;
    InputManager input;
    CapsuleCollider player_collider;

    [Header("Movement Settings")]
    public float runSpeed;
    public float walkSpeed;
    public float crouchSpeed;
    public float jumpForce = 2f;
    public float maxSpeed;
    public float crouchMaxSpeed;

    [Header("Move vector")]
    public Vector3 playerMove;

    //ledge movement required variables
    bool isGrabbingLedge = false;

    [Header("Layers")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask edgeMask;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private float airDrag;
    [SerializeField] private float drag;
    [SerializeField] public bool isGrounded;

    [Header("Speed limits")]
    public float maxVelocityX;
    public float maxVelocityZ;


    [Header("Slope Handling")]
    private RaycastHit slopeHit;
    private Vector3 slopeMoveDirection;

    

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputManager>();
        player_collider = GetComponent<CapsuleCollider>();
    }

    private void FixedUpdate()
    {
        isGrounded = GroundCheck();
    }

    private void Update()
    {
        HandlingMoveInput();
        HandlingRigidbodyDrag();
    }

    void HandlingMoveInput()
    {
        playerMove = orientation.right * input.sideMoveAD + orientation.forward * input.forwardMoveWS;
    }

    public void PlayerMove(bool sprint, bool isCrouching, bool ledgeGrabStatus)
    {
        isGrabbingLedge = ledgeGrabStatus;
        float speed = walkSpeed;

        if (isCrouching) speed = crouchSpeed;

        if(isGrounded) rb.AddForce(playerMove.normalized * speed, ForceMode.Acceleration);
        RigidbodyVelocityLimiting(isCrouching);
    }

    public void Crouching(bool isCrouching)
    {
        if (isCrouching)
        {
            player_collider.height = 1;
            player_collider.center = new Vector3(0, -0.5f, 0);
            camera_pos.localPosition = new Vector3(0, -0.25f, 0);
        }
        else
        {
            player_collider.height = 2;
            player_collider.center = Vector3.zero;
            camera_pos.localPosition = new Vector3(0, 0.75f, 0);
        }
    }

    public void ClimbMovement(Vector3 dir)
    {
        UnfreezePlayer();
        rb.AddForce(dir);
    }

    public void FreezePlayer()
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void UnfreezePlayer()
    {
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void RigidbodyVelocityLimiting(bool isCrouching)
    {
        float speed;
        if (isCrouching) speed = crouchMaxSpeed;
        else speed = maxSpeed;

        if(rb.velocity.magnitude > maxSpeed)
        {
            Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            Vector3 limitedVelocity = Vector3.ClampMagnitude(velocity, speed);

            rb.velocity = limitedVelocity + Vector3.up * rb.velocity.y;
        } 
    }

    private bool GroundCheck()
    {
        return Physics.CheckSphere(groundCheckPoint.transform.position, groundCheckRadius, groundMask);
    }

    public void Jump()
    {   
        if(isGrounded && Input.GetKeyDown(KeyCode.Space)) rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void LedgeJumpOff(float force) 
    {
        UnfreezePlayer();
        Vector3 jump = (orientation.forward + Vector3.up/2) * force;
        rb.AddForce(jump, ForceMode.Impulse);
    }

    public void MovementVectorReset()
    {
       // movement = Vector3.zero;
    }

    public void VelocityReset()
    {
       // velocity = Vector3.zero;
    }

    private bool isOnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, 1 + 0.8f))
        {
            if (slopeHit.normal != Vector3.up) return true;
        }

        return false;  
    }

    private void HandlingRigidbodyDrag()
    {
        if (!isGrounded) rb.drag = airDrag;
        if (isGrounded) rb.drag = drag;
    }
}
