using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

//movement states
public enum Status {idle, moving, crouching, vaulting, ledgeGrabbing, ledgeClimbing, sliding}

public class PlayerController : MonoBehaviour
{
    public Status currentStatus;

    [Header("Layer masks")]
    public LayerMask ledgeLayer;

    [Header("stats")]
    float height = 2;
    float radius = 0.35f;
    public float rayRange;
    public bool crouch;
    bool sprint; //not used yet

    //private vectors
    Vector3 pushFrom;
    Vector3 rotatePos;

    [Header("references")]
    public Transform leftRay;
    public Transform rightRay;
    public Transform orientation;
    InputManager inputManager;
    PlayerMovement movement;

    [Header("Ledge grab properties")]
    public float maxFallVelocity;
    public float climbUpForce;
    public float climbForwardForce;
    public bool isGrabbing;
    bool canGrabLedge;
    

    private void Start()
    {
        inputManager = GetComponent<InputManager>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        //looping through all movement checks
        LedgeCheck();
        CheckCrouching();

        //one special check for deafult movement
        if (currentStatus == Status.moving)
        {
            movement.Jump();
            movement.Crouching(crouch);
        } 
        
        //the only one movement script in Update() method due to the order in which Update and FixedUpdate are performed
        //going to be fixed soon
        if (currentStatus == Status.ledgeGrabbing) LedgeGrabMovement();
    }

    private void FixedUpdate()
    {
        //status check and movement methods execution
        switch (currentStatus)
        {
            case Status.ledgeGrabbing: isGrabbing = true;
                return;
            case Status.ledgeClimbing: LedgeClimbMovement();
                return;
            default: DeafultMovement();
                return;
        }   
    }

    //check methods
    void CheckSliding()
    {
    }

    void CheckCrouching()
    {
        if (Input.GetKey(KeyCode.LeftControl)) crouch = true;
        else crouch = false;
    }

    void CheckVaulting()
    {

    }

    void CheckLedgeGrab()
    {
        Vector3 dir = orientation.transform.TransformDirection(new Vector3(0, -0.5f, 1).normalized);
        Vector3 pos = transform.position + (Vector3.up * height / 3f);
        bool right = Physics.Raycast(rightRay.position, dir, radius + rayRange, ledgeLayer);
        bool left = Physics.Raycast(leftRay.position, dir, radius + rayRange, ledgeLayer);

        Debug.DrawRay(rightRay.position, dir * (radius + rayRange));
        Debug.DrawRay(leftRay.position, dir * (radius + rayRange));
        Debug.DrawRay(pos, dir * (radius + rayRange));

        if (left) Debug.Log("Left working");
        if (right) Debug.Log("Right working");

        RaycastHit hit;
        if (Physics.Raycast(pos, dir, out hit, radius + rayRange, ledgeLayer) && right && left)
        {               
             rotatePos = transform.InverseTransformDirection(hit.point);

            Debug.Log(hit.point + " " + rotatePos.x + " " + rotatePos.z);
            rotatePos.x = 0; rotatePos.z = 1;
            Debug.Log(hit.point + " " + rotatePos.x + " " + rotatePos.z);

            pushFrom = transform.position + transform.TransformDirection(rotatePos);
            Debug.Log(pushFrom + " " + transform.TransformDirection(rotatePos));

            rotatePos.z = radius * 2f;

            //Vector3 checkCollisions = transform.position + transform.TransformDirection(rotatePos);

            currentStatus = Status.ledgeGrabbing;
            if(currentStatus != Status.ledgeGrabbing) Debug.Log("status = grabbing");            
        }
    }

    void CheckLedgeClimb()
    {

    }

    void LedgeCheck()
    {
        if ((movement.isGrounded)  || movement.rb.velocity.y > 0)
            canGrabLedge = true;

        if(currentStatus != Status.ledgeClimbing)
        {
            if(canGrabLedge && !movement.isGrounded)
            {
                
                if(movement.rb.velocity.y > maxFallVelocity)
                {
                    CheckLedgeGrab();
                    Debug.Log("sprawdzam grab");
                }
            }
            if(currentStatus == Status.ledgeGrabbing)
            {
                canGrabLedge = false;
                if (inputManager.forwardMoveWS == -1)
                {
                    currentStatus = Status.moving;
                    movement.UnfreezePlayer();
                    if(currentStatus != Status.moving)
                    return;
                }
                    
                else if(inputManager.forwardMoveWS == 1)
                {
                   currentStatus = Status.ledgeClimbing;
                }
     
            }
        }
    }


    //movement methods
    void DeafultMovement()
    {
        movement.PlayerMove(sprint, crouch, false);
    }
    void VaultMovement()
    {

    }
    void LedgeGrabMovement()
    {  
        movement.FreezePlayer(); 
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("jump off");
            movement.LedgeJumpOff(10);
            currentStatus = Status.moving;          
        }
    }
    void LedgeClimbMovement()
    {
        Vector3 direction = pushFrom - transform.position;
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
        Vector3 move = Vector3.Cross(direction, right).normalized;

        movement.ClimbMovement(move);
        if(new Vector2(direction.x, direction.z).magnitude < 0.125f)
        {
            currentStatus = Status.moving;
        }

        //if(grabDirection)currentStatus = Status.moving;

    }
    void SlideMovement()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 0.05f);
        Gizmos.DrawSphere(rotatePos, .1f);
        Gizmos.DrawSphere(transform.TransformDirection(rotatePos), .1f);
        Gizmos.DrawSphere(pushFrom, .1f);
        

    }
}
