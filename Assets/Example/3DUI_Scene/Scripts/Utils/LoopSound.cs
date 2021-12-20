using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopSound : MonoBehaviour
{
    public AudioClip Audio;

    [Range(0, 1)]
    public float Volume = 0.5f;
    [Range(0, 3)]
    public float Pitch = 1f;
    public float MinDistance = 0.5f;
    public float MaxDistance = 1f;


    public float PauseBetweenLoop = 0.1f;
    private AudioSource Source;

    // Start is called before the first frame update
    void Start()
    {
        Source = gameObject.AddComponent<AudioSource>();
        Source.spatialize = true;
        Source.spatialBlend = 1;
        Source.rolloffMode = AudioRolloffMode.Linear;

        StartCoroutine(PlaySoundLoop());
    }

    private IEnumerator PlaySoundLoop()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0, PauseBetweenLoop));
        while(true)
        {
            yield return new WaitUntil(() => !Source.isPlaying);
            yield return new WaitForSeconds(PauseBetweenLoop);
            Source.minDistance = MinDistance;
            Source.maxDistance = MaxDistance;
            Source.pitch = Pitch;
            if (Audio) Source.PlayOneShot(Audio, Volume);
        }
    }
}
