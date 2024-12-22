namespace CustomWheelCollider.Input
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance = default;

        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<InputManager>();

                    if (_instance == null)
                    {
                        GameObject inputManagerGameObject = new GameObject("InputManager");
                        _instance = inputManagerGameObject.AddComponent<InputManager>();
                        DontDestroyOnLoad(inputManagerGameObject);

                        Debug.Log("InputManager instance created.");
                    }
                    else
                    {
                        Debug.LogWarning($"Multiple InputManager instances found. Using the first instance: {_instance.name}");
                    }
                }
                return _instance;
            }
        }

        private InputActions inputActions = default;

        public float Throttle { get; private set; } = 0.0f;
        public float Brake { get; private set; } = 0.0f;
        public float Steering { get; private set; } = 0.0f;
        public float Handbrake { get; private set; } = 0.0f;

        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }

            if (_instance != this)
            {
                Debug.LogWarning("Multiple instances of InputManager found. Destroying the duplicate.");
                Destroy(gameObject);  // Destroy the duplicate instance
            }

            if (inputActions == null)
            {
                inputActions = new InputActions();
            }

            inputActions.Gameplay.Enable();
            SubscribeToInputEvents();
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.Gameplay.Disable();
                UnsubscribeFromInputEvents();
            }
        }

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                UnsubscribeFromInputEvents();
                inputActions.Dispose();
                inputActions = null;  // Clear reference
            }

            if (_instance == this)
            {
                _instance = null;  // Ensure singleton is properly cleaned up
            }
        }

        private void SubscribeToInputEvents()
        {
            inputActions.Gameplay.Throttle.performed += OnThrottlePerformed;
            inputActions.Gameplay.Throttle.canceled += OnThrottleCanceled;

            inputActions.Gameplay.Brake.performed += OnBrakePerformed;
            inputActions.Gameplay.Brake.canceled += OnBrakeCanceled;

            inputActions.Gameplay.Steering.performed += OnSteeringPerformed;
            inputActions.Gameplay.Steering.canceled += OnSteeringCanceled;

            inputActions.Gameplay.Handbrake.performed += OnHandbrakePerformed;
            inputActions.Gameplay.Handbrake.canceled += OnHandbrakeCanceled;
        }

        private void UnsubscribeFromInputEvents()
        {
            inputActions.Gameplay.Throttle.performed -= OnThrottlePerformed;
            inputActions.Gameplay.Throttle.canceled -= OnThrottleCanceled;

            inputActions.Gameplay.Brake.performed -= OnBrakePerformed;
            inputActions.Gameplay.Brake.canceled -= OnBrakeCanceled;

            inputActions.Gameplay.Steering.performed -= OnSteeringPerformed;
            inputActions.Gameplay.Steering.canceled -= OnSteeringCanceled;

            inputActions.Gameplay.Handbrake.performed -= OnHandbrakePerformed;
            inputActions.Gameplay.Handbrake.canceled -= OnHandbrakeCanceled;
        }

        private void OnThrottlePerformed(InputAction.CallbackContext context) => Throttle = context.ReadValue<float>();
        private void OnThrottleCanceled(InputAction.CallbackContext context) => Throttle = 0.0f;

        private void OnBrakePerformed(InputAction.CallbackContext context) => Brake = context.ReadValue<float>();
        private void OnBrakeCanceled(InputAction.CallbackContext context) => Brake = 0.0f;

        private void OnSteeringPerformed(InputAction.CallbackContext context) => Steering = context.ReadValue<float>();
        private void OnSteeringCanceled(InputAction.CallbackContext context) => Steering = 0.0f;

        private void OnHandbrakePerformed(InputAction.CallbackContext context) => Handbrake = 1.0f;
        private void OnHandbrakeCanceled(InputAction.CallbackContext context) => Handbrake = 0.0f;
    }
}