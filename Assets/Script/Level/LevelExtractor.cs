using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelExtractor : MonoBehaviour
{
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

}
