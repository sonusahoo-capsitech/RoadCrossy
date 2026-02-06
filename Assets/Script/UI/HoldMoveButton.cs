// using UnityEngine;
// using UnityEngine.EventSystems;

// namespace Gamewise.crossyroad
// {
//     public class HoldMoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
//     {
//         public enum MoveDirection
//         {
//             Up,
//             Down,
//             Left,
//             Right
//         }

//         [SerializeField] private MoveDirection direction = MoveDirection.Up;
//         [Header("Press Feedback")]
//         [SerializeField] private Transform visualTarget;
//         [SerializeField] private Vector3 pressedScale = new Vector3(0.92f, 0.92f, 0.92f);
//         [SerializeField] private float scaleLerpSpeed = 18f;
//         [SerializeField] private bool logPressEvents = true;

//         private Vector3 originalScale;
//         private bool isPressed;

//         private void Awake()
//         {
//             if (visualTarget == null)
//             {
//                 visualTarget = transform;
//             }

//             originalScale = visualTarget.localScale;
//         }

//         private void Update()
//         {
//             if (visualTarget == null)
//             {
//                 return;
//             }

//             Vector3 targetScale = isPressed ? pressedScale : originalScale;
//             visualTarget.localScale = Vector3.Lerp(
//                 visualTarget.localScale,
//                 targetScale,
//                 scaleLerpSpeed * Time.unscaledDeltaTime
//             );
//         }

//         public void OnPointerDown(PointerEventData eventData)
//         {
//             if (PlayerManager.Instance == null)
//             {
//                 return;
//             }

//             isPressed = true;
//             if (logPressEvents)
//             {
//                 Debug.Log($"[HoldMoveButton] Down: {direction}", this);
//             }

//             switch (direction)
//             {
//                 case MoveDirection.Up:
//                     PlayerManager.Instance.MoveUp();
//                     break;
//                 case MoveDirection.Down:
//                     PlayerManager.Instance.MoveDown();
//                     break;
//                 case MoveDirection.Left:
//                     PlayerManager.Instance.MoveLeft();
//                     break;
//                 case MoveDirection.Right:
//                     PlayerManager.Instance.MoveRight();
//                     break;
//             }
//         }

//         public void OnPointerUp(PointerEventData eventData)
//         {
//             if (PlayerManager.Instance == null)
//             {
//                 return;
//             }

//             isPressed = false;
//             if (logPressEvents)
//             {
//                 Debug.Log($"[HoldMoveButton] Up: {direction}", this);
//             }
//             PlayerManager.Instance.StopMovement();
//         }

//         public void OnPointerExit(PointerEventData eventData)
//         {
//             if (PlayerManager.Instance == null)
//             {
//                 return;
//             }

//             isPressed = false;
//             if (logPressEvents)
//             {
//                 Debug.Log($"[HoldMoveButton] Exit: {direction}", this);
//             }
//             PlayerManager.Instance.StopMovement();
//         }
//     }
// }
