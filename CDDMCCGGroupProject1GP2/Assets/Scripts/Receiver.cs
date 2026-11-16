using UnityEngine;

public class Receiver : MonoBehaviour
{
    public float speed = 4f;
    public Vector2[] route;      // offsets from the starting spot
    public bool hasBall;
    int idx;
    Vector2 origin, vel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        origin = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasBall)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
            return;
        }
        if (route == null || idx >= route.Length) { vel = Vector2.zero; return; }

        Vector2 goal = origin + route[idx];
        vel = (goal - (Vector2)transform.position).normalized * speed;
        transform.position = Vector2.MoveTowards(transform.position, goal, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, goal) < 0.05f) idx++;
    }

    // Where will this receiver be in t seconds? The QB throws there.
    public Vector2 PredictPosition(float t) => (Vector2)transform.position + vel * t;

    public void CatchBall(Ball b)
    {
        hasBall = true;
        b.transform.SetParent(transform);
        b.transform.localPosition = Vector3.zero;
        b.transform.localRotation = Quaternion.identity;
        GameManager.I.BallCarrier = transform;
    }

    public void ResetPlay()
    {
        origin = transform.position;
        idx = 0; hasBall = false; vel = Vector2.zero;
    }
}
