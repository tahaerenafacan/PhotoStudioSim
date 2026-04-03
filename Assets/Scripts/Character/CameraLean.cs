using UnityEngine;

public class CameraLean : MonoBehaviour
{
    [SerializeField] private float attackDamping = 0.5f;
    [SerializeField] private float decayDamping = 0.5f;
    [SerializeField] private float strength = 1.5f;

    private Vector3 dampedAcceleration;
    private Vector3 dampedAccelerationVel;
    public void Initialize()
    {
        
    }

    public void UpdateLean(float deltaTime, Vector3 acceleration, Vector3 up)
    {
        var planarAcceleration = Vector3.ProjectOnPlane(acceleration, up);
        var damping = planarAcceleration.magnitude > dampedAcceleration.magnitude ? attackDamping : decayDamping;
        dampedAcceleration = Vector3.SmoothDamp(dampedAcceleration, planarAcceleration, ref dampedAccelerationVel, damping, float.PositiveInfinity, deltaTime);
        var leanAxis = Vector3.Cross(dampedAcceleration.normalized, up).normalized;
        transform.localRotation = Quaternion.identity;
        transform.rotation = Quaternion.AngleAxis(dampedAcceleration.magnitude * strength, leanAxis) * transform.rotation;
    }
}
