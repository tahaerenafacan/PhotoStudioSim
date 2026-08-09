using System;
using KinematicCharacterController;
using UnityEngine;

public enum Stance
{
    Stand, Crouch
}

public struct CharacterState
{
    public Vector3 Acceleration;
}
public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool Crouch;
    public bool Sprint;
}
[RequireComponent(typeof(KinematicCharacterMotor))]
public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 3f;
    
    [Header("Inertia")]
    //[SerializeField] private bool useInertia = true;
    [SerializeField] private float walkingInertia = 25f;
    [SerializeField] private float sprintInertia = 10f;
    [SerializeField] private float crouchInertia = 20f;

    [Header("Air Movement")] 
    [Tooltip("Currently overriding by groundSpeed")]
    [SerializeField] private float airSpeed = 8f;
    [SerializeField] private float airAcceleration = 55f;
    
    [Space]
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float gravity = -70f;
    [SerializeField] private float coyoteTime = 0.2f;
    
    [Space]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchHeightResponse = 15f;
    
    [Space]
    [Range(0f, 1f)]
    [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] private float crouchCameraTargetHeight = 0.9f;

    public event Action OnLand;

    private CharacterState state;
    private CharacterState lastState;
    private Stance stance;
    private Quaternion requestedRotation;
    private Vector3 requestedMovement;
    private bool requestedJump;
    private bool ungroundedDueJump;
    private bool requestedCrouch;
    private Collider[] uncrouchOverlapResults;
    private float timeSinceUngrounded;
    private float timeSinceJumpRequested;
    private bool requestedSprint;
    
    public void Initialize()
    {
        stance = Stance.Stand;
        lastState = state;
        uncrouchOverlapResults = new Collider[8];
        motor.CharacterController = this;
    }

    public void UpdateInput(CharacterInput input)
    {
        //TODO: Add inertia to moving
        requestedRotation = input.Rotation;
        
        requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        requestedMovement = Vector3.ClampMagnitude(requestedMovement, 1f);
        requestedMovement = input.Rotation * requestedMovement;
        
        bool wasRequestingJump = requestedJump;
        requestedJump          = requestedJump || input.Jump && stance is Stance.Stand;
        if (requestedJump && !wasRequestingJump)
        {
            timeSinceJumpRequested = 0f;
        }
        
        if (input.Crouch)
        {
            requestedCrouch = !requestedCrouch;
        }

        requestedSprint = input.Sprint && stance is Stance.Stand;

        //Prevent air speed chance mid air
        if (motor.GroundingStatus.IsStableOnGround)
        {
            airSpeed = input.Sprint ? sprintSpeed : walkSpeed;
        }
    }

    public void UpdateBody(float deltaTime)
    {
        float currentHeight = motor.Capsule.height;
        float normalizedHeight = currentHeight / standHeight;
        float cameraTargetHeight = currentHeight * (stance is Stance.Stand ? standCameraTargetHeight : crouchCameraTargetHeight);
        var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);
        cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition, new Vector3(0f, cameraTargetHeight, 0f), 1f - Mathf.Exp(-crouchHeightResponse * deltaTime));
        root.localScale = Vector3.Lerp(root.localScale, rootTargetScale, 1f - Mathf.Exp(-crouchHeightResponse * deltaTime));
    }
    
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        Vector3 forward = Vector3.ProjectOnPlane(requestedRotation * Vector3.forward, motor.CharacterUp);
        if (forward != Vector3.zero)
        {
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        state.Acceleration = Vector3.zero;
        if (motor.GroundingStatus.IsStableOnGround)
        {
            if (timeSinceUngrounded > 0 && ungroundedDueJump)
            {
                OnLand?.Invoke();
            }
            timeSinceUngrounded = 0f;
            ungroundedDueJump = false;
            Vector3 groundedMovement = motor.GetDirectionTangentToSurface(requestedMovement, motor.GroundingStatus.GroundNormal) * requestedMovement.magnitude;
            
            float speed = stance is Stance.Stand ? requestedSprint ? sprintSpeed : walkSpeed : crouchSpeed;
            float inertia = stance is Stance.Stand ? requestedSprint ? sprintInertia : walkingInertia : crouchInertia;
            
            Vector3 targetVelocity = groundedMovement * speed;
            Vector3 moveVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-inertia * deltaTime));
            state.Acceleration = moveVelocity - currentVelocity;
            currentVelocity = moveVelocity;
            
            /*
            Vector3 momentumVelocity = currentVelocity;
            if (useInertia)
            {
                if (requestedMovement.sqrMagnitude > 0f)
                {
                    momentumVelocity = Vector3.Lerp(momentumVelocity, targetVelocity, 1f - Mathf.Exp(-inertia * deltaTime));
                }
                else
                {
                    momentumVelocity = Vector3.Lerp(momentumVelocity, Vector3.zero, 1f - Mathf.Exp(-inertia * deltaTime * 0.5f));
                }
                
            }
            else
            {
                momentumVelocity = targetVelocity;
            }
            
            state.Acceleration = momentumVelocity - currentVelocity;
            currentVelocity = momentumVelocity;
            */
        }
        else //We are in air
        {
            timeSinceUngrounded += deltaTime;
            if (requestedMovement.sqrMagnitude > 0f) // Air Movement
            {
                Vector3 planarMovement = Vector3.ProjectOnPlane(requestedMovement, motor.CharacterUp) * requestedMovement.magnitude;
                Vector3 currentPlanarVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
                Vector3 movementForce = planarMovement * airAcceleration * deltaTime;
                Vector3 targetPlanarVelocity = currentPlanarVelocity + movementForce;
                targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);
                currentVelocity += targetPlanarVelocity - currentPlanarVelocity;
            }
            currentVelocity += motor.CharacterUp * gravity * deltaTime;
        }

        if (requestedJump && stance is not Stance.Crouch)
        {
            bool canCoyoteJump = timeSinceUngrounded < coyoteTime && !ungroundedDueJump;
            
            if (motor.GroundingStatus.IsStableOnGround || canCoyoteJump)
            {
                requestedJump = false;
                motor.ForceUnground(0f);
                ungroundedDueJump = true;
                float currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                float targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }
            else
            {
                timeSinceJumpRequested += deltaTime;
                bool canJumpLater = timeSinceJumpRequested < coyoteTime;
                requestedJump = canJumpLater;
            }
        }
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        if (requestedCrouch && stance is Stance.Stand)
        {
            stance = Stance.Crouch;
            motor.SetCapsuleDimensions(motor.Capsule.radius, crouchHeight, crouchHeight * 0.5f);
        }
    }

    public void PostGroundingUpdate(float deltaTime) { }

    public void AfterCharacterUpdate(float deltaTime)
    {
        
        if (!requestedCrouch && stance is not Stance.Stand)
        {
            motor.SetCapsuleDimensions(motor.Capsule.radius, standHeight, standHeight * 0.5f);
            if (motor.CharacterOverlap(motor.TransientPosition, motor.TransientRotation, uncrouchOverlapResults, motor.CollidableLayers, QueryTriggerInteraction.Ignore) > 0)
            {
                requestedCrouch = true;
                motor.SetCapsuleDimensions(motor.Capsule.radius, crouchHeight, crouchHeight * 0.5f);
            }
            else
            {
                stance = Stance.Stand;
            }
        }
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        state.Acceleration = Vector3.ProjectOnPlane(state.Acceleration, hitNormal);
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

    public void OnDiscreteCollisionDetected(Collider hitCollider) { }

    public Transform GetCameraTarget()
    {
        return cameraTarget;
    }

    public bool IsGrounded()
    {
        return motor.GroundingStatus.IsStableOnGround;
    }

    public bool IsSprinting()
    {
        return stance is Stance.Stand && requestedSprint;
    }

    public bool IsCrouching()
    {
        return stance is Stance.Crouch;
    }

    public CharacterState GetState() => state;
    public CharacterState GetLastState() => lastState;
}
