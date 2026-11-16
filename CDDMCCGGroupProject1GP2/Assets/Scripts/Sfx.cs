using UnityEngine;

public static class Sfx
{
    public static void Play(AudioClip clip)
    {
        if (clip != null) AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }
}
