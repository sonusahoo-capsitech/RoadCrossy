using UnityEngine;



namespace Gamewise.crossyroad
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        public Transform player;

        [Header("Follow Settings")]
        public float zOffset = -6f;
        public float yOffset = 6f;

        [Header("Side Movement")]
        public float sideOffsetAmount = 0.5f;
        public float sideSmoothSpeed = 6f;

        private float currentSideOffset = 0f;
        private float targetSideOffset = 0f;
        private float lockedY;

        void Start()
        {
            if (player)
            {
                lockedY = player.position.y + yOffset;
            }
            else
            {
                lockedY = transform.position.y;
            }
        }

        void LateUpdate()
        {
            if (!player) return;

            // Smooth side movement
            currentSideOffset = Mathf.Lerp(
                currentSideOffset,
                targetSideOffset,
                Time.deltaTime * sideSmoothSpeed
            );

            Vector3 targetPos = new Vector3(
                player.position.x + currentSideOffset,
                lockedY,
                player.position.z + zOffset
            );

            transform.position = targetPos;
        }

        // Called from PlayerManager
        public void MoveSide(float direction)
        {
            targetSideOffset = direction * sideOffsetAmount;
        }

        public void ResetSide()
        {
            targetSideOffset = 0f;
        }
    }
}
