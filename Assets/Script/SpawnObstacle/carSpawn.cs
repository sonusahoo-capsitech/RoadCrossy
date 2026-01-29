using UnityEngine;

namespace Gamewise.crossyroad
{
   public class carSpawn : MonoBehaviour
   {
      // Start is called once before the first execution of Update after the MonoBehaviour is created

      public Transform movementDirection;
      Vector3 startPos;

        public float destroyDistance = 60f;

      void Start()
      {
          startPos = transform.position;
          speed = 10f;
      }
      float speed;

        public void Init(float moveSpeed)
        {
            speed = moveSpeed;
             startPos = transform.position;
        }

      void Update()
      {
         transform.Translate(movementDirection.forward * speed * Time.deltaTime, Space.World);
         // transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);

          if (Vector3.Distance(startPos, transform.position) > destroyDistance)
            {
                Destroy(gameObject);
            }

      }
   }
}