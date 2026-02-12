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

        [Header("Jump Settings")]
        public float jumpHeight = 0.2f;
        public float jumpDuration = 0.10f;

        [Header("Stone Snap")]
        public string stoneTag = "Stone";
        public bool snapToStoneCenter = true;
        public float stoneSurfaceOffset = 0.02f;
        public Collider playerCollider;

        [Header("Obstacle Detection")]
        public string[] obstacleTags = { "Obstacle", "Tree", "Rock" }; // Add your obstacle tags here
        public float raycastDistance = 1.1f; // Slightly more than moveDistance

        public bool isJumping = false;
        private bool isOnStone = false;
        private float baseY = 0f;
        public static PlayerManager Instance;

        public GameObject updownButton;

        private CameraFollow cam;
        private Collider pendingStone;


        public Rigidbody rb;

        [Header("Physics Jump")]
        public float horizontalImpulse = 4.5f;
        public float jumpImpulse = 5.5f;


        void Start()
        {
            Instance = this;
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;




            cam = Camera.main.GetComponent<CameraFollow>();
            updownButton.SetActive(true);
            SetMovementLimits();
            ResolvePlayerCollider();
            baseY = transform.position.y;
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


            bool IsObstacleInDirection(Vector3 direction)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, direction, raycastDistance, ~0, QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit in hits)
            {
                // Ignore own colliders.
                if (hit.collider.transform.root == transform)
                    continue;

                if (IsObstacleTagInHierarchy(hit.collider.transform))
                {
                    return true;
                }
            }

            return false;
        }

        bool IsObstacleTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || obstacleTags == null || obstacleTags.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < obstacleTags.Length; i++)
            {
                if (tag == obstacleTags[i])
                {
                    return true;
                }
            }

            return false;
        }

        bool IsObstacleTagInHierarchy(Transform start)
        {
            Transform current = start;
            while (current != null)
            {
                if (IsObstacleTag(current.tag))
                {
                    return true;
                }
                current = current.parent;
            }

            return false;
        }




        void Update()
        {
            if (isJumping) return;

            if (TryGetKeyboardDirection(out Vector3 direction))
            {
                TryMove(direction);
            }
        }

        public void MoveUp()
        {
            TryMove(Vector3.forward);
        }

        public void MoveDown()
        {
            TryMove(Vector3.back);
        }

        public void MoveLeft()
        {
            TryMove(Vector3.left);
        }

        public void MoveRight()
        {
            TryMove(Vector3.right);
        }

        void TryMove(Vector3 direction)
        {
            if (isJumping) return;
            if (!CanMove(direction)) return;
            if (direction == Vector3.back && !CanMoveBack()) return;
            if (IsObstacleInDirection(direction)) return;

            // StartCoroutine(Jump(direction));
            Jump(direction);
        }

        bool TryGetKeyboardDirection(out Vector3 direction)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                direction = Vector3.forward;
                return true;
            }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                direction = Vector3.back;
                return true;
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                direction = Vector3.left;
                return true;
            }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                direction = Vector3.right;
                return true;
            }

            direction = Vector3.zero;
            return false;
        }

        // IEnumerator Jump(Vector3 direction)
        void Jump(Vector3 direction)
        {
            isJumping = true;

            if (direction == Vector3.left)
                cam.MoveSide(-1);
            else if (direction == Vector3.right)
                cam.MoveSide(1);
            else
                cam.ResetSide();


            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + direction * moveDistance;

            // float elapsed = 0f;

            // while (elapsed < jumpDuration)
            // {
            //     elapsed += Time.deltaTime;
            //     float t = elapsed / jumpDuration;

            //     // Horizontal movement
            //     Vector3 horizontal = Vector3.Lerp(startPos, endPos, t);

            //     // Vertical jump arc
            //     float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;

            //     transform.position = new Vector3(
            //         horizontal.x,
            //         startPos.y + height,
            //         horizontal.z
            //     );

            //     yield return null;
            // }

            // transform.position = endPos;


            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

            // Apply impulse
            Vector3 impulse =
                direction.normalized * horizontalImpulse +
                Vector3.up * jumpImpulse;

            rb.AddForce(impulse, ForceMode.Impulse);
            TrySnapToPendingStone();
            RestoreYIfNeeded();
            cam.ResetSide();
            // Invoke(nameof(ResetJump), 0.9f);

            UpdateBackMoveCount(direction);

        }
        // -----------------------
        void ResetJump()
        {
            isJumping = false;
            cam.ResetSide();
        }
        // -----------------------
        void TrySnapToPendingStone()
        {
            if (!snapToStoneCenter || pendingStone == null) return;
            if (IsOverStoneXZ(pendingStone))
            {
                SnapToStone(pendingStone);
                isOnStone = true;
            }
            pendingStone = null;
        }

        bool IsOverStoneXZ(Collider stoneCollider)
        {
            Bounds bounds = stoneCollider.bounds;
            Vector3 pos = transform.position;
            return pos.x >= bounds.min.x && pos.x <= bounds.max.x
                && pos.z >= bounds.min.z && pos.z <= bounds.max.z;
        }

        void SnapToStone(Collider stoneCollider)
        {
            Vector3 center = stoneCollider.bounds.center;
            Debug.Log($"Snapping to stone at center: {center}");
            Vector3 pos = transform.position;

            pos.x = center.x + 0.3f; // Slightly reduce to prevent edge cases
            pos.z = center.z;
            transform.position = pos;

            if (playerCollider == null) return;

            float stoneTopY = stoneCollider.bounds.max.y + stoneSurfaceOffset;
            float playerBottomY = playerCollider.bounds.min.y;
            float deltaY = stoneTopY - playerBottomY;
            transform.position += new Vector3(0f, deltaY, 0f);

            // Considered grounded after snapping so next jump can start.
            isJumping = false;
        }

        void RestoreYIfNeeded()
        {
            if (isOnStone) return;

            Vector3 pos = transform.position;
            pos.y = baseY;
            transform.position = pos;
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
                if (tag == stoneTag)
                {
                    // pendingStone = collision.collider;
                    if (snapToStoneCenter)
                    {
                        SnapToStone(collision.collider);
                    }
                    isOnStone = true;
                    isJumping = false;
                    pendingStone = null;
                    return;
                }


            }

            if (IsAllowedLandingSurface(tag))
            {
                isJumping = false;
                isOnStone = false;
                pendingStone = null;
                return;
            }

            // Obstacles should block movement but must not permanently lock jump state.
            if (IsObstacleTag(tag) || IsObstacleTagInHierarchy(collision.collider.transform))
            {
                isJumping = false;
                isOnStone = false;
                pendingStone = null;
                return;
            }

            // Any other surface stops further jumps.
            isJumping = true;
        }

        void OnCollisionExit(Collision collision)
        {
            if (pendingStone != null && collision.collider == pendingStone)
            {
                pendingStone = null;
            }

            if (collision.gameObject.CompareTag(stoneTag))
            {
                isOnStone = false;

                if (!isJumping)
                {
                    RestoreYIfNeeded();
                }
            }

            if (IsAllowedLandingSurface(collision.gameObject.tag))
            {
                // Leaving a valid surface keeps us in jump state until we land again.
                isJumping = true;
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

        void ResolvePlayerCollider()
        {
            if (playerCollider != null) return;
            playerCollider = GetComponent<Collider>();
            if (playerCollider == null)
            {
                playerCollider = GetComponentInChildren<Collider>();
            }
        }

        bool CanMoveBack()
        {
            return maxBackwardMoves > 0 && backwardMovesUsed < maxBackwardMoves;
        }

        void UpdateBackMoveCount(Vector3 direction)
        {
            if (direction == Vector3.back)
            {
                backwardMovesUsed++;
            }
            else if (direction == Vector3.forward)
            {
                backwardMovesUsed = 0;
            }
        }
    }
}


