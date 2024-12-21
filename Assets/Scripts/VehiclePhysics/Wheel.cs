namespace CustomWheelCollider.VehiclePhysics
{
    using UnityEngine;

    public class Wheel : MonoBehaviour
    {
        [SerializeField] private float wheelRadius = 0.34f;
        [SerializeField] private float wheelInertia = 1.156f;
        [SerializeField] private float suspensionLength = 0.5f;
        [SerializeField] private float springRate = 50000.0f;
        [SerializeField] private float damperRate = 2500.0f;
        [SerializeField] private LayerMask collisionLayers = Physics.AllLayers;

        private Transform cachedTransform = default;
        private Transform visualTransform = default;
        private Rigidbody cachedRigidbody = default;

        private float fixedDeltaTime = 0.02f;

        private Vector3 cachedPosition = Vector3.zero;
        private Vector3 wheelForward = Vector3.zero;
        private Vector3 wheelRight = Vector3.zero;
        private Vector3 wheelUp = Vector3.zero;

        private bool isGrounded = false;
        private RaycastHit hitResult = default;

        private float currentSuspensionLength = 0.0f;
        private float previousSuspensionLength = 0.0f;

        private Vector3 projectedForward = Vector3.zero;
        private Vector3 projectedRight = Vector3.zero;

        private Vector3 worldVelocity = Vector3.zero;
        private Vector2 localVelocity = Vector2.zero;

        private Vector2 slip = Vector2.zero;

        private Vector2 localGravityForce = Vector2.zero;
        private Vector2 localVelocityForce = Vector2.zero;

        private Vector2 localTireForce = Vector2.zero;
        private Vector3 worldTireForce = Vector3.zero;

        private float visualRotation = 0.0f;

        public float SteerAngle { get; set; } = 0.0f;
        public float Load { get; private set; } = 0.0f;
        public float AngularVelocity { get; private set; } = 0.0f;
        public float MotorTorque { get; set; } = 0.0f;
        public float BrakeTorque { get; set; } = 0.0f;

        private void OnEnable()
        {
            cachedTransform = GetComponent<Transform>();
            visualTransform = cachedTransform.GetChild(0);
            cachedRigidbody = cachedTransform.GetComponentInParent<Rigidbody>();

            if (cachedRigidbody == false)
            {
                enabled = false;
                Debug.LogWarning("Disabling wheel, rigidbody not found.");
            }

            currentSuspensionLength = suspensionLength;
        }

        private void FixedUpdate()
        {
            fixedDeltaTime = Time.fixedDeltaTime;

            cachedPosition = cachedTransform.position;
            Quaternion steerRotation = Quaternion.Euler(0.0f, SteerAngle, 0.0f);
            Quaternion combinedRotation = cachedTransform.rotation * steerRotation;
            wheelForward = combinedRotation * Vector3.forward;
            wheelRight = combinedRotation * Vector3.right;
            wheelUp = combinedRotation * Vector3.up;

            isGrounded = Physics.Raycast(cachedPosition, -wheelUp, out hitResult, suspensionLength + wheelRadius, collisionLayers, QueryTriggerInteraction.Ignore);

            previousSuspensionLength = currentSuspensionLength;
            currentSuspensionLength = isGrounded ? hitResult.distance - wheelRadius : suspensionLength;
            Load = ((suspensionLength - currentSuspensionLength) * springRate) + (((previousSuspensionLength - currentSuspensionLength) / fixedDeltaTime) * damperRate);
            Load = Load > 0 ? Load : 0;
            cachedRigidbody.AddForceAtPosition(hitResult.normal * Load, cachedPosition);

            projectedForward = Vector3.Cross(hitResult.normal, -wheelRight);
            projectedRight = Vector3.Cross(hitResult.normal, wheelForward);

            worldVelocity = cachedRigidbody.GetPointVelocity(cachedPosition);
            localVelocity.x = Vector3.Dot(worldVelocity, projectedRight);
            localVelocity.y = Vector3.Dot(worldVelocity, projectedForward);

            slip.x = -(localVelocity.x);
            slip.y = -(localVelocity.y - AngularVelocity * wheelRadius);

            AngularVelocity = AngularVelocity + MotorTorque / wheelInertia * fixedDeltaTime;

            float absAngularSlip = (slip.y >= 0 ? slip.y : -slip.y) / wheelRadius;
            float frictionTorque = -Mathf.Clamp(localVelocityForce.y, -Mathf.Abs(localTireForce.y), Mathf.Abs(localTireForce.y)) * wheelRadius;
            AngularVelocity = AngularVelocity + Mathf.Clamp(frictionTorque / wheelInertia * fixedDeltaTime, -absAngularSlip, absAngularSlip);

            float absAngularVelocity = (AngularVelocity >= 0 ? AngularVelocity : -AngularVelocity);
            float absBrakeTorque = BrakeTorque > 0 ? BrakeTorque : -BrakeTorque;
            float signedBrakeTorque = -absBrakeTorque * Mathf.Sign(AngularVelocity);
            AngularVelocity = AngularVelocity + Mathf.Clamp(signedBrakeTorque / wheelInertia * fixedDeltaTime, -absAngularVelocity, absAngularVelocity);

            float absBrakeForce = absBrakeTorque / wheelRadius;
            float dotProduct = Vector3.Dot(hitResult.normal, -Physics.gravity.normalized);
            Vector3 gravityForce = -Physics.gravity.normalized * (dotProduct > 1E-5f ? Load / dotProduct : 10000.0f);
            localGravityForce.x = Vector3.Dot(gravityForce, projectedRight);
            localGravityForce.y = Mathf.Clamp(Vector3.Dot(gravityForce, projectedForward), -absBrakeForce, absBrakeForce);

            localVelocityForce = slip * ((Load / Physics.gravity.magnitude) / fixedDeltaTime);

            localTireForce = localVelocityForce + localGravityForce;
            localTireForce = Vector3.ClampMagnitude(localTireForce, Load);

            worldTireForce = projectedForward * localTireForce.y + projectedRight * localTireForce.x;
            cachedRigidbody.AddForceAtPosition(worldTireForce, cachedPosition);

            visualTransform.localPosition = new Vector3(0.0f, -currentSuspensionLength, 0.0f);
            visualRotation = Mathf.Repeat(visualRotation + AngularVelocity * Mathf.Rad2Deg * fixedDeltaTime, 360.0f);
            visualTransform.localRotation = Quaternion.Euler(visualRotation, SteerAngle, 0.0f);
        }
    }
}