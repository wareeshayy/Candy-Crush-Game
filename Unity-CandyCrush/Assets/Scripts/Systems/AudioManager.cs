using UnityEngine;

namespace CandyCrush.Systems
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] AudioSource musicSource;
        [SerializeField] AudioSource sfxSource;
        [SerializeField] AudioClip menuMusic;
        [SerializeField] AudioClip gameMusic;
        [SerializeField] AudioClip matchSound;
        [SerializeField] AudioClip swapSound;
        [SerializeField] AudioClip invalidSwapSound;
        [SerializeField] AudioClip comboSound;
        [SerializeField] AudioClip gameOverSound;
        [SerializeField] AudioClip victorySound;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayMenuMusic()
        {
            PlayMusic(menuMusic);
        }

        public void PlayGameMusic()
        {
            PlayMusic(gameMusic);
        }

        void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null)
                return;

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void PlayMatch() => PlaySfx(matchSound);
        public void PlaySwap() => PlaySfx(swapSound);
        public void PlayInvalidSwap() => PlaySfx(invalidSwapSound);
        public void PlayCombo() => PlaySfx(comboSound);
        public void PlayGameOver() => PlaySfx(gameOverSound);
        public void PlayVictory() => PlaySfx(victorySound);

        void PlaySfx(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
                sfxSource.PlayOneShot(clip);
        }

        public void SetMusicVolume(float volume)
        {
            if (musicSource != null)
                musicSource.volume = volume;
        }

        public void SetSfxVolume(float volume)
        {
            if (sfxSource != null)
                sfxSource.volume = volume;
        }
    }
}
