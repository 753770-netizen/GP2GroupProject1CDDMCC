using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DialogueScene : MonoBehaviour
{
    public TMP_Text speaker, line;
    public GameObject choicePanel;      // holds the 3 personality buttons
    [TextArea] public string[] lines;   // each line written as  Name: what they say
    int i;

    void Start() { choicePanel.SetActive(false); Show(); }

    void Update()
    {
        if (choicePanel.activeSelf) return;
        var p = Pointer.current;
        if (p != null && p.press.wasPressedThisFrame) Next();
    }

    void Next()
    {
        i++;
        if (i >= lines.Length)
        {
            speaker.text = "";
            line.text = "How do you introduce yourself?";
            choicePanel.SetActive(true);
        }
        else Show();
    }

    void Show()
    {
        var parts = lines[i].Split(new[] { ':' }, 2);
        speaker.text = parts[0];
        line.text = parts[1].Trim();
    }
}
