// using UnityEngine;
 
// // namespace Gamewise.crossyroad
// // {
//     [RequireComponent(typeof(Rigidbody))]
//     public class PlayerManager : MonoBehaviour
//     {
//         [Header("Movement Limits - Mobile")]
//         public float mobileMinX = -3f;
//         public float mobileMaxX = 3f;
//         public float mobileMinZ = -2f;
 
//         [Header("Movement Limits - Tablet")]
//         public float tabletMinX = -5f;
//         public float tabletMaxX = 5f;
//         public float tabletMinZ = -3f;
 
//         private float minX;
//         private float maxX;
//         private float minZ;
 
//         [Header("Grid Settings")]
//         public float moveDistance = 1f;
 
//         [Header("Backward Limit")]
//         public int maxBackwardMoves = 3;
//         private int backwardMovesUsed;
 
//         [Header("Physics Jump")]
//         public float horizontalImpulse = 4.5f;
//         public float jumpImpulse = 5.5f;
 
//         [Header("Ground Check")]
//         public float groundCheckDistance = 0.2f;
 
//         [Header("Stone Snap")]
//         public string stoneTag = "Stone";
//         public float stoneSurfaceOffset = 0.02f;
//         public Collider playerCollider;
 
//         public static PlayerManager Instance;
//         public GameObject updownButton;
 
//         private Rigidbody rb;
//         private CameraFollow cam;
 
//         private bool isGrounded;
//         private bool isJumping;
//         private bool isOnStone;
//         private Collider pendingStone;
 
//         void Awake()
//         {
//             Instance = this;
 
//             rb = GetComponent<Rigidbody>();
//             rb.freezeRotation = true;
//             rb.interpolation = RigidbodyInterpolation.Interpolate;
//             rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
 
//             cam = Camera.main.GetComponent<CameraFollow>();
//         }
 
//         void Start()
//         {
//             updownButton.SetActive(true);
//             ResolvePlayerCollider();
//             SetMovementLimits();
//         }
 
//         void Update()
//         {
//             CheckGround();
 
//             if (isJumping || !isGrounded)
//                 return;
 
//             if (TryGetKeyboardDirection(out Vector3 direction))
//                 TryMove(direction);
//         }
 
//         void SetMovementLimits()
//         {
//             bool isTablet = Mathf.Max(Screen.width, Screen.height) >= 2000;
 
//             minX = isTablet ? tabletMinX : mobileMinX;
//             maxX = isTablet ? tabletMaxX : mobileMaxX;
//             minZ = isTablet ? tabletMinZ : mobileMinZ;
//         }
 
//         void TryMove(Vector3 direction)
//         {
//             if (!CanMove(direction)) return;
//             if (direction == Vector3.back && !CanMoveBack()) return;
 
//             PerformHop(direction);
//         }
 
//         bool CanMove(Vector3 direction)
//         {
//             Vector3 targetPos = rb.position + direction * moveDistance;
 
//             if (targetPos.x < minX || targetPos.x > maxX)
//                 return false;
 
//             if (direction == Vector3.back && targetPos.z < minZ)
//                 return false;
 
//             return true;
//         }
 
//         void PerformHop(Vector3 direction)
//         {
//             isJumping = true;
 
//             // Camera behavior
//             if (direction == Vector3.left)
//                 cam.MoveSide(-1);
//             else if (direction == Vector3.right)
//                 cam.MoveSide(1);
//             else
//                 cam.ResetSide();
 
//             // Reset horizontal velocity (prevents jitter)
//             rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
 
//             // Apply impulse
//             Vector3 impulse =
//                 direction.normalized * horizontalImpulse +
//                 Vector3.up * jumpImpulse;
 
//             rb.AddForce(impulse, ForceMode.Impulse);
 
//             UpdateBackMoveCount(direction);
 
//             Invoke(nameof(ResetJump), 0.18f);
//         }
 
//         void ResetJump()
//         {
//             isJumping = false;
//             cam.ResetSide();
//         }
 
//         void CheckGround()
//         {
//             Ray ray = new Ray(transform.position + Vector3.up * 0.05f, Vector3.down);
//             isGrounded = Physics.Raycast(ray, groundCheckDistance);
//         }
 
//         bool TryGetKeyboardDirection(out Vector3 direction)
//         {
//             if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
//             {
//                 direction = Vector3.forward;
//                 return true;
//             }
//             if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
//             {
//                 direction = Vector3.back;
//                 return true;
//             }
//             if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
//             {
//                 direction = Vector3.left;
//                 return true;
//             }
//             if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
//             {
//                 direction = Vector3.right;
//                 return true;
//             }
 
//             direction = Vector3.zero;
//             return false;
//         }
 
//         void OnCollisionEnter(Collision collision)
//         {
//             if (collision.gameObject.CompareTag("Car") ||
//                 collision.gameObject.CompareTag("River"))
//             {
//                 MenuUiManager.Instance.EndGame();
//             }
//             else if (collision.gameObject.CompareTag(stoneTag))
//             {
//                 pendingStone = collision.collider;
//                 SnapToStone(pendingStone);
//                 isOnStone = true;
//             }
//         }
 
//         void OnCollisionExit(Collision collision)
//         {
//             if (collision.gameObject.CompareTag(stoneTag))
//             {
//                 isOnStone = false;
//             }
//         }
 
//         void SnapToStone(Collider stone)
//         {
//             if (playerCollider == null) return;
 
//             float stoneTopY = stone.bounds.max.y + stoneSurfaceOffset;
//             float playerBottomY = playerCollider.bounds.min.y;
//             float deltaY = stoneTopY - playerBottomY;
 
//             rb.position += Vector3.up * deltaY;
//         }
 
//         void ResolvePlayerCollider()
//         {
//             if (playerCollider == null)
//                 playerCollider = GetComponent<Collider>() ??
//                                  GetComponentInChildren<Collider>();
//         }
 
//         bool CanMoveBack()
//         {
//             return maxBackwardMoves <= 0 || backwardMovesUsed < maxBackwardMoves;
//         }
 
//         void UpdateBackMoveCount(Vector3 direction)
//         {
//             if (direction == Vector3.back)
//                 backwardMovesUsed++;
//             else if (direction == Vector3.forward)
//                 backwardMovesUsed = 0;
//         }
//     }
// // }
 