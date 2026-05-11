using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SnowSurfer
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        [SerializeField] private ParticleSystem snowTrail;
        [SerializeField] private float keyboardSpeed = 7.5f;
        [SerializeField] private float smoothTime = 0.085f;
        [SerializeField] private float horizontalMargin = 0.65f;
        [SerializeField] private float bottomMargin = 1.25f;
        [SerializeField] private float topMargin = 1.65f;
        [SerializeField] private float tiltAmount = 18f;

        private GameManager gameManager;
        private ScoreManager scoreManager;
        private AudioManager audioManager;
        private Camera mainCamera;
        private Vector3 targetPosition;
        private Vector3 smoothVelocity;
        private Vector2 lastPosition;
        private bool pointerWasDown;

        public void Initialize(GameManager manager, ScoreManager score, AudioManager audio)
        {
            gameManager = manager;
            scoreManager = score;
            audioManager = audio;
            mainCamera = Camera.main;
            targetPosition = transform.position;
        }

        private void Update()
        {
            if (gameManager == null || !gameManager.IsPlaying)
            {
                pointerWasDown = false;
                return;
            }

            ReadInput();
            Vector3 previous = transform.position;
            transform.position = Vector3.SmoothDamp(transform.position, ClampToPlayArea(targetPosition), ref smoothVelocity, smoothTime);

            float xVelocity = (transform.position.x - previous.x) / Mathf.Max(Time.deltaTime, 0.001f);
            if (visual != null)
            {
                Quaternion tilt = Quaternion.Euler(0f, 0f, Mathf.Clamp(-xVelocity * tiltAmount * 0.12f, -tiltAmount, tiltAmount));
                visual.localRotation = Quaternion.Slerp(visual.localRotation, tilt, Time.deltaTime * 12f);
            }
        }

        public void ResetPlayer()
        {
            mainCamera = Camera.main;
            transform.position = new Vector3(0f, -2.55f, 0f);
            targetPosition = transform.position;
            smoothVelocity = Vector3.zero;
            pointerWasDown = false;
            if (visual != null)
            {
                visual.localRotation = Quaternion.identity;
                visual.localScale = Vector3.one;
            }

            if (snowTrail != null)
            {
                snowTrail.Play();
            }
        }

        public void PlayCrash()
        {
            if (snowTrail != null)
            {
                snowTrail.Stop();
            }

            if (visual != null)
            {
                visual.localRotation = Quaternion.Euler(0f, 0f, 58f);
                visual.localScale = Vector3.one * 0.92f;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (gameManager == null || !gameManager.IsPlaying)
            {
                return;
            }

            if (other.TryGetComponent(out Collectible collectible))
            {
                collectible.Collect(scoreManager, audioManager);
                return;
            }

            if (other.TryGetComponent(out Obstacle _))
            {
                audioManager.PlayCrash();
                gameManager.GameOver();
            }
        }

        private void ReadInput()
        {
            Vector2 keyboard = ReadKeyboard();
            if (keyboard.sqrMagnitude > 0.001f)
            {
                targetPosition += (Vector3)(keyboard.normalized * keyboardSpeed * Time.deltaTime);
            }

            if (TryReadPointerWorld(out Vector3 pointerWorld, out bool pointerDown))
            {
                if (!pointerWasDown)
                {
                    lastPosition = pointerWorld;
                    pointerWasDown = true;
                }

                Vector2 delta = (Vector2)pointerWorld - lastPosition;
                targetPosition += (Vector3)delta;
                lastPosition = pointerWorld;
            }
            else if (!pointerDown)
            {
                pointerWasDown = false;
            }
        }

        private Vector3 ClampToPlayArea(Vector3 position)
        {
            if (mainCamera == null)
            {
                return position;
            }

            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * mainCamera.aspect;
            Vector3 cameraPosition = mainCamera.transform.position;
            position.x = Mathf.Clamp(position.x, cameraPosition.x - halfWidth + horizontalMargin, cameraPosition.x + halfWidth - horizontalMargin);
            position.y = Mathf.Clamp(position.y, cameraPosition.y - halfHeight + bottomMargin, cameraPosition.y + halfHeight - topMargin);
            position.z = 0f;
            return position;
        }

        private static Vector2 ReadKeyboard()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
            {
                return Vector2.zero;
            }

            Vector2 value = Vector2.zero;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                value.x -= 1f;
            }
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                value.x += 1f;
            }
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                value.y += 1f;
            }
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                value.y -= 1f;
            }

            return value;
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }

        private bool TryReadPointerWorld(out Vector3 world, out bool pointerDown)
        {
            pointerDown = false;
            world = Vector3.zero;
#if ENABLE_INPUT_SYSTEM
            Vector2 screenPosition = Vector2.zero;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                pointerDown = true;
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                pointerDown = true;
                screenPosition = Mouse.current.position.ReadValue();
            }
#else
            Vector2 screenPosition = Vector2.zero;
            if (Input.touchCount > 0)
            {
                pointerDown = true;
                screenPosition = Input.GetTouch(0).position;
            }
            else if (Input.GetMouseButton(0))
            {
                pointerDown = true;
                screenPosition = Input.mousePosition;
            }
#endif
            if (!pointerDown || mainCamera == null)
            {
                return false;
            }

            world = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(mainCamera.transform.position.z)));
            world.z = 0f;
            return true;
        }
    }
}
