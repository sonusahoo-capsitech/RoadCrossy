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

        public GameObject levelsParent;
        public static LevelExtractor Instance;

        private bool buttonsInitialized = false;
        private Button[] cachedButtons;
        private readonly Dictionary<int, Transform> levelByNumber = new Dictionary<int, Transform>(64);
        private readonly HashSet<int> tempLevelSet = new HashSet<int>();

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
            if (buttonsInitialized || levelsParent == null) return;

            CacheLevelTransforms();
            cachedButtons = levelsParent.GetComponentsInChildren<Button>(true);

            for (int i = 0; i < cachedButtons.Length; i++)
            {
                Button btn = cachedButtons[i];
                int levelNumber = ParseLevelNumber(btn.transform);
                if (levelNumber <= 0) continue;

                int capturedLevel = levelNumber;
                btn.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetInt("CurrentLevel", capturedLevel);
                    PlayerPrefs.Save();
                    StartLevel();
                    SceneManager.LoadScene("GamePlay");

                });
            }

            buttonsInitialized = true;
        }


        public void StartLevel()
        {
            MenuUiManager.Instance.play.gameObject.SetActive(false);
            MenuUiManager.Instance.levelSelectPanel.gameObject.SetActive(false);
            MenuUiManager.Instance.Background.gameObject.SetActive(false);
        }


        public void UpdateLevelLocks()
        {
            if (levelsParent == null) return;
            if (!buttonsInitialized) SetupLevelButtons();
            tempLevelSet.Clear();
            tempLevelSet.UnionWith(played);

            // Loop through all available levels (1 to 99)
            for (int i = 1; i <= levelsParent.transform.childCount; i++)
            {
                if (!levelByNumber.TryGetValue(i, out Transform levelTransform))
                {
                    continue;
                }

                Transform lockBtn = levelTransform.Find("LockButton/LockBtn");
                Transform icon = levelTransform.Find("LockButton/Icon");

                bool isCompleted = tempLevelSet.Contains(i);

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
            tempLevelSet.Clear();
            tempLevelSet.UnionWith(completedLevel);

            if (nextLevel > levelsParent.transform.childCount)
                return; // No next level to unlock


            bool isCompleted = tempLevelSet.Contains(currentLevel);

            if (isCompleted)
            {
                if (!levelByNumber.TryGetValue(nextLevel, out Transform levelTransform))
                {
                    return;
                }

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

        void CacheLevelTransforms()
        {
            levelByNumber.Clear();
            Transform parent = levelsParent.transform;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                int levelNumber = ParseLevelNumberFromName(child.name);
                if (levelNumber > 0)
                {
                    levelByNumber[levelNumber] = child;
                }
            }
        }

        int ParseLevelNumber(Transform buttonTransform)
        {
            if (buttonTransform == null) return -1;
            Transform parent = buttonTransform.parent;
            Transform levelRoot = parent != null ? parent.parent : null;
            if (levelRoot == null) return -1;

            return ParseLevelNumberFromName(levelRoot.name);
        }

        int ParseLevelNumberFromName(string name)
        {
            const string prefix = "Level_";
            if (string.IsNullOrEmpty(name) || !name.StartsWith(prefix))
            {
                return -1;
            }

            string number = name.Substring(prefix.Length);
            if (int.TryParse(number, out int levelNumber))
            {
                return levelNumber;
            }

            return -1;
        }

    }
}
