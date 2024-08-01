using System.Linq;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core.Footsteps
{
    /// <summary>
    /// A container ScriptableObject for creating instances of 'texture name' driven surfaces, that have a corresponding
    /// list of AudioClips. These instances are registered in the FootstepManager, and used to derive audio and visual
    /// effects based on the primary texture of a given footstep collision.
    /// </summary>
    [CreateAssetMenu(fileName = "FootstepSurface", menuName = "Final Character Controller/Foot Step Surface", order = 1)]
    public class FootstepSurface : ScriptableObject
    {
        #region Class Variables
        [Header("Footstep Settings")]
        [SerializeField] private bool spawnParticle;
        [SerializeField] private bool spawnFootprint;
        [SerializeField] private string[] textureNames;
        [SerializeField] private AudioClip[] audioClips;
        #endregion

        // Public getters, to protect the instances from being modified outside of the inspector
        public bool SpawnParticle => spawnParticle;
        public bool SpawnFootprint => spawnFootprint;
        public AudioClip[] AudioClips => audioClips;

        #region Class methods
        public bool ContainsTextureName(string textureName)
        {
            return textureNames.Contains(textureName);
        }

        public AudioClip GetRandomAudioClip()
        {
            if (audioClips.Length == 0)
            {
                return null;
            }
            System.Random randomAudio = new System.Random();
            return audioClips[randomAudio.Next(0, audioClips.Length)];
        }

        #endregion
    }
}