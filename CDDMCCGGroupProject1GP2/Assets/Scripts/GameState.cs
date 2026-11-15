using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Matchup
{
    public string round, opponent;
    public float difficulty;
    public Color opponentColor;
    public Matchup(string r, string o, float d, Color c)
    {
        round = r; opponent = o; difficulty = d; opponentColor = c;
    }
}
public class GameState : MonoBehaviour
{
    public static GameState I;

    public int gameIndex;
    public int personality;
    public string jersey = "49ers";
    public bool tutorialDone, tutorialMode;
    public Matchup[] schedule;

    public Matchup Current => schedule[Mathf.Min(gameIndex, schedule.Length - 1)];

    static Color C(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
    // Update is called once per frame
    void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject); return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        schedule = new Matchup[]
        {
            new Matchup("Week 1", "Cardinals", 0.80f, C("#97233f")), new Matchup("Week 2", "Rams", 0.85f, C("#003594")), new Matchup("Week 3", "Cowboys", 0.90f, C("#6b7480")), new Matchup("Week 4", "Packers", 0.95f, C("#203731")), new Matchup("Week 5", "Commanders", 1.00f, C("5a1414")), new Matchup("NFC Divisional Round", "Eagles", 1.10f, C("#004c54")), new Matchup("NFC Championship", "Lions", 1.20f, C("#0076b6")), new Matchup("Retro Cup Finale!", "Seahawks", 1.30f, C("#002244")),
        };
    }

    public void GameWon()
    {
        gameIndex++;
        SceneManager.LoadScene(gameIndex >= schedule.Length ? "Ending" : "Result");
    }

    public void GameLost()
    {
        SceneManager.LoadScene("GameOver");
    }
}
