using UnityEngine;
using System.Collections.Generic;



namespace Gamewise.crossyroad
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Mode")]
        public bool useInfinitePath = true;
        public bool usePrefabArray = true;

        public int level = 1;

        [Header("References")]
        public Transform player;
        public Transform lanesParent;

        [Header("Lane Prefabs")]
        public GameObject grassLanePrefab;
        public GameObject roadLanePrefab;
        public GameObject riverLanePrefab;
        public GameObject railLanePrefab;
        public GameObject finishLanePrefab;

        [Header("Path Prefab Array")]
        public GameObject[] pathPrefabs;
        public bool useFirstPrefabForSafeStart = true;
        public bool usePrefabY = true;
        public float spawnTriggerDistance = 20f;

        [Header("Lane Y Offsets")]
        public float grassLaneY = 0f;
        public float roadLaneY = 0f;
        public float riverLaneY = 0.6123f;
        public float railLaneY = 0f;

        public float laneLength = 5f;

        [Header("Infinite Path Settings")]
        public int initialLaneCount = 18;
        public int safeStartLanes = 3;
        public int lanesAhead = 12;
        public int lanesBehind = 6;
        public int maxSameTypeStreak = 2;

        [Header("Performance")]
        public int spawnBudgetPerFrame = 3;
        public bool cacheLaneLengths = true;

        [Header("Lane Weights")]
        [Range(0f, 1f)] public float grassWeight = 0.35f;
        [Range(0f, 1f)] public float roadWeight = 0.40f;
        [Range(0f, 1f)] public float riverWeight = 0.15f;
        [Range(0f, 1f)] public float railWeight = 0.10f;

        [Header("Road Settings")]
        public Vector2 carSpeedRange = new Vector2(3f, 6f);
        public Vector2 spawnRateRange = new Vector2(1f, 2.5f);

        [Header("River Settings")]
        public Vector2 logSpeedRange = new Vector2(2f, 4f);
        public Vector2Int gapRange = new Vector2Int(2, 5);

        [Header("Rail Settings")]
        public Vector2 trainDelayRange = new Vector2(2f, 5f);

        private float nextZPos = 0f;
        private LaneType lastType = LaneType.Grass;
        private int sameTypeCount = 0;
        private readonly Queue<SpawnedLane> spawnedLanes = new Queue<SpawnedLane>();
        private readonly Dictionary<GameObject, float> laneLengthCache = new Dictionary<GameObject, float>(16);
        private bool hasLanesParent = false;

        void Start()
        {
            hasLanesParent = lanesParent != null;
            if (useInfinitePath)
            {
                InitInfinitePath();
            }
            else
            {
                LoadLevel();
            }
        }

        void Update()
        {
            if (!useInfinitePath) return;

            if (player == null)
            {
                ResolvePlayerReference();
                if (player == null) return;
            }

            SpawnAheadIfNeeded();
            CleanupBehind();
        }

        void InitInfinitePath()
        {
            ResolvePlayerReference();
            ResetInfiniteState();

            if (usePrefabArray && pathPrefabs != null && pathPrefabs.Length > 0)
            {
                GameObject safePrefab = useFirstPrefabForSafeStart ? pathPrefabs[0] : null;

                for (int i = 0; i < safeStartLanes; i++)
                {
                    SpawnLaneFromArray(safePrefab);
                }

                int remaining = Mathf.Max(0, initialLaneCount - safeStartLanes);
                for (int i = 0; i < remaining; i++)
                {
                    SpawnLaneFromArray();
                }
            }
            else
            {
                for (int i = 0; i < safeStartLanes; i++)
                {
                    SpawnLane(LaneType.Grass);
                }

                int remaining = Mathf.Max(0, initialLaneCount - safeStartLanes);
                for (int i = 0; i < remaining; i++)
                {
                    SpawnLane(PickLaneType());
                }
            }
        }

        void ResetInfiniteState()
        {
            while (spawnedLanes.Count > 0)
            {
                SpawnedLane oldLane = spawnedLanes.Dequeue();
                if (oldLane != null && oldLane.obj != null)
                {
                    Destroy(oldLane.obj);
                }
            }

            nextZPos = 0f;
            lastType = LaneType.Grass;
            sameTypeCount = 0;
        }

        void ResolvePlayerReference()
        {
            if (player != null) return;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        void SpawnAheadIfNeeded()
        {
            int budget = Mathf.Max(1, spawnBudgetPerFrame);
            int spawned = 0;

            if (usePrefabArray && pathPrefabs != null && pathPrefabs.Length > 0)
            {
                float distanceToEnd = nextZPos - player.position.z;
                while (distanceToEnd < spawnTriggerDistance && spawned < budget)
                {
                    SpawnLaneFromArray();
                    spawned++;
                    distanceToEnd = nextZPos - player.position.z;
                }
            }
            else
            {
                float targetZ = player.position.z + lanesAhead * laneLength;
                while (nextZPos < targetZ && spawned < budget)
                {
                    SpawnLane(PickLaneType());
                    spawned++;
                }
            }
        }

        void CleanupBehind()
        {
            float despawnZ = player.position.z - lanesBehind * laneLength;
            while (spawnedLanes.Count > 0 && spawnedLanes.Peek().EndZ < despawnZ)
            {
                SpawnedLane lane = spawnedLanes.Dequeue();
                if (lane != null && lane.obj != null)
                {
                    Destroy(lane.obj);
                }
            }
        }

        void LoadLevel()
        {
            // 1. Load JSON file
            level = PlayerPrefs.GetInt("CurrentLevel", 1);
            Debug.Log("Loading Level: " + level);
            string fileName = "Level_" + level;
            TextAsset json = Resources.Load<TextAsset>("Levels/" + fileName);

            if (json == null)
            {
                Debug.LogError("LEVEL FILE NOT FOUND");
                return;
            }

            // 2. Convert JSON → C# object
            LevelData data = JsonUtility.FromJson<LevelData>(json.text);

            // 3. Build lanes   
            CreateLanes(data);
        }

        void CreateLanes(LevelData data)
        {
            float zPos = 0f;

            foreach (LaneData lane in data.lanes)
            {
                GameObject laneObj = null;
                GameObject prefab = null;
                Vector3 pos = new Vector3(0, 0, zPos);

                if (lane.type == "Grass")
                {
                    prefab = grassLanePrefab;
                    pos.y = grassLaneY;
                }
                else if (lane.type == "Road")
                {
                    prefab = roadLanePrefab;
                    pos.y = roadLaneY;
                }
                else if (lane.type == "River")
                {
                    prefab = riverLanePrefab;
                    pos.y = riverLaneY;
                }
                else if (lane.type == "Rail")
                {
                    prefab = railLanePrefab;
                    pos.y = railLaneY;
                }
                else if (lane.type == "Finish")
                {
                    prefab = finishLanePrefab;
                }

                if (prefab != null)
                {
                    laneObj = Instantiate(prefab, pos, Quaternion.identity);
                }

                if (laneObj != null)
                {
                    if (prefab == roadLanePrefab)
                    {
                        laneObj.GetComponent<RoadLane>().Init(lane.carSpeed, lane.spawnRate);
                    }
                    else if (prefab == riverLanePrefab)
                    {
                        laneObj.GetComponent<RiverLane>().Init(lane.logSpeed, lane.gap);
                    }
                    else if (prefab == railLanePrefab)
                    {
                        laneObj.GetComponent<RailLane>().Init(lane.trainDelay);
                    }

                    zPos += GetLaneLength(laneObj, prefab);
                }
            }
        }

        void SpawnLane(LaneType type)
        {
            GameObject prefab = null;
            float yPos = 0f;

            switch (type)
            {
                case LaneType.Grass:
                    prefab = grassLanePrefab;
                    yPos = grassLaneY;
                    break;
                case LaneType.Road:
                    prefab = roadLanePrefab;
                    yPos = roadLaneY;
                    break;
                case LaneType.River:
                    prefab = riverLanePrefab;
                    yPos = riverLaneY;
                    break;
                case LaneType.Rail:
                    prefab = railLanePrefab;
                    yPos = railLaneY;
                    break;
            }

            if (prefab == null)
            {
                Debug.LogWarning("Lane prefab missing for type: " + type);
                return;
            }

            Vector3 pos = new Vector3(0f, yPos, nextZPos);
            GameObject laneObj = hasLanesParent
                ? Instantiate(prefab, pos, Quaternion.identity, lanesParent)
                : Instantiate(prefab, pos, Quaternion.identity);

            InitLane(laneObj, type);

            float laneSize = GetLaneLength(laneObj, prefab);
            spawnedLanes.Enqueue(new SpawnedLane(laneObj, nextZPos, laneSize));
            nextZPos += laneSize;
        }

        void SpawnLaneFromArray(GameObject prefabOverride = null)
        {
            GameObject prefab = prefabOverride;
            if (prefab == null)
            {
                if (pathPrefabs == null || pathPrefabs.Length == 0)
                {
                    Debug.LogWarning("Path prefabs array is empty.");
                    return;
                }

                prefab = pathPrefabs[Random.Range(0, pathPrefabs.Length)];
            }

            if (prefab == null)
            {
                Debug.LogWarning("Path prefab is missing.");
                return;
            }

            float yPos = usePrefabY ? prefab.transform.position.y : 0f;
            Vector3 pos = new Vector3(0f, yPos, nextZPos);
            GameObject laneObj = hasLanesParent
                ? Instantiate(prefab, pos, Quaternion.identity, lanesParent)
                : Instantiate(prefab, pos, Quaternion.identity);

            InitLaneFromComponents(laneObj);

            float laneSize = GetLaneLength(laneObj, prefab);
            spawnedLanes.Enqueue(new SpawnedLane(laneObj, nextZPos, laneSize));
            nextZPos += laneSize;
        }

        void InitLane(GameObject laneObj, LaneType type)
        {
            if (laneObj == null) return;

            if (type == LaneType.Road)
            {
                RoadLane road = laneObj.GetComponent<RoadLane>();
                if (road != null)
                {
                    road.Init(GetRandomFloat(carSpeedRange), GetRandomFloat(spawnRateRange));
                }
            }
            else if (type == LaneType.River)
            {
                RiverLane river = laneObj.GetComponent<RiverLane>();
                if (river != null)
                {
                    river.Init(GetRandomFloat(logSpeedRange), GetRandomInt(gapRange));
                }
            }
            else if (type == LaneType.Rail)
            {
                RailLane rail = laneObj.GetComponent<RailLane>();
                if (rail != null)
                {
                    rail.Init(GetRandomFloat(trainDelayRange));
                }
            }
        }

        void InitLaneFromComponents(GameObject laneObj)
        {
            if (laneObj == null) return;

            RoadLane road = laneObj.GetComponent<RoadLane>();
            if (road != null)
            {
                road.Init(GetRandomFloat(carSpeedRange), GetRandomFloat(spawnRateRange));
            }

            RiverLane river = laneObj.GetComponent<RiverLane>();
            if (river != null)
            {
                river.Init(GetRandomFloat(logSpeedRange), GetRandomInt(gapRange));
            }

            RailLane rail = laneObj.GetComponent<RailLane>();
            if (rail != null)
            {
                rail.Init(GetRandomFloat(trainDelayRange));
            }
        }

        float GetLaneLength(GameObject laneObj, GameObject prefab)
        {
            if (laneObj == null) return laneLength;

            if (cacheLaneLengths && prefab != null && laneLengthCache.TryGetValue(prefab, out float cached))
            {
                return cached;
            }

            Renderer renderer = laneObj.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                Debug.LogWarning("No Renderer found on lane, using fallback length");
                return laneLength;
            }

            float size = renderer.bounds.size.z;
            float length = size > 0.01f ? size : laneLength;

            if (cacheLaneLengths && prefab != null)
            {
                laneLengthCache[prefab] = length;
            }

            return length;
        }

        LaneType PickLaneType()
        {
            float wGrass = Mathf.Max(0f, grassWeight);
            float wRoad = Mathf.Max(0f, roadWeight);
            float wRiver = Mathf.Max(0f, riverWeight);
            float wRail = Mathf.Max(0f, railWeight);

            float total = wGrass + wRoad + wRiver + wRail;
            if (total <= 0f) return LaneType.Grass;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                LaneType choice = WeightedRandom(wGrass, wRoad, wRiver, wRail, total);

                if (choice == lastType && sameTypeCount >= maxSameTypeStreak)
                {
                    continue;
                }

                if (choice == lastType)
                {
                    sameTypeCount++;
                }
                else
                {
                    lastType = choice;
                    sameTypeCount = 1;
                }

                return choice;
            }

            lastType = LaneType.Grass;
            sameTypeCount = 1;
            return LaneType.Grass;
        }

        LaneType WeightedRandom(float wGrass, float wRoad, float wRiver, float wRail, float total)
        {
            float r = Random.value * total;
            if (r < wGrass) return LaneType.Grass;
            r -= wGrass;
            if (r < wRoad) return LaneType.Road;
            r -= wRoad;
            if (r < wRiver) return LaneType.River;
            return LaneType.Rail;
        }

        float GetRandomFloat(Vector2 range)
        {
            float min = Mathf.Min(range.x, range.y);
            float max = Mathf.Max(range.x, range.y);
            return Random.Range(min, max);
        }

        int GetRandomInt(Vector2Int range)
        {
            int min = Mathf.Min(range.x, range.y);
            int max = Mathf.Max(range.x, range.y);
            return Random.Range(min, max + 1);
        }

        private enum LaneType
        {
            Grass,
            Road,
            River,
            Rail
        }

        private class SpawnedLane
        {
            public GameObject obj;
            public float startZ;
            public float length;

            public float EndZ => startZ + length;

            public SpawnedLane(GameObject obj, float startZ, float length)
            {
                this.obj = obj;
                this.startZ = startZ;
                this.length = length;
            }
        }

    }
}
