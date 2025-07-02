using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadscene : MonoBehaviour
{
    public void ChangeScene(string scene)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (nextIndex > unlocked)
        {
            PlayerPrefs.SetInt("UnlockedLevel", nextIndex);
        }

        // Pindah ke scene berikutnya
        SceneManager.LoadScene(nextIndex);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("mainmenu");
    }

    public void quitGame()
    {
        Application.Quit();
    }
    public void paused()
    {
        Time.timeScale = 0;
    }
    public void resume()
    {
        Time.timeScale = 1;
    }
    public void RestartLevel()
    {
        Time.timeScale = 1f; // Kembalikan kecepatan waktu
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Restart scene yang sedang aktif
    }

    // Tambahan untuk cek apakah scene (level) ini boleh dimainkan
    public bool IsLevelUnlocked(int levelIndex)
    {
        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
        return levelIndex <= unlocked;
    }
}
