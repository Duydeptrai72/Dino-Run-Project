using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] AudioSource effectsSource;
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip tapClip;
    [SerializeField] AudioClip hurtClip;
    private bool hasPlayedEffectSound = false;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public bool HasPlayEffectSound()
    {
        return hasPlayedEffectSound;
    }
    public void SetHasPlayEffectSound(bool value)
    {
        hasPlayedEffectSound = value;
    }
    void Start()
    {
        effectsSource.Stop();
        hasPlayedEffectSound = true;    
    }
    public void PlayJumpClip()
    {
        if (effectsSource != null) effectsSource.PlayOneShot(jumpClip);
    }
    public void PlayTapClip()
    {
        if (effectsSource != null) effectsSource.PlayOneShot(tapClip);
    }
    public void PlayHurtClip()
    {
        if (effectsSource != null) effectsSource.PlayOneShot(hurtClip);
    }
}
