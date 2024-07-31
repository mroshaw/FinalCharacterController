using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core.FootSteps
{
    public class FootstepTrigger : CharacterTrigger
    {
        #region Class Variables
        private AudioSource _audioSource;

        public FootstepManager FootstepManager { get; set; }
        #endregion

        #region Startup
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            if (!_audioSource)
            {
                Debug.LogError($"FootstepTrigger: no AudioSource on this gameobject! {gameObject}");
            }
        }
        #endregion

        #region Class methods
        public override void TriggerEnter(Collider other)
        {
            FootstepManager.GetSurfaceFromCollision(transform, other, out FootstepSurface footstepSurface,
                out Vector3 spawnPosition);

            // Spawn particles
            if (footstepSurface.spawnParticle)
            {
                FootstepManager.SpawnFootStepParticleFx(spawnPosition, FootstepManager.transform.rotation);
            }

            // Spawn decal
            if (footstepSurface.spawnDecal)
            {
                FootstepManager.SpawnFootStepDecal(spawnPosition, FootstepManager.transform.rotation);
            }

            // Play random audio
            System.Random randomAudio = new System.Random();
            int audioIndex = randomAudio.Next(0, footstepSurface.audioClips.Length);
            AudioClip audioClip = footstepSurface.audioClips[audioIndex];
            _audioSource.Stop();
            _audioSource.PlayOneShot(audioClip);
        }

        public override void TriggerExit(Collider other)
        {
        }
        #endregion
    }
}