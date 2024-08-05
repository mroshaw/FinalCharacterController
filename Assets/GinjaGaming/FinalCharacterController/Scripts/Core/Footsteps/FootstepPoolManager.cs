using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core.Footsteps
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