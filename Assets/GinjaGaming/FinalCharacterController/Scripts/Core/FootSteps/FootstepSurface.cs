using System.Linq;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core.FootSteps
{
    [CreateAssetMenu(fileName = "FootstepSurface", menuName = "Final Character Controller/Foot Step Surface", order = 1)]
    public class FootstepSurface : ScriptableObject
    {
        #region Class Variables
        [Header("Footstep Settings")]
        public bool spawnParticle;
        public bool spawnDecal;
        public string[] textureNames;
        public AudioClip[] audioClips;
        #endregion

        #region Class methods

        public bool ContainsTextureName(string textureName)
        {
            return textureNames.Contains(textureName);
        }
        #endregion
    }
}