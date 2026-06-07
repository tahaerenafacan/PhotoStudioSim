using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; }

    public bool IsMoving { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ─────────────────────────────────────────────────────────────
    // API – Sadece PlayerMovement çağırır
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Hareket sisteminin her frame çağıracağı tek metot.
    /// </summary>
    public void SetMovementState(bool isMoving, bool isSprinting, bool isCrouching)
    {
        IsMoving = isMoving;
        IsSprinting = isSprinting;
        IsCrouching = isCrouching;
    }
}