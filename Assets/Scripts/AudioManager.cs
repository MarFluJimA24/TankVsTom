using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayMusicLoop(AudioClip clip, float volume = 0.5f)
    {
        if (clip == null || musicSource == null) return;

        // Si ya está sonando esa misma música, no reinicies
        if (musicSource.isPlaying && musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
        musicSource.clip = null;
    }
}
