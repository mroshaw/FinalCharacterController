using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class CharacterAudio : MonoBehaviour
    {
        #region Class Variables

        [Header("Character Audio Settings")]
        [SerializeField] private AudioClip[] jumpAudioClips;
        [SerializeField] private AudioClip[] effortAudioClips;
        [SerializeField] private AudioClip[] hitAudioClips;
        [SerializeField] private AudioClip[] attackAudioClips;
        [SerializeField] private AudioClip[] attackShortAudioClips;
        [SerializeField] private AudioClip[] painAudioClips;
        [SerializeField] private AudioClip[] deathAudioClips;

        [Header("Environment Audio Settings")]
        [SerializeField] private AudioClip[] groundThudAudioClips;

        private AudioSource _audioSource;
        #endregion

        #region Startup
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }
        #endregion

        #region Class Methods

        public void PlayJumpAudio()
        {
            _audioSource.PlayOneShot(GetRandomAudioClip(jumpAudioClips));
        }

        public void PlayEffortAudio()
        {
            _audioSource.PlayOneShot(GetRandomAudioClip(effortAudioClips));
        }

        public void PlayHitAudio()
        {
            _audioSource.PlayOneShot(GetRandomAudioClip(hitAudioClips));
        }

        public void PlayShortAttackAudio()
        {
            _audioSource.PlayOneShot(GetRandomAudioClip(attackShortAudioClips));
        }

        public void PlayAttackAudio()
        {
            _audioSource.PlayOneShot(GetRandomAudioClip(attackAudioClips));
        }

        public void PlayPainAudio()
        {
            _audioSource.PlayOneShot(GetRandomAudioClip(painAudioClips));
        }

        public void PlayGroundThudAudio()
        {
            _audioSource.PlayOneShot(GetRandomAudioClip(groundThudAudioClips));
        }

        public void PlayDeathAudio()
        {
            _audioSource.PlayOneShot(GetRandomAudioClip(deathAudioClips));
        }

        private AudioClip GetRandomAudioClip(AudioClip[] audioClipArray)
        {
            if (audioClipArray.Length == 0)
            {
                return null;
            }
            System.Random randomClipRand = new System.Random();
            int randomIndex = randomClipRand.Next(0, audioClipArray.Length);
            return audioClipArray[randomIndex];

        }
        #endregion
    }
}