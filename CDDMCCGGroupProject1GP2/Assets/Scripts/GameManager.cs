using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using JetBrains.Annotations;

public class GameManager : MonoBehaviour
{
    private const int V = 3;
    public static GameManager I;

    [Header("Scene objects")]
    public QB qb;
    public Ball ball;
    public Receiver[] receivers;
    public Defender[] defenders;
    public TMP_Text scoreboardText;

    [Header("Rules")]
    public float endZoneX = 50f;
    public float quarterLength = 60f;
    public float difficulty = 1f;
    public float defenderScale = 1f; //Hype will lower this to slow the defense

    [Header("State (read only)")]
    public Transform BallCarrier;
    public bool playLive;
    public int down = 1, quarter = 1, score, opponentScore;
    public float lineOfScrimmage, firstDownX, clock;

    float startX;
    Vector2 qbOffset;
    Vector2[] recOffsets, defOffsets;
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
        //Remember that everyone starts where the line of scrimmage is
        startX = qb.transform.position.x;
        Vector2 los = new Vector2(lineOfScrimmage, 0);
        qbOffset = (Vector2)qb.transform.position - los;

        recOffsets = new Vector2[receivers.Length];
        for (int i = 0; i < receivers.Length; i++) recOffsets[i] = (Vector2)receivers[i].transform.position - los;

        defOffsets = new Vector2[defenders.Length];
        for (int i = 0; i < defenders.Length; i++) defOffsets[i] = (Vector2)defenders[i].transform.position - los;

        //Pick up the opponent and your jersey from the GameState, will skip if there isn't one
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
        SetSprite(qb.gameObject, "Sprites/qb_" + jersey);
        foreach (var r in receivers) SetSprite(r.gameObject, "Sprites/wr_" + jersey);
        foreach (var d in defenders) SetSprite(d.gameObject, "Sprites/def_" + opponent);
    }

    void SetSprite(GameObject go, string path)
    {
        var sp = Resources.Load<Sprite>(path);
        if (sp != null) go.GetComponent<SpriteRenderer>().sprite = sp;
    }

    private TMP_Text GetScoreboardText1()
    {
        return scoreboardText;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {
            //Touchdowm check for the ball carrier or a running QB
            Transform carrier = BallCarrier ? BallCarrier : (qb.ballInHand ? qb.transform : null);
            if (playLive && carrier != null && carrier.position.x >= endZoneX) Tackle(endZoneX);

            //The clock only runs during live plays and not in tut
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
                scoreboardText.text = "49ers" + score + "  -  " + opponentScore + " " + opp + "\n" + q + " " + Mathf.CeilToInt(Mathf.Max(0f, clock)) + "s   Down" + down + "    " + toGo + " to go";
            }
        }
    }

    // play resulted called by ball, defender, and update
    public void Tackle(float x)
    {
        if (!playLive) return;
        if (x >= endZoneX)
        {
            score += 7;
            if (Hype.I != null) Hype.I.Add(0.5f);
            EndPlay(true);
            //After TD opponent gets a drive
            return;
            {
                lineOfScrimmage = x;
                if (x >= firstDownX)
                {
                    down = 1; firstDownX = x + 10;
                    if (Hype.I != null) Hype.I.Add(0.15f);
                }
                else down++;
                Endplay(down > 4);
                //4 downs without a first down: turnover
            }
        }
    }

    private void EndPlay(bool v)
    {
        throw new System.NotImplementedException();
    }

    public void Incomplete()
    {
        if (!playLive) return;
        down++;
        Endplay(down > 4);
    }
    public void Interception()
    {
        if (playLive) return;
        Endplay(true);
    }

    //------Flow boi-------
    void Endplay(bool turnover)
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
            lineOfScrimmage = startX; //restarting at our own end to make it fair
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

    private void OpponentDrive()
    {
        throw new System.NotImplementedException();
    }

    //the defense is stimmed, so the opponent scores based on difficulty
    void OpponentDrice()
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

        for (int i = 0; i < receivers.Length; i++)
        {
            receivers[i].transform.position = los + recOffsets[i];
            receivers[i].ResetPlay();
        }
        for (int i = 0; i < defenders.Length; i++) defenders[i].transform.position = los + defOffsets[i];

        ball.ResetBall(qb.transform.position);
        BallCarrier = null;
        playLive = true;
    }
    void EndQuarter()
    {
        //After Q4 the game ends unless its tied, then we go into OT
        if (quarter >= 4 && score != opponentScore)
        {
            EndGame(); return;
        }
            quarter++;
            clock = quarter > 4 ? 30f : quarterLength;
    }

    void EndGame()
    {
        gameOver = true;
        playLive = false;
        bool won = score > opponentScore;
        Debug.Log(won ? "YOU WIN!" : "YOU LOSE");
        if (GameState.I != null)
        {
            if (won) GameState.I.GameWon(); else GameState.I.GameLost();
        }
    }
}
