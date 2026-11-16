using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchupCard : MonoBehaviour
{
    public GameObject panel;
    public Image background, qbImage, wrImage, defImage;
    public TMP_Text title;
    public float showSeconds = 2.5f;

    void OnDestroy() { Time.timeScale = 1f; }

    IEnumerator Start()
    {
        // No card when testing without GameState, or during the tutorial
        if (GameState.I == null || GameState.I.tutorialMode) { panel.SetActive(false); yield break; }

        var m = GameState.I.Current;
        string team = m.opponent.ToLower();
        bool playoffs = m.round.Contains("NFC") || m.round.Contains("Finale");
        title.text = (playoffs ? "PLAYOFFS\n" : "") + m.round.ToUpper() + "\n49ERS vs " + m.opponent.ToUpper();
        background.color = m.opponentColor;
        qbImage.sprite = Resources.Load<Sprite>("Sprites/qb_" + team);
        wrImage.sprite = Resources.Load<Sprite>("Sprites/wr_" + team);
        defImage.sprite = Resources.Load<Sprite>("Sprites/def_" + team);

        panel.SetActive(true);
        Time.timeScale = 0f;                                              // freeze the game
        yield return new WaitForSecondsRealtime(playoffs ? 3.5f : showSeconds);
        Time.timeScale = 1f;
        panel.SetActive(false);
    }
}
