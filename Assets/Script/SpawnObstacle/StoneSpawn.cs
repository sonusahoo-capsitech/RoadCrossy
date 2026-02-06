

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
        void SpawnWave()
        {
            BoxCollider col = areaCollider;
            // Debug.Log("Spawn Area Size: " + col.size);

            float halfWidth = col.size.x * 0.5f;
            // float[] halfZ = {-1.2f,0f,1.15f};
            // Debug.Log("Half Width: " + halfWidth);

            for (int i = 0; i < stonesPerWave; i++)
            {
                // Random X inside river width (LOCAL)
                float localX = Random.Range(-halfWidth, halfWidth);
                // float localZ = Random.Range(-halfZ, halfZ);

                // Z stays 0 (center of river)
                Vector3 localPos = new Vector3(localX, -0.3f, 0f);
                // Debug.Log("Spawning stone at local Z: " + localPos);

                // Convert local → world
                Vector3 worldPos = spawnArea.TransformPoint(localPos);
                Debug.Log("Spawning stone at world X: " + worldPos);
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