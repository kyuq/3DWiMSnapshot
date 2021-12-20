// Comment this line if no spatial audio is needed (see: https://valvesoftware.github.io/steam-audio/)
#define STEAM_SPATIALAUDIO

using UnityEngine;
namespace Magnorama
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance = null;

        public AudioClip Ambient;
        public AudioClip Capture;
        public AudioClip Take;
        public AudioClip Put;

        [Header("Debug")]
        public bool TriggerCapture = false;
        public bool TriggerTake = false;
        public bool TriggerGrab = false;

        private AudioSource AmbientPlayer;
        private GameObject AmbientSound;

        private AudioSource SoundEffectPlayer;
        private GameObject SoundEffect;


        void Start()
        {
            Instance = this;

            AmbientSound = new GameObject("AmbientSound");
            AmbientSound.transform.parent = transform;
            AmbientPlayer = AmbientSound.AddComponent<AudioSource>();
#if STEAM_SPATIALAUDIO
            AmbientSound.AddComponent<SteamAudio.SteamAudioAmbisonicSource>();
#endif

            AmbientPlayer.clip = Ambient;
            AmbientPlayer.loop = true;
            AmbientPlayer.Play();

            SoundEffect = new GameObject("SoundEffect");
            SoundEffect.transform.parent = transform;
            SoundEffectPlayer = SoundEffect.AddComponent<AudioSource>();
            SoundEffectPlayer.spatialize = true;
            SoundEffectPlayer.spatialBlend = 1;

        }

        void Update()
        {
            if (TriggerCapture)
            {
                TriggerCapture = false;
                if (SoundEffectPlayer.isPlaying) SoundEffectPlayer.Stop();
                SoundEffectPlayer.PlayOneShot(Capture, 0.5f);
            }

            if (TriggerTake)
            {
                TriggerTake = false;
                if (SoundEffectPlayer.isPlaying) SoundEffectPlayer.Stop();
                SoundEffectPlayer.PlayOneShot(Take, 0.5f);
            }

            if (TriggerGrab)
            {
                TriggerGrab = false;
                if (SoundEffectPlayer.isPlaying) SoundEffectPlayer.Stop();
                SoundEffectPlayer.PlayOneShot(Put, 0.5f);
            }
        }



        public static void PlayCapture(Vector3 position)
        {
            if (Instance && Instance.Capture)
            {
                Instance.SoundEffect.transform.position = position;
                if (Instance.SoundEffectPlayer.isPlaying) Instance.SoundEffectPlayer.Stop();
                Instance.SoundEffectPlayer.PlayOneShot(Instance.Capture, 0.5f);
            }
        }

        public static void PlayTake(Vector3 position)
        {
            if (Instance && Instance.Take)
            {
                Instance.SoundEffect.transform.position = position;
                if (Instance.SoundEffectPlayer.isPlaying) Instance.SoundEffectPlayer.Stop();
                Instance.SoundEffectPlayer.PlayOneShot(Instance.Take, 0.25f);
            }
        }

        public static void PlayPut(Vector3 position)
        {
            if (Instance && Instance.Put)
            {
                Instance.SoundEffect.transform.position = position;
                if (Instance.SoundEffectPlayer.isPlaying) Instance.SoundEffectPlayer.Stop();
                Instance.SoundEffectPlayer.PlayOneShot(Instance.Put, 0.25f);
            }
        }
    }

}
