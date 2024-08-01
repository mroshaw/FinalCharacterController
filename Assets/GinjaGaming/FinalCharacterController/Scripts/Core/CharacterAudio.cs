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
        [SerializeField] private AudioClip[] reliefAudioClips;

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
            PlayAudio(jumpAudioClips);
        }

        public void PlayEffortAudio()
        {
            PlayAudio(effortAudioClips);
        }

        public void PlayReliefAudio()
        {
            PlayAudio(reliefAudioClips);
        }

        public void PlayHitAudio()
        {
            PlayAudio(hitAudioClips);
        }

        public void PlayShortAttackAudio()
        {
            PlayAudio(attackShortAudioClips);
        }

        public void PlayAttackAudio()
        {
            PlayAudio(attackAudioClips);
        }

        public void PlayPainAudio()
        {
            PlayAudio(painAudioClips);
        }

        public void PlayGroundThudAudio()
        {
            PlayAudio(groundThudAudioClips);
        }

        public void PlayDeathAudio()
        {
            PlayAudio(deathAudioClips);
        }

        private void PlayAudio(AudioClip[] audioClips)
        {
            AudioClip audioClip = GetRandomAudioClip(audioClips);
            if (audioClip)
            {
                _audioSource.PlayOneShot(audioClip);
            }
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