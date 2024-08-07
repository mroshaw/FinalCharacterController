using GinjaGaming.FinalCharacterController.Core.Utils;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core.CharacterController.Footsteps
{
    public class FootstepPoolManager : MonoBehaviour
    {
        #region Class Variables

        [Header("Pools")]
        [SerializeField] private PrefabPool particlePool;
        [SerializeField] private PrefabPool footprintPool;

        public PrefabPool ParticlePool => particlePool;
        public PrefabPool FootprintPool => footprintPool;
        #endregion
    }
}