using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private Transform vehicleModel;
    [SerializeField] private Rigidbody vehicleRigidbody;

    [Header("Vehicle")]
    [SerializeField] private Transform vehicleBody;

    private float speed, speedTarget, _currentSpeed;
    private float rotate, tiltTarget;
    private float rayMaxDistance;

    public Transform VehicleModel { get { return vehicleModel; } set { vehicleModel = value; } }
    public Rigidbody VehicleRigidbody { get { return vehicleRigidbody; } set { vehicleRigidbody = value; } }

    public float Acceleration = 12f;
    public float MaxSpeed = 30f;
    public float BreakSpeed = 1.7f;

    public float MaxSpeedToStartReverse = 150f;
    public float Steering = 80f;
    public float Gravity = 20f;
    public float Drift = 0.85f;
    public float VehicleBodyTilt = 1f;
    public float ForwardTilt = 9.06f;
    public bool TurnInAir = true;
    public bool TurnWhenStationary = true;
    
    public float RotateTarget { get; private set; }
    public bool NearGround { get; private set; }
    public bool OnGround { get; private set; }
    public LayerMask GroundMask;

    public float CurrentSpeed => _currentSpeed;
    public float GetVehicleVelocitySqrMagnitude { get { return VehicleRigidbody.velocity.sqrMagnitude; } }
    public Vector3 GetVehicleVelocity { get { return VehicleRigidbody.velocity; } }
    public bool EngineRunning => true;

    private void Awake()
    {
        SetVehicleSettings();
    }

    public void SetVehicleSettings()
    {
        rayMaxDistance = Vector3.Distance(transform.position, VehicleModel.position) + 1f; // add 0.05f extra to the distance to account for vehicle tilt
    }

    private void Update()
    {
        Accelerate();
    }

    private void FixedUpdate()
    {
        Turn();
        BodyTiltOnMovement();
        GroundVehicle();

        RaycastHit hitNear;

        OnGround = Physics.Raycast(transform.position, Vector3.down, rayMaxDistance, GroundMask);
        NearGround = Physics.Raycast(transform.position, Vector3.down, out hitNear, rayMaxDistance + 1f, GroundMask);

        VehicleModel.up = Vector3.Lerp(VehicleModel.up, hitNear.normal, Time.deltaTime * 8.0f);
        VehicleModel.Rotate(0, transform.eulerAngles.y, 0);

        if (NearGround || OnGround)
        {
            VehicleRigidbody.AddForce(transform.forward * speedTarget, ForceMode.Acceleration);
        }

        Vector3 localVelocity = transform.InverseTransformVector(VehicleRigidbody.velocity);
        localVelocity.x *= 0.9f + (Drift / 10);

        if (NearGround || OnGround) { VehicleRigidbody.velocity = transform.TransformVector(localVelocity); }

        transform.position = VehicleRigidbody.transform.position;
    }

    private void Accelerate()
    {
        speedTarget = Mathf.SmoothStep(speedTarget, speed, Time.deltaTime * Acceleration);
        speed = 0f;
    }

    private void Turn()
    {
        RotateTarget = Mathf.Lerp(RotateTarget, rotate, Time.deltaTime * 4f);
        CalculateTilt(rotate);
        rotate = 0f;

        if (TurnWhenStationary == false && GetVehicleVelocitySqrMagnitude < 0.1f) { return; }

        float yRotation = speedTarget < 0 ? transform.eulerAngles.y - RotateTarget : transform.eulerAngles.y + RotateTarget;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(0, yRotation, 0)), Time.deltaTime * 2.0f);
    }

    private void CalculateTilt(float tilt)
    {
        tiltTarget = Mathf.Lerp(tiltTarget, tilt, Time.deltaTime * 4f);
    }

    private void BodyTiltOnMovement()
    {
        if (TurnWhenStationary == false && GetVehicleVelocitySqrMagnitude < 0.1f) { return; }

        float xRotation = ForwardTilt == 0 ? 0 : speedTarget / ForwardTilt;
        float zRotation = VehicleBodyTilt == 0 ? RotateTarget / 6 : (RotateTarget / 6) * VehicleBodyTilt;

        vehicleBody.localRotation = Quaternion.Slerp(vehicleBody.localRotation, Quaternion.Euler(new Vector3(xRotation, 0, zRotation)), Time.deltaTime * 4f);
    }

    private void GroundVehicle()
    {
        // Keeps vehicle grounded when standing still
        if (speed == 0 && GetVehicleVelocitySqrMagnitude < 4f)
        {
            VehicleRigidbody.velocity = Vector3.Lerp(VehicleRigidbody.velocity, Vector3.zero, Time.deltaTime * 2.0f);
        }
    }

    // Input controls	

    public void ControlAcceleration()
    {
        speed = MaxSpeed;
    }

    public void ControlBrake()
    {
        if (GetVehicleVelocitySqrMagnitude > MaxSpeedToStartReverse)
        {
            speed -= BreakSpeed;
        }
        else
        {
            speed = -MaxSpeed;
        }
    }

    public void ControlTurning(float direction)
    {
        if (NearGround || TurnInAir)
        {
            rotate = Steering * direction;
        }
    }

    public void SetPosition(Vector3 position, Quaternion rotation)
    {
        VehicleRigidbody.velocity = Vector3.zero;
        VehicleRigidbody.angularVelocity = Vector3.zero;
        VehicleRigidbody.position = position;

        speed = speedTarget = rotate = 0.0f;

        VehicleRigidbody.Sleep();
        transform.SetPositionAndRotation(position, rotation);
        VehicleRigidbody.WakeUp();
    }
}