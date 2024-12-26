namespace CustomWheelCollider.VehiclePhysics
{
    using CustomWheelCollider.Input;
    using UnityEngine;

    public class VehicleController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Wheel wheelFL = default;
        [SerializeField] private Wheel wheelFR = default;
        [SerializeField] private Wheel wheelRL = default;
        [SerializeField] private Wheel wheelRR = default;

        [Header("Settings")]
        [SerializeField] private AnimationCurve motorTorqueCurve = default;
        [SerializeField] private AnimationCurve reverseTorqueCurve = default;

        [SerializeField] private float maxSteerAngle = 40.0f;

        private Rigidbody cachedRigidbody = default;

        private int driveDirection = 0;
        private float previousBrakeInput = 0.0f;

        private float totalMotorTorque = 0.0f;
        private float totalBrakeTorque = 0.0f;

        private float velocity = 0.0f;
        private float absVelocity = 0.0f;

        private float wheelBase = 0.0f;
        private float wheelTrack = 0.0f;

        private float steerAngle = 0.0f;

        private void OnEnable()
        {
            cachedRigidbody = GetComponentInParent<Rigidbody>();

            if (cachedRigidbody == null)
            {
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            float throttleInput = InputManager.Instance.Throttle;
            float brakeInput = InputManager.Instance.Brake;
            float combinedInput = throttleInput - brakeInput;

            switch (driveDirection)
            {
                case -1:

                    totalMotorTorque = -brakeInput * reverseTorqueCurve.Evaluate(absVelocity);
                    totalBrakeTorque = throttleInput * 8000.0f;

                    if (absVelocity < 0.1f && brakeInput == 0)
                    {
                        driveDirection = 0;
                    }

                    break;

                case 0:

                    // This ensures the brake needs to be released and then pressed again to make the car go in reverse.
                    if (InputManager.Instance.BrakeStarted)
                    {
                        driveDirection = -1;
                    }

                    if (throttleInput > 0)
                    {
                        driveDirection = 1;
                    }

                    break;

                case 1:

                    totalMotorTorque = throttleInput * motorTorqueCurve.Evaluate(absVelocity);
                    totalBrakeTorque = brakeInput * 8000.0f;

                    if (absVelocity < 0.1f && throttleInput == 0)
                    {
                        driveDirection = 0;
                    }

                    break;

                default:
                    driveDirection = 0;
                    break;
            }

            previousBrakeInput = brakeInput;

            // steering

            steerAngle = maxSteerAngle * InputManager.Instance.Steering;
        }

        private void FixedUpdate()
        {
            velocity = Vector3.Dot(cachedRigidbody.linearVelocity, cachedRigidbody.transform.forward) * 3.6f;
            absVelocity = velocity >= 0 ? velocity : -velocity;

            wheelFL.MotorTorque = totalMotorTorque * 0.25f;
            wheelFR.MotorTorque = totalMotorTorque * 0.25f;
            wheelRL.MotorTorque = totalMotorTorque * 0.25f;
            wheelRR.MotorTorque = totalMotorTorque * 0.25f;

            wheelFL.BrakeTorque = totalBrakeTorque * 0.25f;
            wheelFR.BrakeTorque = totalBrakeTorque * 0.25f;
            wheelRL.BrakeTorque = totalBrakeTorque * 0.25f;
            wheelRR.BrakeTorque = totalBrakeTorque * 0.25f;

            wheelBase = Mathf.Abs(wheelFL.transform.localPosition.z) + Mathf.Abs(wheelRL.transform.localPosition.z);
            wheelTrack = Mathf.Abs(wheelFL.transform.localPosition.x) + Mathf.Abs(wheelFL.transform.localPosition.x);

            float radius = steerAngle != 0.0f ? wheelBase / Mathf.Tan(steerAngle * Mathf.Deg2Rad) : 0.0f;

            float rightWheelAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (radius - (wheelTrack * 0.5f)));
            float leftWheelAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (radius + (wheelTrack * 0.5f)));

            wheelFL.SteerAngle = steerAngle != 0.0f ? leftWheelAngle : 0.0f;
            wheelFR.SteerAngle = steerAngle != 0.0f ? rightWheelAngle : 0.0f;
        }
    }
}