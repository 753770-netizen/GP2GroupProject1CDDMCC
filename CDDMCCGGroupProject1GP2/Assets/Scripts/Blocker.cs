using UnityEngine;

public class Blocker : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float blockStrength = 1f;     // higher = holds longer
    public float holdTime = 2.2f;        // seconds he can hold a defender of equal strength
    public float engageRange = 0.7f;
    public float pushBackSpeed = 0.5f;   // how fast a rusher drives him backward
    public float maxSlide = 1.5f;        // how far back he can be driven

    Defender target, engagedWith;
    Vector2 home;
    float holdTimer;
    bool beaten;

    public void ResetPlay()
    {
        Release();
        home = transform.position;
        target = null;
        beaten = false;
    }

    // Update is called once per frame
    void Update()
    {
        var gm = GameManager.I;
        if (!gm.playLive) return;

        // After the throw the pass rush is over: let everyone go
        if (!gm.qb.ballInHand) { Release(); return; }
        if (beaten) return;

        if (engagedWith != null) { HoldBlock(); return; }

        if (target == null || target.IsEngaged) target = FindTarget();
        if (target == null) return;

        // Pass set: hold the line and slide sideways to meet the rusher
        Vector2 tp = target.transform.position;
        Vector2 goal = new Vector2(home.x, Mathf.Clamp(tp.y, home.y - 1.8f, home.y + 1.8f));
        transform.position = Vector2.MoveTowards(transform.position, goal, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, tp) < engageRange) Engage(target);
    }

    void Engage(Defender d)
    {
        engagedWith = d;
        d.engagedBy = this;
        // Tougher opponents (higher difficulty) beat blocks faster
        holdTimer = holdTime * (blockStrength / d.rushStrength) / GameManager.I.difficulty * Random.Range(0.8f, 1.2f);
    }

    void HoldBlock()
    {
        holdTimer -= Time.deltaTime;

        // The pocket slowly collapses toward the QB
        float push = pushBackSpeed * (engagedWith.rushStrength / blockStrength) * Time.deltaTime;
        if (transform.position.x > home.x - maxSlide)
        {
            transform.position += Vector3.left * push;
            engagedWith.transform.position += Vector3.left * push;
        }

        if (holdTimer <= 0f) { Release(); beaten = true; }   // the rusher breaks free
    }

    void Release()
    {
        if (engagedWith != null)
        {
            engagedWith.engagedBy = null;
            engagedWith = null;
        }
    }

    Defender FindTarget()
    {
        Defender best = null;
        float bestDist = 999f;
        foreach (var d in GameManager.I.defenders)
        {
            if (d.job != Defender.Job.Rusher || d.IsEngaged) continue;
            float dist = Vector2.Distance(transform.position, d.transform.position);
            if (dist < bestDist) { best = d; bestDist = dist; }
        }
        return best;
    }
}
