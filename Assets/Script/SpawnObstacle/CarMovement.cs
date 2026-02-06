using System.Collections;
using UnityEngine;


namespace Gamewise.crossyroad
{
    public class CarMovement : MonoBehaviour
    {
        public GameObject carPrefab;
        public bool moveRight = true;
        public Transform spawnPoint;


        void Start()
        {
            SpawnCar();
            // StartCoroutine(MoveCar(carPrefab));
        }

        // IEnumerator MoveCar(GameObject car)
        // {
        //     float speed = 5f;
        //     while (true)
        //     {
        //         if (moveRight)
        //             car.transform.Translate(Vector3.right * speed * Time.deltaTime);
        //         else
        //             car.transform.Translate(Vector3.left * speed * Time.deltaTime);

        //         yield return null;
        //     }
        // }


        void SpawnCar()
        {
            GameObject car = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);

            // Decide direction
            if (moveRight)
                // car.transform.rotation = Quaternion.Euler(0, 90, 0);
                car.transform.rotation = spawnPoint.rotation;
            else
                car.transform.rotation = spawnPoint.rotation;
        }
    }
}