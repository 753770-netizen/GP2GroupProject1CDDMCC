using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    [Header("Scene objects")]
    public QB qb;
    public Ball ball;
    public Receiver[] receivers;
    public Defender[] defenders;
    public Blocker[] blockers;
    public TMP_Text scoreboardText;

    [Header("Rules")]
    public float endZoneX = 50f;
    public float quarterLength = 60f;
    public float difficulty = 1f;
    public float defenderScale = 1f;     // Hype lowers this to slow the defense

    [Header("State (read only)")]
    public Transform BallCarrier;
    public bool playLive;
    public int down = 1, quarter = 1, score, opponentScore;
    public float lineOfScrimmage, firstDownX, clock;

    float startX;
    Vector2 qbOffset;
    Vector2[] recOffsets, defOffsets, blkOffsets;
    bool gameOver;
    int tutorialPlays;

    bool Tutorial => GameState.I != null && GameState.I.tutorialMode;

    void Awake()
    { 
        I = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Remember where everyone starts, relative to the line of scrimmage
        startX = qb.transform.position.x;
        lineOfScrimmage = startX;
        Vector2 los = new Vector2(lineOfScrimmage, 0);
        qbOffset = (Vector2)qb.transform.position - los;

        recOffsets = new Vector2[receivers.Length];
        for (int i = 0; i < receivers.Length; i++)
            recOffsets[i] = (Vector2)receivers[i].transform.position - los;

        defOffsets = new Vector2[defenders.Length];
        for (int i = 0; i < defenders.Length; i++)
            defOffsets[i] = (Vector2)defenders[i].transform.position - los;

        blkOffsets = new Vector2[blockers.Length];
        for (int i = 0; i < blockers.Length; i++)
            blkOffsets[i] = (Vector2)blockers[i].transform.position - los;

        // Pick up the opponent and your jersey from GameState (skipped if there isn't one)
        if (GameState.I != null)
        {
            var m = GameState.I.Current;
            difficulty = m.difficulty;
            LoadSprites(m.opponent.ToLower());
        }

        clock = quarterLength;
        firstDownX = lineOfScrimmage + 10;
        ResetPlay();
    }

    void LoadSprites(string opponent)
    {
        string jersey = GameState.I.jersey;
        Apply(qb.gameObject, jersey, "qb");
        foreach (var r in receivers) Apply(r.gameObject, jersey, "wr");
        foreach (var d in defenders) Apply(d.gameObject, opponent, "def");
        foreach (var b in blockers) Apply(b.gameObject, jersey, "def");
    }

    void Apply(GameObject go, string team, string role)
    {
        var anim = go.GetComponent<SpriteAnimator>();
        if (anim != null) { anim.role = role; anim.SetTeam(team); return; }

        // No animator on this object: fall back to the standing sprite
        var sp = Resources.Load<Sprite>("Sprites/" + role + "_" + team);
        if (sp != null) go.GetComponent<SpriteRenderer>().sprite = sp;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameOver) return;

        // Touchdown check for the ball carrier or a running QB
        Transform carrier = BallCarrier ? BallCarrier : (qb.ballInHand ? qb.transform : null);
        if (playLive && carrier != null && carrier.position.x >= endZoneX) Tackle(endZoneX);

        // The clock only runs during live plays (and not in the tutorial)
        if (playLive && !Tutorial)
        {
            clock -= Time.deltaTime;
            if (clock <= 0f) EndQuarter();
        }

        if (scoreboardText)
        {
            string opp = GameState.I != null ? GameState.I.Current.opponent.ToUpper() : "OPP";
            string q = quarter > 4 ? "OT" : "Q" + quarter;
            int toGo = Mathf.Max(0, Mathf.RoundToInt(firstDownX - lineOfScrimmage));
            scoreboardText.text = "49ERS " + score + "  -  " + opponentScore + " " + opp + "\n" +
                                  q + "  " + Mathf.CeilToInt(Mathf.Max(0f, clock)) + "s    Down " + down + "    " + toGo + " to go";
        }
    }

    // ---------- Play results (called by Ball, Defender, and Update) ----------
    public void Tackle(float x)
    {
        if (!playLive) return;
        if (x >= endZoneX)
        {
            score += 7;
            if (Hype.I != null) Hype.I.Add(0.5f);
            EndPlay(true);                 // after a touchdown the opponent gets a drive
            return;
        }
        lineOfScrimmage = x;
        if (x >= firstDownX)
        {
            down = 1; firstDownX = x + 10;
            if (Hype.I != null) Hype.I.Add(0.15f);
        }
        else down++;
        EndPlay(down > 4);                 // 4 downs without a first down: turnover
    }

    public void Incomplete()
    {
        if (!playLive) return;
        down++;
        EndPlay(down > 4);
    }

    public void Interception()
    {
        if (!playLive) return;
        EndPlay(true);
    }

    // ---------- Flow ----------
    void EndPlay(bool turnover)
    {
        playLive = false;
        BallCarrier = null;
        qb.ballInHand = false;
        StartCoroutine(AfterPlay(turnover));
    }

    IEnumerator AfterPlay(bool turnover)
    {
        yield return new WaitForSeconds(1.2f);
        if (gameOver) yield break;

        if (turnover)
        {
            if (!Tutorial) OpponentDrive();
            lineOfScrimmage = startX;      // restart at your own end
            down = 1;
            firstDownX = lineOfScrimmage + 10;
        }

        if (Tutorial)
        {
            tutorialPlays++;
            if (tutorialPlays >= 3)
            {
                GameState.I.tutorialMode = false;
                GameState.I.tutorialDone = true;
                SceneManager.LoadScene("MainMenu");
                yield break;
            }
        }
        ResetPlay();
    }

    // The defense is simulated, so the opponent scores based on difficulty
    void OpponentDrive()
    {
        float roll = Random.value;
        if (roll < 0.22f * difficulty) opponentScore += 7;
        else if (roll < 0.40f * difficulty) opponentScore += 3;
    }

    void ResetPlay()
    {
        Vector2 los = new Vector2(lineOfScrimmage, 0);
        qb.transform.position = los + qbOffset;
        qb.ballInHand = true;
        qb.bulletMode = false;

        for (int i = 0; i < receivers.Length; i++)
        {
            receivers[i].transform.position = los + recOffsets[i];
            receivers[i].ResetPlay();
        }
        for (int i = 0; i < defenders.Length; i++)
        {
            defenders[i].transform.position = los + defOffsets[i];
            defenders[i].ResetPlay();
        }
        for (int i = 0; i < blockers.Length; i++)
        {
            blockers[i].transform.position = los + blkOffsets[i];
            blockers[i].ResetPlay();
        }

        ball.ResetBall(qb.transform.position);
        BallCarrier = null;
        playLive = true;
    }

    void EndQuarter()
    {
        // After Q4 the game ends unless it is tied (then overtime repeats until someone leads)
        if (quarter >= 4 && score != opponentScore) { EndGame(); return; }
        quarter++;
        clock = quarter > 4 ? 30f : quarterLength;
    }

    void EndGame()
    {
        gameOver = true;
        playLive = false;
        bool won = score > opponentScore;
        Debug.Log(won ? "YOU WIN" : "YOU LOSE");
        if (GameState.I != null)
        {
            if (won) GameState.I.GameWon(); else GameState.I.GameLost();
        }
    }
}
