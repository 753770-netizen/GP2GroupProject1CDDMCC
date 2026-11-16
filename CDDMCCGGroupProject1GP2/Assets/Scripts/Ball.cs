using UnityEngine;

public class Ball : MonoBehaviour
{
    public AudioClip throwSound, catchSound;

    float flightTime = 0.8f;
    Vector2 start, target;
    Receiver targetReceiver;
    float t;
    bool inFlight, isBullet;

    public void Throw(Vector2 from, Receiver r, bool bullet)
    {
        start = from; targetReceiver = r; isBullet = bullet;
        flightTime = bullet ? 0.45f : 0.8f;
        // Confident perk: every throw is 15% faster
        if (GameState.I != null && GameState.I.personality == 0) flightTime *= 0.85f;
        target = r.PredictPosition(flightTime);
        t = 0; inFlight = true;
        Sfx.Play(throwSound);
    }

    public void ResetBall(Vector2 pos)
    {
        inFlight = false;
        transform.SetParent(null);
        transform.rotation = Quaternion.identity;
        transform.position = pos;
        t = 0;
    }

    // Update is called once per frame
    void Update()
     {
        if (!inFlight)
        {
            // Before the throw, the ball rides with the QB
            if (transform.parent == null && GameManager.I.qb.ballInHand)
                transform.position = GameManager.I.qb.transform.position;
            return;
        }

        t += Time.deltaTime / flightTime;
        Vector2 pos = Vector2.Lerp(start, target, t);
        pos.y += Mathf.Sin(t * Mathf.PI) * 1.5f;   // the arc
        Vector2 dir = pos - (Vector2)transform.position;
        if (dir.sqrMagnitude > 0.0001f) transform.right = dir;   // nose follows the flight path
        transform.position = pos;
        if (t >= 1f) { inFlight = false; Resolve(); }
    }

    void Resolve()
    {
        bool covered = false;
        foreach (var d in FindObjectsByType<Defender>(FindObjectsSortMode.None))
            if (Vector2.Distance(d.transform.position, target) < 0.8f) covered = true;

        float pickChance = (covered ? 0.35f : 0f) * (isBullet ? 0.5f : 1f);
        float catchChance = (covered ? 0.65f : 0.9f) - (isBullet ? 0.15f : 0f);
        // Humble perk: +10% catch chance
        if (GameState.I != null && GameState.I.personality == 1) catchChance += 0.10f;

        float roll = Random.value;
        if (roll < pickChance) GameManager.I.Interception();
        else if (roll < catchChance)
        {
            targetReceiver.CatchBall(this);
            Sfx.Play(catchSound);
            if (Hype.I != null) Hype.I.Add(0.1f);
        }
        else GameManager.I.Incomplete();
    }
}
