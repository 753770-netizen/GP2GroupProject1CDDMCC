using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    public string role = "qb";          // qb, wr, or def
    public string team = "49ers";
    public float framesPerSecond = 10f;
    public float throwFramesPerSecond = 14f;
    public bool lockFacing;             // linemen: never turn around
    public bool faceRight = true;
    public bool startsFacingLeft;       // defenders stand facing the QB

    SpriteRenderer sr;
    Sprite[] frames = new Sprite[5];        // 0 = standing, 1 to 4 = run cycle
    Sprite[] throwFrames = new Sprite[4];   // only the QB has these
    Vector3 lastPos;
    float timer, throwTimer = -1f;
    int frame;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        lastPos = transform.position;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Game Manager may have chosen the teams b4 this runs
        if (frames[0] == null) SetTeam(team);
    }

    public void SetTeam(string newTeam)
    {
        team = newTeam;
        for (int i = 0; i < 5; i++)
            frames[i] = Resources.Load<Sprite>("Sprites/anim/" + role + "_" + team + "_" + i);
        for (int i = 0; i < 4; i++)
            throwFrames[i] = Resources.Load<Sprite>("Sprites/anim/" + role + "_" + team + "_throw_" + i);
        if (frames[0] != null) sr.sprite = frames[0];
    }

    public void PlayThrow()
    {
        if (throwFrames[0] == null) return;   // this player has no throw frames
        sr.flipX = false;                      // throw frames face right, toward the receivers
        throwTimer = 0f;
    }

    void LateUpdate()
    {
        if (frames[0] == null || Time.deltaTime <= 0f) return;

        Vector3 d = transform.position - lastPos;
        lastPos = transform.position;

        // A throw overrides standing and running until its last frame is done
        if (throwTimer >= 0f)
        {
            int f = Mathf.FloorToInt(throwTimer * throwFramesPerSecond);
            if (f < throwFrames.Length)
            {
                sr.sprite = throwFrames[f];
                throwTimer += Time.deltaTime;
                return;
            }
            throwTimer = -1f;
        }

        float dist = d.magnitude;
        bool moving = dist / Time.deltaTime > 0.3f && dist < 1.5f;   // a big jump is a play reset, not running
        if (!moving)
        {
            sr.sprite = frames[0];
            sr.flipX = lockFacing ? !faceRight : startsFacingLeft;
            timer = 0f; frame = 0;
            return;
        }

        timer += Time.deltaTime;
        if (timer >= 1f / framesPerSecond) { timer = 0f; frame = (frame + 1) % 4; }
        sr.sprite = frames[1 + frame];

        // Face the direction of travel (the sprites face right)
        if (lockFacing) sr.flipX = !faceRight;
        else if (Mathf.Abs(d.x) > 0.001f) sr.flipX = d.x < 0f;
    }
}
