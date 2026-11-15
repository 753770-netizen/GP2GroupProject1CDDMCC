using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class QB : MonoBehaviour
{
    public Ball ball;
    public float runSpeed = 4f;
    public bool ballInHand = true;
    public bool bulletMode; //Gotta switch with the bullet button mode

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ToggleBulletMode()
    {
        bulletMode = !bulletMode;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f || ballInHand) return;
        var p = Pointer.current;
        if (p == null) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; //ignoring the taps on buttons

        Vector2 wp = Camera.main.ScreenToWorldPoint(p.position.ReadValue());

        if (p.press.wasPressedThisFrame)
        {
            var hit = Physics2D.OverlapPoint(wp);
            if (hit && hit.CompareTag("Receiver"))
            {
                ball.Throw(transform.position, hit.GetComponent<Receiver>(), bulletMode);
                ballInHand = false;
                bulletMode = false;
                return;
            }
        }
        //Hold and drag 2 run
        if (p.press.isPressed) transform.position = Vector2.MoveTowards(transform.position, wp, runSpeed * Time.deltaTime);
    }
}
