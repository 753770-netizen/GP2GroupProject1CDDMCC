using UnityEngine;

public class Defender : MonoBehaviour
{
    public enum Job { Rusher, Cover }

    public Job job = Job.Rusher;
    public float baseSpeed = 3f;
    public float rushStrength = 1f;         // higher = beats blockers faster
    public float coverSpeedBonus = 1.25f;   // coverage defenders run a bit faster
    public float coverOffset = 0.4f;        // how close a cover defender stays to his receiver
    public QB qb;
    public Receiver coverTarget;            // only used when the job is Cover

    [HideInInspector] public Blocker engagedBy;

    public bool IsEngaged => engagedBy != null;

    public void ResetPlay()
    { 
        engagedBy = null; 
    }

    // Update is called once per frame
    void Update()
    {
        var gm = GameManager.I;
        if (!gm.playLive || IsEngaged) return;   // a blocked defender is held in place by his blocker

        Transform carrier = gm.BallCarrier;
        bool qbHasBall = qb.ballInHand;
        bool someoneHasBall = carrier != null || qbHasBall;

        Vector2 goal;
        if (carrier != null) goal = carrier.position;                        // everyone pursues the ball carrier
        else if (!qbHasBall) goal = gm.ball.transform.position;              // pass in the air: go for the ball
        else if (job == Job.Cover && coverTarget != null)
            goal = (Vector2)coverTarget.transform.position + Vector2.right * coverOffset;   // shadow the receiver
        else goal = qb.transform.position;                                   // rushers hunt the QB

        float speed = baseSpeed * gm.difficulty * gm.defenderScale;
        if (job == Job.Cover && carrier == null && qbHasBall) speed *= coverSpeedBonus;

        transform.position = Vector2.MoveTowards(transform.position, goal, speed * Time.deltaTime);

        // Tackle whoever has the ball
        if (someoneHasBall)
        {
            Vector2 holder = carrier != null ? (Vector2)carrier.position : (Vector2)qb.transform.position;
            if (Vector2.Distance(transform.position, holder) < 0.4f) gm.Tackle(holder.x);
        }
    }
}
