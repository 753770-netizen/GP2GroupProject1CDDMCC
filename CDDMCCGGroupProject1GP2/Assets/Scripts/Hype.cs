using UnityEngine;
using UnityEngine.UI;

public class Hype : MonoBehaviour
{
    public static Hype I;
    public Slider meterBar;            // UI slider from 0 to 1
    public float duration = 5f;
    float fillMultiplier = 1f, meter, activeTimer;

    public bool Active => activeTimer > 0f;

    void Awake()
    { 
        I = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //If he's feeling like he's got some jokes: hype fills 50% faster foshooo
        if (GameState.I != null && GameState.I.personality == 2) fillMultiplier = 1.5f;
    }

    public void Add(float amount)
    {
        if (!Active) meter = Mathf.Clamp01(meter + amount * fillMultiplier);
    }

    // Hook this to the Hype button's OnClick
    public void Activate()
    {
        if (meter < 1f || Active) return;
        meter = 0f; activeTimer = duration;
        GameManager.I.defenderScale = 0.4f;   // defenders move in slow motion
    }

    // Update is called once per frame
    void Update()
    {
        if (Active)
        {
            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0f) GameManager.I.defenderScale = 1f;
        }
        if (meterBar) meterBar.value = Active ? activeTimer / duration : meter;
    }
}
