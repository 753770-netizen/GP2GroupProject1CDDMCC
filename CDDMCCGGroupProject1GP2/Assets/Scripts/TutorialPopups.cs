using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TutorialPopups : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text text;

    string[] tips =
    {
        "Receivers run routes. TAP a receiver to throw to them!",
        "Hold and drag anywhere to RUN with the QB. Defenders will chase you!",
        "Tap BULLET, then a receiver, for a fast but risky pass.",
        "Get a first down within 4 plays. Score touchdowns. Win the Retro Cup!"
    };
    int i;
    bool active;

    void OnDestroy() { Time.timeScale = 1f; }

    void Start()
    {
        if (GameState.I == null || !GameState.I.tutorialMode)
        {
            panel.SetActive(false);
            enabled = false;
            return;
        }
        active = true;
        panel.SetActive(true);
        text.text = tips[0];
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (!active) return;
        var p = Pointer.current;
        if (p == null || !p.press.wasPressedThisFrame) return;

        i++;
        if (i >= tips.Length)
        {
            active = false;
            panel.SetActive(false);
            Time.timeScale = 1f;
        }
        else text.text = tips[i];
    }
}
