using UnityEngine;
using TMPro;

public class TextScreen : MonoBehaviour
{
    public enum Mode { Static, Result, GameOver, Ending }
    public Mode mode;
    public TMP_Text body;
    [TextArea] public string staticText;
    public GameObject popup;           // optional: an icon that pops up (Draft screen)
    public float popupDelay = 1.5f;

    void Start()
    {
        var gs = GameState.I;
        switch (mode)
        {
            case Mode.Static:
                body.text = staticText;
                if (popup) { popup.SetActive(false); Invoke(nameof(ShowPopup), popupDelay); }
                break;

            case Mode.Result:
                // gameIndex was already advanced, so Current is the NEXT game
                body.text = ResultLine(gs.gameIndex - 1) + "\n\nNext up: " + gs.Current.round + " vs " + gs.Current.opponent.ToUpper();
                break;

            case Mode.GameOver:
                body.text = "Your season is over.\nYou lost to the " + gs.Current.opponent + " in " + gs.Current.round + ".";
                break;

            case Mode.Ending:
                string[] endings =
                {
                    "You confidently led the 49ers to the Retro Cup!",
                    "Humble to the end, you lifted the Retro Cup for your team.",
                    "The joker became a champion. Retro Cup winner!"
                };
                body.text = endings[Mathf.Clamp(gs.personality, 0, 2)];
                break;
        }
    }

    void ShowPopup() { popup.SetActive(true); }

    string ResultLine(int wonIndex)
    {
        switch (wonIndex)
        {
            case 5: return "Congratulations! You win the Divisional Round!";
            case 6: return "You win the NFC trophy!";
            default: return "Victory!";
        }
    }
}
