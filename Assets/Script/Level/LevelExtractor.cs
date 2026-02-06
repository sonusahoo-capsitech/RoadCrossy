using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameWise.crossyroad
{
    [System.Serializable]
    public class LevelProgress
    {
        public List<int> playedLevels = new List<int>();
        public List<int> completedLevels = new List<int>();
    }
    public class LevelExtractor : MonoBehaviour
    {
        List<int> played => ProgressManager.data.playedLevels;
        List<int> completedLevel => ProgressManager.data.completedLevels;

        // public TextAsset levelData;
        public GameObject levelsParent;
        // void Start() {
        //     if (levelData != null) {
        //         string[] lines = levelData.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        //         foreach (string line in lines) {
        //             Debug.Log(line);
        //         }
        //     } else {
        //         Debug.LogError("Level data file is not assigned.");
        //     }
        // }
        public static LevelExtractor Instance;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        void Start()
        {
            SetupLevelButtons();
        }

        public void SetupLevelButtons()
        {
            Button[] buttons = levelsParent.GetComponentsInChildren<Button>(true);

            foreach (Button btn in buttons)
            {
                btn.onClick.AddListener(() =>
                {
                    string parentLevelName = btn.transform.parent.parent.name;

                    string number = parentLevelName.Replace("Level_", "");

                    Debug.Log("Selected Level: " + number);

                    PlayerPrefs.SetInt("CurrentLevel", int.Parse(number));
                    PlayerPrefs.Save();
                    StartLevel();
                    SceneManager.LoadScene("GamePlay");

                });
            }
        }


        public void StartLevel()
        {
            SetupLevelButtons();

            MenuUiManager.Instance.play.gameObject.SetActive(false);
            MenuUiManager.Instance.levelSelectPanel.gameObject.SetActive(false);
            MenuUiManager.Instance.Background.gameObject.SetActive(false);
        }


        public void UpdateLevelLocks()
        {
            HashSet<int> completedSet = new HashSet<int>(played);

            // Loop through all available levels (1 to 99)
            for (int i = 1; i <= levelsParent.transform.childCount; i++)
            {
                string levelName = "Level_" + i;
                Transform levelTransform = levelsParent.transform.Find(levelName);

                if (levelTransform == null)
                {
                    continue;
                }

                Transform lockBtn = levelTransform.Find("LockButton/LockBtn");
                Transform icon = levelTransform.Find("LockButton/Icon");

                bool isCompleted = completedSet.Contains(i);

                // Completed ⇒ show LockBtn, hide Icon
                // Not completed ⇒ hide LockBtn, show Icon
                if (lockBtn != null) lockBtn.gameObject.SetActive(isCompleted);
                if (icon != null) icon.gameObject.SetActive(!isCompleted);
            }
        }


        public void nextLevel()
        {
            int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
            int nextLevel = currentLevel + 1;
            Debug.Log("Current Level: " + currentLevel);
            Debug.Log("Next Level: " + nextLevel);
            HashSet<int> completedSet = new HashSet<int>(completedLevel);

            if (nextLevel > levelsParent.transform.childCount)
                return; // No next level to unlock


            string levelName = "Level_" + nextLevel;
            Transform levelTransform = levelsParent.transform.Find(levelName);

            bool isCompleted = completedSet.Contains(currentLevel);

            if (isCompleted)
            {
                Transform lockBtn = levelTransform.Find("LockButton/LockBtn");
                Transform icon = levelTransform.Find("LockButton/Icon");
                if (lockBtn != null) lockBtn.gameObject.SetActive(isCompleted);
                if (icon != null) icon.gameObject.SetActive(!isCompleted);
            }

        }


        public void LoadNextLevel()
        {
            int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
            int nextLevel = currentLevel + 1;

            if (nextLevel > levelsParent.transform.childCount)
            {
                Debug.Log("No more levels available!");
                return;
            }

            PlayerPrefs.SetInt("CurrentLevel", nextLevel);
            PlayerPrefs.Save();

            // Win state pauses the game (timeScale = 0). Reset before loading.
            Time.timeScale = 1f;
            if (MenuUiManager.Instance != null)
            {
                MenuUiManager.Instance.winPanel.SetActive(false);
                MenuUiManager.Instance.gameOverPanel.SetActive(false);
                MenuUiManager.Instance.StartGame();
            }

            // ProgressManager.AddPlayed(nextLevel);
            SceneManager.LoadScene("GamePlay");
        }

    }
}
