using System;
using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Animation Sound Event")]
    [HelpBox("This component is used to play sound clips from Animation Events.")]
    [RequireComponent(typeof(AudioSource))]
    public class AnimationSoundEvent : MonoBehaviour
    {
        [Serializable]
        public struct SoundEvent
        {
            public string Name;
            public SoundClip Sound;
        }

        public SoundEvent[] SoundEvents;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void PlaySound(string name)
        {
            foreach (var sound in SoundEvents)
            {
                if(sound.Name == name)
                {
                    audioSource.PlayOneShotSoundClip(sound.Sound);
                    break;
                }
            }
        }
    }
}