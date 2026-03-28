using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {  get; private set; }

    private PlayerInputActions actions;

    //Sprint
    private bool localSprint = false;
    private float sprintTimer = 0f;
    private const float sprintDelay = 0.15f;

    private void Start()
    {
        Instance = this;
        actions = new PlayerInputActions();
        actions.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        SprintTimer();

        //DEBUG
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            ToggleCursorLock();
        }
    }

    public static void ToggleCursorLock()
    {
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = !Cursor.visible;
    }

    private void SprintTimer()
    {
        if (actions.Player.Sprint.IsInProgress())
        {
            sprintTimer += Time.deltaTime;
            if (sprintTimer >= sprintDelay)
            {
                localSprint = true;
            }
        }
        else
        {
            sprintTimer = 0f;
            localSprint = false;
        }
    }

    public Vector2 GetMovementInput() => actions.Player.Move.ReadValue<Vector2>();

    public Vector2 GetMouseDelta() => actions.Player.Look.ReadValue<Vector2>();

    public bool GetJumpInput() => actions.Player.Jump.WasPressedThisFrame();

    public bool GetCrouchInput() => actions.Player.Crouch.WasPressedThisFrame();

    public bool GetSprintInput() => localSprint;

    public bool InteractKeyPressed() => actions.Player.Interact.WasPressedThisFrame();

    public bool DropKeyPressed() => actions.Player.Drop.WasPressedThisFrame();

    /// <summary>Sol tık bu frame basıldı mı? (Kullanımı başlat)</summary>
    public bool GetUseInputDown()
        => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

    /// <summary>Sol tık bu frame bırakıldı mı? (Kullanımı durdur)</summary>
    public bool GetUseInputUp()
        => Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;

    /// <summary>Sol tık şu an basılı tutuluyor mu?</summary>
    public bool GetUseInput()
        => Mouse.current != null && Mouse.current.leftButton.isPressed;
}
