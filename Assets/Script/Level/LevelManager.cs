using UnityEngine;
using System.Collections.Generic;



namespace Gamewise.crossyroad
{
    public class LevelManager : MonoBehaviour
    {


        public int level = 1;

        [Header("Lane Prefabs")]
        public GameObject grassLanePrefab;
        public GameObject roadLanePrefab;
        public GameObject riverLanePrefab;
        public GameObject railLanePrefab;
        public GameObject finishLanePrefab;

        public float laneLength = 5f;

        void Start()
        {
            LoadLevel();
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

        // void CreateLanes(LevelData data)
        // {
        //     float zPos = 0f;

        //     foreach (LaneData lane in data.lanes)
        //     {
        //         if (lane.type == "Grass")
        //         {
        //             Instantiate(grassLanePrefab, new Vector3(0, 0, zPos), Quaternion.identity);
        //             float laneZSize = grassLanePrefab.GetComponent<Renderer>().bounds.size.z;
        //             zPos += laneZSize;
        //         }
        //         else if (lane.type == "Road")
        //         {
        //             GameObject obj = Instantiate(roadLanePrefab, new Vector3(0, 0, zPos), Quaternion.identity);
        //             obj.GetComponent<RoadLane>().Init(lane.carSpeed, lane.spawnRate);
        //              float laneZSize = grassLanePrefab.GetComponent<Renderer>().bounds.size.z;
        //             zPos += laneZSize;
        //         }
        //         else if (lane.type == "River")
        //         {
        //             GameObject obj = Instantiate(riverLanePrefab, new Vector3(0, 0, zPos), Quaternion.identity);
        //             obj.GetComponent<RiverLane>().Init(lane.logSpeed, lane.gap);
        //              float laneZSize = grassLanePrefab.GetComponent<Renderer>().bounds.size.z;
        //             zPos += laneZSize;
        //         }
        //         else if (lane.type == "Rail")
        //         {
        //             GameObject obj = Instantiate(railLanePrefab, new Vector3(0, 0, zPos), Quaternion.identity);
        //             obj.GetComponent<RailLane>().Init(lane.trainDelay);
        //              float laneZSize = grassLanePrefab.GetComponent<Renderer>().bounds.size.z;
        //             zPos += laneZSize;
        //         }
        //         else if (lane.type == "Finish")
        //         {
        //             Instantiate(finishLanePrefab, new Vector3(0, 0, zPos), Quaternion.identity);
        //              float laneZSize = grassLanePrefab.GetComponent<Renderer>().bounds.size.z;
        //             zPos += laneZSize;
        //         }

        //         // zPos += laneLength;
        //     }
        // }

        void CreateLanes(LevelData data)
        {   
            float zPos = 0f;

            foreach (LaneData lane in data.lanes)
            {
                GameObject laneObj = null;

                if (lane.type == "Grass")
                {
                    laneObj = Instantiate(grassLanePrefab, new Vector3(0, 0, zPos), Quaternion.identity);
                }
                else if (lane.type == "Road")
                {
                    laneObj = Instantiate(roadLanePrefab, new Vector3(0, 0, zPos), Quaternion.identity);
                    laneObj.GetComponent<RoadLane>().Init(lane.carSpeed, lane.spawnRate);
                }
                else if (lane.type == "River")
                {
                    laneObj = Instantiate(riverLanePrefab, new Vector3(0, 0, zPos), Quaternion.identity);
                    laneObj.GetComponent<RiverLane>().Init(lane.logSpeed, lane.gap);
                }
                else if (lane.type == "Rail")
                {
                    laneObj = Instantiate(railLanePrefab, new Vector3(0, 0, zPos), Quaternion.identity);
                    laneObj.GetComponent<RailLane>().Init(lane.trainDelay);
                }
                else if (lane.type == "Finish")
                {
                    laneObj = Instantiate(finishLanePrefab, new Vector3(0, 0, zPos), Quaternion.identity);
                }

                // ✅ Measure the ACTUAL instantiated lane
                if (laneObj != null)
                {
                    Renderer renderer = laneObj.GetComponentInChildren<Renderer>();
                    // Debug.Log("Lane Type: " + lane.type + ", Z Size: " + (renderer != null ? renderer.bounds.size.z.ToString() : "No Renderer"));

                    if (renderer != null)
                    {
                        zPos += renderer.bounds.size.z;
                    }
                    else
                    {
                        Debug.LogWarning("No Renderer found on lane, using fallback length");
                        zPos += laneLength;
                    }
                }
            }
        }


    }
}