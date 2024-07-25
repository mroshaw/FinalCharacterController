using UnityEngine;

namespace GinjaGaming.FinalCharacterController.FootSteps
{
    public class FootstepTrigger : CharacterTrigger
    {
        #region Class Variables
        private AudioSource _audioSource;
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

        public FootstepManager FootstepManager { get; set; }
        public override void TriggerEnter(Collider other)
        {
            Vector3 collisionPosition = other.ClosestPoint(transform.position);

            FootStepAudio footstepAudio =
                FootstepManager.GetFootStepForPosition(collisionPosition);

            if (footstepAudio == null)
            {
                return;
            }

            float terrainHeight =  Terrain.activeTerrain.SampleHeight(collisionPosition);
            Vector3 positionOnTerrain = new Vector3(collisionPosition.x, terrainHeight, collisionPosition.z);

            // Spawn particles
            if (footstepAudio.spawnParticle)
            {
                FootstepManager.SpawnFootStepParticleFx(positionOnTerrain, FootstepManager.transform.rotation);
            }
            // Spawn decal
            if (footstepAudio.spawnParticle)
            {
                FootstepManager.SpawnFootStepDecal(positionOnTerrain, FootstepManager.transform.rotation);
            }

            // Play audio
            System.Random randomAudio = new System.Random();
            int audioIndex = randomAudio.Next(0, footstepAudio.audioClips.Length);
            AudioClip audioClip = footstepAudio.audioClips[audioIndex];
            _audioSource.Stop();
            _audioSource.PlayOneShot(audioClip);
        }

        public override void TriggerExit(Collider other)
        {

        }

    }
}