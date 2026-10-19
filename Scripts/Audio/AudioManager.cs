using System.Collections.Generic;
using NS.Core;
using NS.Utility;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace NS.Audio
{
    [DefaultExecutionOrder(-20)]
    public class AudioManager : LocalSingleton<AudioManager>
    {
        public enum Bus
        {
            Master,
            Music,
            Sfx
        }

        [SerializeField] private SoundEffect[] sfx;
        [SerializeField] private AudioSource audioSourceTemplate;
        [SerializeField] private Transform sfxTransform;
        [SerializeField] private AudioMixer mixer;

        [SerializeField] private bool audioDebug;

        private readonly Dictionary<string, SoundEffect> soundEffects = new(); // keyed by sound id
        private readonly Dictionary<string, AudioSource[]> audioSources = new(); // keyed by sound id
        private readonly Dictionary<string, float> lastPlayTime = new (); // keyed by sound id

        protected override void Awake()
        {
            base.Awake();

            foreach (SoundEffect soundEffect in sfx)
            {
                soundEffects.Add(soundEffect.id, soundEffect);
                lastPlayTime.Add(soundEffect.id, 0);

                var sources = new AudioSource[soundEffect.simultaneousLimit];
                var soundParent = new GameObject($"{soundEffect.id}");
                soundParent.transform.SetParent(sfxTransform);
                for (int i = 0; i < soundEffect.simultaneousLimit; i++)
                {
                    var source = Instantiate(audioSourceTemplate, soundParent.transform);
                    source.gameObject.name = i.ToString();
                    sources[i] = source;
                }

                audioSources.Add(soundEffect.id, sources);
            }
        }

        private void Start()
        {
            // load from prefs
            SetVolume01(Bus.Master, GetVolume01(Bus.Master));
            SetVolume01(Bus.Music, GetVolume01(Bus.Music));
            SetVolume01(Bus.Sfx, GetVolume01(Bus.Sfx));
            SetIsMuted(IsMuted());
        }

        private bool TryGetAudioSource(string id, bool interrupt, out AudioSource source)
        {
            float mostTimePlayed = 0;
            int mostTimePlayedIndex = -1;
            for (int i = 0; i < audioSources[id].Length; i++)
            {
                AudioSource audioSource = audioSources[id][i];
                if (audioSource.isPlaying)
                {
                    // we interrupt the one that has been playing for the longest time
                    if (interrupt && audioSource.time > mostTimePlayed)
                    {
                        mostTimePlayed = audioSource.time;
                        mostTimePlayedIndex = i;
                    }

                    continue;
                }

                source = audioSource;
                return true;
            }

            if (interrupt)
            {
                source = audioSources[id][mostTimePlayedIndex];
                return true;
            }

            source = null;
            return false;
        }

        public bool TryPlaySoundEffect(string id, bool interrupt = true)
        {
            if (soundEffects.TryGetValue(id, out SoundEffect sound) == false)
            {
                Debug.LogWarning($"[AudioManager] Failed to find a sound with id: {id}");
                return false;
            }

            if (TryGetAudioSource(id, interrupt && sound.isInterruptable, out AudioSource source) == false)
            {
                return false;
            }

            float timeSinceLastPlayed = Time.time - lastPlayTime[sound.id];
            if (timeSinceLastPlayed < sound.cooldown)
            {
                return false;
            }

            source.volume = sound.baseVolume + Random.Range(-sound.volumeVariance, +sound.volumeVariance);
            source.pitch = sound.basePitch + Random.Range(-sound.pitchVariance, +sound.pitchVariance);
            source.clip = sound.clips.RandomElement();
            source.Play();

            lastPlayTime[sound.id] = Time.time;

#if UNITY_EDITOR
            if (audioDebug)
            {
                Debug.Log($"[AudioManager] Play sound at {Time.time}: {sound.id} (vol: {source.volume}, pitch: {source.pitch})");
            }
#endif

            return true;
        }

        public void SetVolume01(Bus bus, float volume01)
        {
            string paramName = $"{bus}Volume";
            PlayerPrefs.SetFloat($"AudioManager.{paramName}", volume01);

            float decibels = LinearToDecibels(volume01);
            mixer.SetFloat(paramName, decibels);
        }

        public float GetVolume01(Bus bus)
        {
            string paramName = $"{bus}Volume";
            return PlayerPrefs.GetFloat($"AudioManager.{paramName}", 1.0f);
        }

        public void SetIsMuted(bool mute)
        {
            const string paramName = "MuteAll";
            PlayerPrefs.SetInt($"AudioManager.{paramName}", mute? 1 : 0);

            mixer.SetFloat(paramName, mute? -80 : 0);
        }

        public bool IsMuted()
        {
            return PlayerPrefs.GetInt("AudioManager.MuteAll", 0) == 1;
        }

        private static float LinearToDecibels(float value01)
        {
            float decibels = -80f;

            value01 = Mathf.Clamp01(value01);
            if (value01 > 0)
            {
                decibels = 20.0f * Mathf.Log10(value01);
            }

            return decibels;
        }
    }
}
