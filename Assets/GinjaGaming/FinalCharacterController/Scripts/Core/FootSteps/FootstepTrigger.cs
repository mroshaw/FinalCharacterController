using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core.Footsteps
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

        protected override void TriggerEnter(Collider other)
        {
            FootstepManager.GetSurfaceFromCollision(transform, other, out FootstepSurface footstepSurface,
                out Vector3 spawnPosition);

            // Spawn particles
            if (footstepSurface.SpawnParticle)
            {
                FootstepManager.SpawnFootStepParticleFx(spawnPosition, FootstepManager.transform.rotation);
            }

            // Spawn decal
            if (footstepSurface.SpawnFootprint)
            {
                FootstepManager.SpawnFootprint(spawnPosition, FootstepManager.transform.rotation);
            }

            // Play random audio

            _audioSource.Stop();
            _audioSource.PlayOneShot(footstepSurface.GetRandomAudioClip());
        }

        protected override void TriggerExit(Collider other)
        {
        }

        protected override void TriggerStay(Collider other)
        {
        }
        #endregion
    }
}