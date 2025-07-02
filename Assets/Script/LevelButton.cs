using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    public int levelBuildIndex;
    public Color unlockedColor = Color.white;
    public Color lockedColor = Color.gray;

    private Button button;
    private Image buttonImage;

    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (levelBuildIndex <= unlocked)
        {
            // Level terbuka
            button.interactable = true;
            buttonImage.color = unlockedColor;

            button.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(levelBuildIndex);
            });
        }
        else
        {
            // Level terkunci
            button.interactable = false;
            buttonImage.color = lockedColor;
        }
    }
}
