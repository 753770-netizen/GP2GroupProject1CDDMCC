using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; //Our awesome QB
    // Update is called once per frame
    void LateUpdate()
    {
        Transform t = GameManager.I.BallCarrier ? GameManager.I.BallCarrier : target;
        float x = Mathf.Lerp(transform.position.x, t.position.x + 2f, 5f * Time.deltaTime);
        transform.position = new Vector3(x, 0, -10);
    }
}
