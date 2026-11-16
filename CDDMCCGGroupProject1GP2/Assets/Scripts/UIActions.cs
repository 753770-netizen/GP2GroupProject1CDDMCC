using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIActions : MonoBehaviour
{
    public Image jerseyPreview;     // optional, used on the MainMenu

    public void LoadScene(string sceneName) { SceneManager.LoadScene(sceneName); }

    public void SetPersonality(int p) { GameState.I.personality = p; }

    public void SetJersey(string jersey)
    {
        GameState.I.jersey = jersey;
        if (jerseyPreview) jerseyPreview.sprite = Resources.Load<Sprite>("Sprites/qb_" + jersey);
    }

    public void StartTutorial()
    {
        GameState.I.tutorialMode = true;
        SceneManager.LoadScene("Game");
    }

    public void PlayCurrentGame()
    {
        GameState.I.tutorialMode = false;
        SceneManager.LoadScene("Game");
    }

    public void PlayAgain()
    {
        GameState.I.gameIndex = 0;
        SceneManager.LoadScene("MainMenu");
    }
}