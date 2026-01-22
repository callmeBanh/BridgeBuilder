using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    public void GotoHome()
    {
        SceneManager.LoadScene("StartGame");
    }

    public void BackToGameplay()
    {
        SceneManager.LoadScene("GamePlay");
    }
    public void ExitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}