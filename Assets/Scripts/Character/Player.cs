using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;
    [SerializeField] private Headbob headbob;
    [SerializeField] private Footstep footstep;

    private void Start()
    {        
        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());
        cameraSpring.Initialize();
        cameraLean.Initialize();
        headbob.Initialize();
        footstep.Initialize(headbob, playerCharacter);
    }

    private void Update()
    {
        CharacterInput characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = InputManager.Instance.GetMovementInput(),
            Jump = InputManager.Instance.GetJumpInput(),
            Crouch = InputManager.Instance.GetCrouchInput() ? CrouchInput.Toggle : CrouchInput.None,
            Sprint = InputManager.Instance.GetSprintInput()
        };
        
        float deltaTime = Time.deltaTime;
        
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);
        
        headbob.UpdateHeadbob(deltaTime, playerCharacter.IsSprinting(), playerCharacter.IsCrouching(), playerCharacter.IsGrounded(), characterInput.Move.magnitude > 0);

        PlayerState.Instance.SetMovementState(
            isMoving: characterInput.Move.magnitude > 0.1f,
            isSprinting: characterInput.Sprint,
            isCrouching: characterInput.Crouch == CrouchInput.Toggle
        );
    }

    private void LateUpdate()
    {
        float deltaTime = Time.deltaTime;
        Transform cameraTarget = playerCharacter.GetCameraTarget();
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        cameraLean.UpdateLean(deltaTime, playerCharacter.GetState().Acceleration, cameraTarget.up);
        
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            CameraInput cameraInput = new CameraInput { Look = Mouse.current.delta.ReadValue() };
            playerCamera.UpdateRotation(cameraInput);    
        }
        
        playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());
        footstep.UpdateFootstep(deltaTime, playerCamera.transform.position);
    }
    
}
