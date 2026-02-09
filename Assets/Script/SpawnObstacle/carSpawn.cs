using UnityEngine;

namespace Gamewise.crossyroad
{
   public class carSpawn : MonoBehaviour
   {
      public Transform movementDirection;
      private Vector3 startPos;

      public float destroyDistance = 60f;

      void Start()
      {
          startPos = transform.position;
          speed = 10f;
          destroyDistanceSqr = destroyDistance * destroyDistance;
      }
      private float speed;
      private float destroyDistanceSqr;

        public void Init(float moveSpeed)
        {
            speed = moveSpeed;
            startPos = transform.position;
            destroyDistanceSqr = destroyDistance * destroyDistance;
        }

      void Update()
      {
         transform.Translate(movementDirection.forward * speed * Time.deltaTime, Space.World);
         // transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);

          if ((transform.position - startPos).sqrMagnitude > destroyDistanceSqr)
            {
                Destroy(gameObject);
            }

      }
   }
}
