// using UnityEngine;


// namespace Gamewise.crossyroad
// {
//     public class RoadLane : MonoBehaviour
//     {
//         public float carSpeed;
//         public float spawnRate;

//         public void Init(float speed, float rate)
//         {
//             carSpeed = speed;
//             spawnRate = rate;
//         }
//     }
// }

using UnityEngine;

namespace Gamewise.crossyroad
{
    public class RoadLane : MonoBehaviour
    {
        public float carSpeed;
        public float spawnRate;

        [Header("Car Setup")]
        public GameObject carPrefab;
        public Transform spawnPoint;

         [Header("Car Setup")]
        public GameObject carPrefab1;
        public Transform spawnPoint1;

        public void Init(float speed, float rate)
        {
            carSpeed = speed;
            spawnRate = rate;

            InvokeRepeating(nameof(SpawnCar), 1f, spawnRate);
        }

        void SpawnCar()
        {
            GameObject car = Instantiate(
                carPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );
             GameObject car2 = Instantiate(
                carPrefab1,
                spawnPoint1.position,
                spawnPoint1.rotation
            );

            carSpawn carMove = car.GetComponent<carSpawn>();
            
            carMove.Init(carSpeed);
            carSpawn carMove2 = car2.GetComponent<carSpawn>();
            carMove2.Init(carSpeed);
        }
    }
}
