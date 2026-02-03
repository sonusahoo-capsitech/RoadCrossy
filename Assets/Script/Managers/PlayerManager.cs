// using UnityEngine;
// using System.Collections;



// namespace Gamewise.crossyroad
// {
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

//         float minX, maxX, minZ;



//         [Header("Grid Settings")]
//         public float moveDistance = 1f;

//         [Header("Jump Settings")]
//         public float jumpHeight = 0.2f;
//         public float jumpDuration = 0.10f;

//         private bool isJumping = false;
//         public static PlayerManager Instance;

//         public GameObject updownButton;
//         public Animator playerAnimator;

//         CameraFollow cam;
//         private bool isRunningForward = false;

//         void Start()
//         {
//             Instance = this;


//             cam = Camera.main.GetComponent<CameraFollow>();
//             // updownButton = GetComponent<Image>();
//             updownButton.SetActive(true);
//             SetMovementLimits();

//         }

//         void SetMovementLimits()
//         {
//             bool isTablet = Mathf.Max(Screen.width, Screen.height) >= 2000;

//             if (isTablet)
//             {
//                 minX = tabletMinX;
//                 maxX = tabletMaxX;
//                 minZ = tabletMinZ;
//             }
//             else
//             {
//                 minX = mobileMinX;
//                 maxX = mobileMaxX;
//                 minZ = mobileMinZ;
//             }
//         }


//         bool CanMove(Vector3 direction)
//         {
//             Vector3 targetPos = transform.position + direction * moveDistance;

//             if (targetPos.x < minX || targetPos.x > maxX)
//                 return false;

//             if (direction == Vector3.back && targetPos.z < minZ)
//                 return false;

//             return true;
//         }




//         void Update()
//         {
//             if (isJumping) return;
//             playerAnimator.SetBool("Test", false);

//             if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) && CanMove(Vector3.forward))
//             {
//                 // StartCoroutine(Jump(Vector3.forward));
//                 // StartCoroutine(Move(Vector3.forward));
//                 if (!isRunningForward)
//                 {
//                     StartCoroutine(ContinuousMoveForward());

//                     playerAnimator.SetBool("Test", true);
//                 }
//             }

//             else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) && CanMove(Vector3.back))
//                 StartCoroutine(Jump(Vector3.back));

//             else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) && CanMove(Vector3.left))
//                 StartCoroutine(Jump(Vector3.left));

//             else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) && CanMove(Vector3.right))
//                 StartCoroutine(Jump(Vector3.right));
//         }



//         public void MoveUp()
//         {
//             if (!isJumping && CanMove(Vector3.forward))
//                 StartCoroutine(Jump(Vector3.forward));
//         }

//         public void MoveDown()
//         {
//             if (!isJumping && CanMove(Vector3.back))
//                 StartCoroutine(Jump(Vector3.back));
//         }

//         public void MoveLeft()
//         {
//             if (!isJumping && CanMove(Vector3.left))
//                 StartCoroutine(Jump(Vector3.left));
//         }

//         public void MoveRight()
//         {
//             if (!isJumping && CanMove(Vector3.right))
//                 StartCoroutine(Jump(Vector3.right));
//         }




//         IEnumerator Jump(Vector3 direction)
//         {
//             isJumping = true;

//             if (direction == Vector3.left)
//                 cam.MoveSide(-1);
//             else if (direction == Vector3.right)
//                 cam.MoveSide(1);
//             else
//                 cam.ResetSide();


//             Vector3 startPos = transform.position;
//             Vector3 endPos = startPos + direction * moveDistance;

//             float elapsed = 0f;

//             while (elapsed < jumpDuration)
//             {
//                 elapsed += Time.deltaTime;
//                 float t = elapsed / jumpDuration;

//                 // Horizontal movement
//                 Vector3 horizontal = Vector3.Lerp(startPos, endPos, t);

//                 // Vertical jump arc
//                 float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;

//                 transform.position = new Vector3(
//                     horizontal.x,
//                     startPos.y + height,
//                     horizontal.z
//                 );

//                 yield return null;
//             }

//             transform.position = endPos;
//             isJumping = false;
//             cam.ResetSide();

//         }



//         IEnumerator Move(Vector3 direction)
//         {
//             // isJumping = true;

//             if (direction == Vector3.left)
//                 cam.MoveSide(-1);
//             else if (direction == Vector3.right)
//                 cam.MoveSide(1);
//             else
//                 cam.ResetSide();

//             Vector3 startPos = transform.position;
//             Vector3 endPos = startPos + direction * moveDistance;

//             float elapsed = 0f;

//             while (elapsed < jumpDuration)
//             {
//                 elapsed += Time.deltaTime;
//                 float t = elapsed / jumpDuration;

//                 // Smooth flat movement (no Y change)
//                 transform.position = Vector3.Lerp(startPos, endPos, t);

//                 yield return null;
//             }

//             transform.position = endPos;
//             isJumping = false;
//             cam.ResetSide();
//         }


//         void OnCollisionEnter(Collision collision)
//         {
//             if (collision.gameObject.CompareTag("Car"))
//             {
//                 // Debug.Log("Collided with Car! Game Over.");
//                 MenuUiManager.Instance.EndGame();

//             }
//             else if (collision.gameObject.CompareTag("Finish"))
//             {
//                 // Debug.Log("Reached Goal! You Win!");
//                 MenuUiManager.Instance.WinGame();
//             }
//             else if (collision.gameObject.CompareTag("River"))
//             {
//                 Debug.Log("Fell into Water! Game Over.");
//                 MenuUiManager.Instance.EndGame();

//             }
//         }
//         IEnumerator ContinuousMoveForward()
//         {
//             isRunningForward = true;

//             while (isRunningForward && CanMove(Vector3.forward))
//             {
//                 isJumping = true;
//                 yield return StartCoroutine(Move(Vector3.forward));
//                 isJumping = false;

//                 // small delay to control speed (VERY IMPORTANT)
//                 yield return new WaitForSeconds(0.05f);
//             }

//             isRunningForward = false;
//         }

//     }
// }


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

        float minX, maxX, minZ;

        [Header("Movement Settings")]
        public float moveSpeed = 5f; // Speed of continuous movement

        private bool isMoving = false;
        private Vector3 currentMoveDirection = Vector3.zero;

        public static PlayerManager Instance;
        public GameObject updownButton;
        public Animator playerAnimator;

        CameraFollow cam;

        void Start()
        {
            Instance = this;
            cam = Camera.main.GetComponent<CameraFollow>();
            updownButton.SetActive(true);
            SetMovementLimits();
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
            Vector3 targetPos = transform.position + direction * moveSpeed * Time.deltaTime;

            if (targetPos.x < minX || targetPos.x > maxX)
                return false;

            if (direction == Vector3.back && targetPos.z < minZ)
                return false;

            return true;
        }

        // void Update()
        // {
        //     playerAnimator.SetBool("Test", false);
        //     playerAnimator.SetBool("Left", false);

        //     // Check for key press/hold to start/continue movement
        //     if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        //     {

        //         playerAnimator.SetBool("Test", true);
        //         playerAnimator.SetBool("Left", false);
        //         playerAnimator.SetBool("Right", false);
        //         currentMoveDirection = Vector3.forward;
        //         isMoving = true;
        //     }
        //     else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        //     {
        //         currentMoveDirection = Vector3.back;
        //         isMoving = true;
        //     }
        //     else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        //     {
        //         playerAnimator.SetBool("Left", true);
        //         playerAnimator.SetBool("Test", true);
        //         currentMoveDirection = Vector3.left;
        //         isMoving = true;
        //     }
        //     else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        //     {
        //         playerAnimator.SetBool("Right", true);
        //         currentMoveDirection = Vector3.right;
        //         isMoving = true;
        //     }
        //     else
        //     {
        //         isMoving = false;
        //         playerAnimator.SetBool("Test", false);
        //         playerAnimator.SetBool("Left", false);
        //         playerAnimator.SetBool("Right", false);
        //         currentMoveDirection = Vector3.zero;
        //         cam.ResetSide();
        //     }

        //     // Move the player if a direction is active
        //     if (isMoving && CanMove(currentMoveDirection))
        //     {
        //         // Update camera side position
        //         if (currentMoveDirection == Vector3.left)
        //             cam.MoveSide(-1);
        //         else if (currentMoveDirection == Vector3.right)
        //             cam.MoveSide(1);
        //         else
        //             cam.ResetSide();

        //         // Move the player
        //         transform.position += currentMoveDirection * moveSpeed * Time.deltaTime;
        //     }
        // }


        void Update()
        {
            Vector3 inputDir = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                inputDir = Vector3.forward;
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                inputDir = Vector3.back;
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                inputDir = Vector3.left;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                inputDir = Vector3.right;

            bool isMoving = inputDir != Vector3.zero;

            // 🎬 Animator
            playerAnimator.SetBool("IsMoving", isMoving);
            playerAnimator.SetFloat("MoveX", inputDir.x);
            playerAnimator.SetFloat("MoveZ", inputDir.z);

            if (!isMoving)
            {
                cam.ResetSide();
                return;
            }

            if (!CanMove(inputDir)) return;

            // 🎥 Camera side shift
            if (inputDir == Vector3.left)
                cam.MoveSide(-1);
            else if (inputDir == Vector3.right)
                cam.MoveSide(1);
            else
                cam.ResetSide();

            // 🧍 Movement
            transform.position += inputDir * moveSpeed * Time.deltaTime;

            // 🔄 Face direction
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(inputDir),
                15f * Time.deltaTime
            );
        }





        // For UI Button - Start continuous movement on button press (OnPointerDown)
        public void MoveUp()
        {
            currentMoveDirection = Vector3.forward;
            isMoving = true;
        }

        public void MoveDown()
        {
            currentMoveDirection = Vector3.back;
            isMoving = true;
        }

        public void MoveLeft()
        {
            currentMoveDirection = Vector3.left;
            isMoving = true;
        }

        public void MoveRight()
        {
            currentMoveDirection = Vector3.right;
            isMoving = true;
        }

        // For UI Button - Stop continuous movement on button release (OnPointerUp)
        public void StopMovement()
        {
            isMoving = false;
            currentMoveDirection = Vector3.zero;
            cam.ResetSide();
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Car"))
            {
                isMoving = false;
                currentMoveDirection = Vector3.zero;
                MenuUiManager.Instance.EndGame();
            }
            else if (collision.gameObject.CompareTag("Finish"))
            {
                isMoving = false;
                currentMoveDirection = Vector3.zero;
                MenuUiManager.Instance.WinGame();
            }
            else if (collision.gameObject.CompareTag("River"))
            {
                Debug.Log("Fell into Water! Game Over.");
                isMoving = false;
                currentMoveDirection = Vector3.zero;
                MenuUiManager.Instance.EndGame();
            }
        }
    }
}