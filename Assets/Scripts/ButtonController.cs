
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    [SerializeField] private Image soundIcon;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    private bool isSoundOn = false;
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

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        if (isSoundOn)
        {
            soundIcon.sprite = soundOnSprite;
            // Bật âm thanh trong game
            AudioListener.volume = 1f;
        }
        else
        {
            soundIcon.sprite = soundOffSprite;
            // Tắt âm thanh trong game
            AudioListener.volume = 0f;
        }
    }


}