using UnityEngine;
using System.Collections;

namespace Gamewise.crossyroad
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Movement Limits - Mobile")]
        public float mobileMinX = -3f;
        public float mobileMaxX = 3f;
        public float mobileMinZ = -2f;

        [Header("Movement Limits - Tablet")]
        public float tabletMinX = -5f;
        public float tabletMaxX = 5f;
        public float tabletMinZ = -3f;

        private float minX;
        private float maxX;
        private float minZ;

        [Header("Grid Settings")]
        public float moveDistance = 1f;

        [Header("Backward Limit")]
        public int maxBackwardMoves = 3;
        private int backwardMovesUsed = 0;

        [Header("Stone Snap")]
        public string stoneTag = "Stone";
        public bool snapToStoneCenter = true;
        public float stoneSurfaceOffset = 0.02f;
        public Collider playerCollider;

        public bool isJumping = false;
        private bool isOnStone = false;
        private float baseY = 0f;

        public static PlayerManager Instance;

        public GameObject updownButton;

        private CameraFollow cam;

        public Rigidbody rb;

        [Header("Physics Jump")]
        public float horizontalImpulse = 4.5f;
        public float jumpImpulse = 5.5f;

        void Start()
        {
            Instance = this;

            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            cam = Camera.main.GetComponent<CameraFollow>();

            updownButton.SetActive(true);

            SetMovementLimits();
            ResolvePlayerCollider();

            baseY = transform.position.y;
        }

        void Update()
        {
            // ? SAFETY FIX � prevents stuck jumping forever
            if (isJumping && IsGrounded())
            {
                isJumping = false;
            }

            if (isJumping) return;

            if (TryGetKeyboardDirection(out Vector3 direction))
            {
                TryMove(direction);
            }
        }

        void SetMovementLimits()
        {
            bool isTablet = Mathf.Max(Screen.width, Screen.height) >= 2000;

            if (isTablet)
            {
                minX = tabletMinX;
                maxX = tabletMaxX;
                minZ = tabletMinZ;
            }
            else
            {
                minX = mobileMinX;
                maxX = mobileMaxX;
                minZ = mobileMinZ;
            }
        }

        bool CanMove(Vector3 direction)
        {
            Vector3 targetPos = transform.position + direction * moveDistance;

            if (targetPos.x < minX || targetPos.x > maxX)
                return false;

            if (direction == Vector3.back && targetPos.z < minZ)
                return false;

            return true;
        }

        public void MoveUp() => TryMove(Vector3.forward);
        public void MoveDown() => TryMove(Vector3.back);
        public void MoveLeft() => TryMove(Vector3.left);
        public void MoveRight() => TryMove(Vector3.right);

        void TryMove(Vector3 direction)
        {
            if (isJumping) return;
            if (!CanMove(direction)) return;
            if (direction == Vector3.back && !CanMoveBack()) return;

            Jump(direction);
        }

        bool TryGetKeyboardDirection(out Vector3 direction)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            { direction = Vector3.forward; return true; }

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            { direction = Vector3.back; return true; }

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            { direction = Vector3.left; return true; }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            { direction = Vector3.right; return true; }

            direction = Vector3.zero;
            return false;
        }

        void Jump(Vector3 direction)
        {
            isJumping = true;

            if (direction == Vector3.left)
                cam.MoveSide(-1);
            else if (direction == Vector3.right)
                cam.MoveSide(1);
            else
                cam.ResetSide();

            // ? FIXED Rigidbody velocity
            Vector3 vel = rb.linearVelocity;
            vel.x = 0f;
            vel.z = 0f;
            rb.linearVelocity = vel;

            Vector3 impulse =
                direction.normalized * horizontalImpulse +
                Vector3.up * jumpImpulse;

            rb.AddForce(impulse, ForceMode.Impulse);

            UpdateBackMoveCount(direction);
        }

        void OnCollisionEnter(Collision collision)
        {
            string tag = collision.gameObject.tag;

            if (tag == "Car" || tag == "River")
            {
                MenuUiManager.Instance.EndGame();
                return;
            }

            if (tag == stoneTag)
            {
                if (snapToStoneCenter)
                    SnapToStone(collision.collider);

                isOnStone = true;
                isJumping = false;
                return;
            }

            if (IsAllowedLandingSurface(tag))
            {
                isJumping = false;
                isOnStone = false;
                return;
            }
        }

        void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag(stoneTag))
            {
                isOnStone = false;

                if (!isJumping)
                    RestoreYIfNeeded();
            }
        }

        bool IsAllowedLandingSurface(string tag)
        {
            return tag == stoneTag
                || tag == "Grass"
                || tag == "Road"
                || tag == "Rail"
                || tag == "River";
        }

        void SnapToStone(Collider stoneCollider)
        {
            Vector3 center = stoneCollider.bounds.center;
            Vector3 pos = transform.position;

            pos.x = center.x + 0.3f;
            pos.z = center.z;
            transform.position = pos;

            if (playerCollider == null) return;

            float stoneTopY = stoneCollider.bounds.max.y + stoneSurfaceOffset;
            float playerBottomY = playerCollider.bounds.min.y;
            float deltaY = stoneTopY - playerBottomY;

            transform.position += new Vector3(0f, deltaY, 0f);

            isJumping = false;
        }

        void RestoreYIfNeeded()
        {
            if (isOnStone) return;

            Vector3 pos = transform.position;
            pos.y = baseY;
            transform.position = pos;
        }

        void ResolvePlayerCollider()
        {
            if (playerCollider != null) return;

            playerCollider = GetComponent<Collider>();
            if (playerCollider == null)
                playerCollider = GetComponentInChildren<Collider>();
        }

        bool CanMoveBack()
        {
            return maxBackwardMoves > 0 && backwardMovesUsed < maxBackwardMoves;
        }

        void UpdateBackMoveCount(Vector3 direction)
        {
            if (direction == Vector3.back)
                backwardMovesUsed++;
            else if (direction == Vector3.forward)
                backwardMovesUsed = 0;
        }

        // ? Ground check (prevents stuck jump forever)
        bool IsGrounded()
        {
            Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
            return Physics.Raycast(ray, 0.3f);
        }
    }
}
