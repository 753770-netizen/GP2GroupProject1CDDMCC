using UnityEditor.Build.Content;
using UnityEngine;

public class Defender : MonoBehaviour
{
    public float baseSpeed = 3f;
    public QB qb;
   
    // Update is called once per frame
    void Update()
    {
        if (!GameManager.I.playLive) return;
        Transform carrier = GameManager.I.BallCarrier;
        bool qbHasBall = qb.ballInHand;

        Vector2 goal;
        if (carrier != null) goal = carrier.position; //Chasing our receivers
        else if (qbHasBall) goal = qb.transform.position; //Rushing the qb fosho
        else goal = GameManager.I.ball.transform.position; //Passing in the air, making the defenders going for the ball fosho
        float speed = baseSpeed * GameManager.I.difficulty * GameManager.I.defenderScale;
        transform.position = Vector2.MoveTowards(transform.position, goal, speed * Time.deltaTime);

        //Only tackling the person who has the ball foshoo
        if ((carrier != null || qbHasBall) && Vector2.Distance(transform.position, goal) < 0.4f) GameManager.I.Tackle(goal.x);

    }
}
