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

        private bool isJumping = false;
        private bool isOnStone = false;
        private float baseY = 0f;
        public static PlayerManager Instance;

        public GameObject updownButton;

        private CameraFollow cam;
        private Collider pendingStone;

        void Start()
        {
            Instance = this;

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

            StartCoroutine(Jump(direction));
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

        IEnumerator Jump(Vector3 direction)
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

            float elapsed = 0f;

            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / jumpDuration;

                // Horizontal movement
                Vector3 horizontal = Vector3.Lerp(startPos, endPos, t);

                // Vertical jump arc
                float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;

                transform.position = new Vector3(
                    horizontal.x,
                    startPos.y + height,
                    horizontal.z
                );

                yield return null;
            }

            transform.position = endPos;
            TrySnapToPendingStone();
            RestoreYIfNeeded();
            isJumping = false;
            cam.ResetSide();

            UpdateBackMoveCount(direction);

        }

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
            Vector3 pos = transform.position;
            pos.x = center.x;
            pos.z = center.z;
            transform.position = pos;

            if (playerCollider == null) return;

            float stoneTopY = stoneCollider.bounds.max.y + stoneSurfaceOffset;
            float playerBottomY = playerCollider.bounds.min.y;
            float deltaY = stoneTopY - playerBottomY;
            transform.position += new Vector3(0f, deltaY, 0f);
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
            if (collision.gameObject.CompareTag("Car"))
            {
                // Debug.Log("Collided with Car! Game Over.");
                MenuUiManager.Instance.EndGame();

            }
            else if (collision.gameObject.CompareTag(stoneTag))
            {
                if (!snapToStoneCenter) return;
                if (isJumping)
                {
                    pendingStone = collision.collider;
                }
                else
                {
                    SnapToStone(collision.collider);
                    isOnStone = true;
                }
            }
            else if (collision.gameObject.CompareTag("River"))
            {
                Debug.Log("Fell into Water! Game Over.");
                MenuUiManager.Instance.EndGame();
            }
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
