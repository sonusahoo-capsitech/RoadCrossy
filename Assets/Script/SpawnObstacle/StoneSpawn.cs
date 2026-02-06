// using UnityEngine;
// using System.Collections;

// public class StoneSpawn : MonoBehaviour
// {
//     [Header("Stone Settings")]
//     public GameObject stonePrefab;
//     public int stonesPerWave = 3;
//     public float spawnInterval = 1.5f;
//     public float stoneLifeTime = 4f;

//     [Header("ZigZag Settings")]
//     public float zigZagOffset = 1.5f;

//     private BoxCollider spawnArea;
//     private bool zigZagLeft = true;

//     void Start()
//     {
//         spawnArea = GetComponent<BoxCollider>();
//         StartCoroutine(SpawnRoutine());
//     }

//     IEnumerator SpawnRoutine()
//     {
//         while (true)
//         {
//             SpawnWave();
//             yield return new WaitForSeconds(spawnInterval);
//         }
//     }

//     void SpawnWave()
//     {
//         Vector3 center = spawnArea.bounds.center;
//         Vector3 size = spawnArea.bounds.size;

//         for (int i = 0; i < stonesPerWave; i++)
//         {
//             float xOffset = zigZagLeft ? -zigZagOffset : zigZagOffset;
//             zigZagLeft = !zigZagLeft;

//             float randomZ = Random.Range(
//                 center.z - size.z / 2,
//                 center.z + size.z / 2
//             );

//             Vector3 spawnPos = new Vector3(
//                 center.x + xOffset,
//                 center.y,
//                 randomZ
//             );

//             GameObject stone = Instantiate(stonePrefab, spawnPos, Quaternion.identity, transform);

//             Destroy(stone, stoneLifeTime);
//         }
//     }
// }



using UnityEngine;
using System.Collections;

namespace Gamewise.crossyroad
{
    public class StoneSpawn : MonoBehaviour
    {
        [Header("References")]
        public Transform spawnArea;
        public GameObject stonePrefab;

        [Header("Spawn Settings")]
        public int stonesPerWave = 1;
        public float spawnInterval = 1.5f;
        public float stoneLifeTime = 4f;

        [Header("ZigZag Settings")]
        public float zigZagOffset = 1.5f;

        private BoxCollider areaCollider;
        // private bool zigLeft = true;

        void Start()
        {
            areaCollider = spawnArea.GetComponent<BoxCollider>();
            StartCoroutine(SpawnRoutine());
        }

        IEnumerator SpawnRoutine()
        {
            while (true)
            {
                SpawnWave();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        // void SpawnWave()
        // {
        //     Bounds b = areaCollider.bounds;
        //     // Debug.Log("Spawn Area Bounds: " + b.min.x);
        //     // Debug.Log("Spawn Area Bounds: " + b);

        //     for (int i = 0; i < stonesPerWave; i++)
        //     {
        //         // float x = zigLeft
        //         //     ? b.min.x + zigZagOffset
        //         //     : b.max.x - zigZagOffset;

        //         zigLeft = !zigLeft;

        //         float x = Random.Range(b.min.x, b.max.x);
        //         // float z = 0f;

        //         float z = Random.Range(b.min.z, b.max.z);
        //         Debug.Log("Spawning stone at z max: " + b.max.z + ", Z min: " + b.min.z);
        //         Debug.Log("Spawning stone at z: " + z);

        //         Vector3 spawnPos = new Vector3(
        //             x,
        //             b.center.y + 0.3f,
        //             z
        //         );

        //         GameObject stone = Instantiate(
        //             stonePrefab,
        //             spawnPos,
        //             Quaternion.identity,
        //             transform
        //         );

        //         Destroy(stone, stoneLifeTime);
        //     }
        // }

        void SpawnWave()
        {
            BoxCollider col = areaCollider;

            float halfWidth = col.size.x * 0.5f;

            for (int i = 0; i < stonesPerWave; i++)
            {
                // Random X inside river width (LOCAL)
                float localX = Random.Range(-halfWidth, halfWidth);

                // Z stays 0 (center of river)
                Vector3 localPos = new Vector3(localX, 2f, 0f);
                // Debug.Log("Spawning stone at local Z: " + localPos);

                // Convert local → world
                Vector3 worldPos = spawnArea.TransformPoint(localPos);
                // Debug.Log("Spawning stone at world Z: " + worldPos);

                GameObject stone = Instantiate(
                    stonePrefab,
                    worldPos,
                    Quaternion.identity,
                    transform
                );

                Destroy(stone, stoneLifeTime);
            }
        }

    }
}