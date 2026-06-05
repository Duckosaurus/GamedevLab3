using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.3f;

    void Start()
    {
        AudioSource source = GetComponent<AudioSource>();
        source.clip = musicClip;
        source.loop = true;
        source.volume = volume;
        source.playOnAwake = false;
        source.Play();
    }
}
