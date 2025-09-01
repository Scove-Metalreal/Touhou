using UnityEngine;

namespace _Project._Scripts.Core
{
    [System.Serializable]
    public class Sound
    {
        public string name; // Tên để gọi âm thanh

        public AudioClip clip; // File âm thanh

        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(.1f, 3f)]
        public float pitch = 1f;

        public bool loop = false;

        [HideInInspector]
        public AudioSource source;
    }
}