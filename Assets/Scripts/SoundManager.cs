using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private AudioSource audioSource;

    public AudioClip cardFlip;
    public AudioClip cardPick;
    public AudioClip cardSelect;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public static void Play(AudioClip audio)
    {
        Instance.audioSource.PlayOneShot(audio);
    }
}
