using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    #region SFX

    public AudioSource sfxASource;
    public AudioSource bgmSource;

    public float masterVolume = 1;
    public float sfxVolume;
    public float bgmVolume;

    public AudioClip[] Bounces;
    public AudioClip[] Death;
    public AudioClip[] Pause;
    public AudioClip[] Replay;
    public AudioClip[] StartSound;
    public AudioClip Rotate;

    #endregion

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        sfxASource.volume = masterVolume * sfxVolume;
        bgmVolume = masterVolume * bgmVolume;
    }

    public void UpdateMasterVolume(float volume)
    {
        masterVolume = volume;
        sfxASource.volume = masterVolume * sfxVolume;
        bgmSource.volume = masterVolume * bgmVolume;
    }

    public void PlayBounce()
    {
        sfxASource.PlayOneShot(Bounces[Random.Range(0, Bounces.Length)]);
    }

    public void PlayDeath()
    {
        sfxASource.PlayOneShot(Death[Random.Range(0, Death.Length)]);
    }

    public void PlayPause()
    {
        sfxASource.PlayOneShot(Pause[Random.Range(0, Pause.Length)]);
    }

    public void PlayReplay()
    {
        sfxASource.PlayOneShot(Replay[Random.Range(0, Replay.Length)]);
    }

    public void PlayStart()
    {
        sfxASource.PlayOneShot(StartSound[Random.Range(0, StartSound.Length)]);
    }

    public void PlayRotate()
    {
        sfxASource.PlayOneShot(Rotate);
    }

    private void HoldAudio()
    {
        sfxASource.Stop();
    }

    public void ToggleSound(int state)
    {
        UpdateMasterVolume(state);
    }
}
