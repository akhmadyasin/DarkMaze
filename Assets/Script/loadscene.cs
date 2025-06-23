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
        Time.timeScale = 1f;
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
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
}
