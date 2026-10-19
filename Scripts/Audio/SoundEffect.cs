using UnityEngine;

namespace NS.Audio
{
    [CreateAssetMenu(menuName = "Game/Audio/SFX", fileName = "new SoundEffect")]
    public class SoundEffect : ScriptableObject
    {
        public string id;
        public float baseVolume = 1.0f;
        public float basePitch = 1.0f;
        public float volumeVariance = 0.1f;
        public float pitchVariance = 0.1f;
        public int simultaneousLimit = 1;
        public bool isInterruptable = true;
        public float cooldown = 0.05f;
        public AudioClip[] clips;
    }
}
