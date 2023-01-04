using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public bool hasPauseMenu;
    public GameObject pauseMenu;
    public GameObject board;

    public void Update()
    {
        if (!hasPauseMenu)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            board.SetActive(!board.activeSelf);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(1);
    }

    public void Play()
    {
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
