using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Android;

public class Ball : MonoBehaviour
{
    float flightTime = 0.8f;
    Vector2 start, target;
    Receiver targetReceiver;
    bool inFlight, isBullet;
    private int t;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Throw(Vector2 from, Receiver r, bool bullet)
    {
        start = from; targetReceiver = r; isBullet = bullet;
        flightTime = bullet ? 0.45f : 0.8f;
        //if confident, the throw will be 15% faster
        if (GameState.I != null && GameState.I.personality == 0) flightTime *= 0.85f;
        target = r.PredictPosition(flightTime);
        t = 0; inFlight = true; 
    }

    public void ResetBall(Vector2 pos)
    {
        inFlight = false;
        transform.SetParent(null);
        transform.position = pos;
        t = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(inFlight)
        {
            //Before the ball is thrown by the QB, its riding with them
            if (transform.parent == null && GameManager.I.qb.ballInHand) transform.position = GameManager.I.qb.transform.position;
            return;
        }

        t += (int)(Time.deltaTime / flightTime);
        Vector2 pos = Vector2.Lerp(start, target, t);
        pos.y += Mathf.Sin(t * Mathf.PI) * 1.5f;
        transform.position = pos;
        if ( t >= 1f)
        {
            inFlight = false; Resolve();
        }

        void Resolve()
        {
            bool covered = false;
            foreach (var d in FindObjectsByType<Defender>(FindObjectsSortMode.None)) if (Vector2.Distance(d.transform.position, target) < 0.8f) covered = true;
            float pickChance = (covered ? 0.35f : 0f) * (isBullet ? 0.5f : 1f);
            float catchChance = (covered ? 0.65f : 0.9f) - (isBullet ? 0.15f : 0f);
            //If they're humble: +10% catch chance
            if (GameState.I != null && GameState.I.personality == 1) catchChance += 0.10f;
            float roll = Random.value;
            if (roll < pickChance) GameManager.I.Interception();
            else if (roll < catchChance)
            {
                targetReceiver.CatchBall(this);
                if (Hype.I != null) Hype.I.Add(0.1f);
            }
                else GameManager.I.Incomplete();

        }
    }
}
