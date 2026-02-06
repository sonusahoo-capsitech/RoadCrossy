using UnityEngine;
using System.Collections.Generic;

namespace GameWise.crossyroad
{
    public static class ProgressManager
    {
        private const string KEY = "LEVEL_PROGRESS";

        public static LevelProgress data = new LevelProgress();


        // Load Progress
        public static void Load()
        {
            if (PlayerPrefs.HasKey(KEY))
            {
                string json = PlayerPrefs.GetString(KEY);
                data = JsonUtility.FromJson<LevelProgress>(json);
            }
            else
            {
                data = new LevelProgress();
                data.playedLevels.Add(1);
                Save();
            }
        }

        // Save Progress
        public static void Save()
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(KEY, json);
            PlayerPrefs.Save(); 
        }

        // Mark Level as Played
        public static void AddPlayed(int level)
        {
            if (!data.playedLevels.Contains(level))
            {
                data.playedLevels.Add(level);
                Save();
            }
        }

        // Mark Level as Completed
        public static void AddCompleted(int level)
        {
            if (!data.completedLevels.Contains(level))
            {
                data.completedLevels.Add(level);
                Save();
            }
        }
    }

}