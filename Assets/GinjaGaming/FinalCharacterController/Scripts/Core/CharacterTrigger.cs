using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace GinjaGaming.FinalCharacterController.Core
{
    public abstract class CharacterTrigger : MonoBehaviour
    {
        #region Class Variables
        [Header("Trigger Settings")]
        [SerializeField] [Tooltip("Trigger will only fire if the collider has any one of these tags.")] private string[] triggerTags;
        [SerializeField] [Tooltip("Trigger will only fire if the collider is on any one of these layers.")] private LayerMask triggerLayers;

        [Header("Events")]
        [SerializeField] private UnityEvent<Collider> triggerEnterEvent;
        [SerializeField] private UnityEvent<Collider> triggerExitEvent;
        #endregion

        #region Startup
        /// <summary>
        /// Configure the component on awake
        /// </summary>   
        private void Awake()
        {
            if (!GetComponent<Collider>())
            {
                Debug.LogError($"CharacterTrigger: There is no collider on this gameobject! {gameObject}");
            }
        }
        #endregion

        private void OnTriggerEnter(Collider other)
        {
            if (CollisionIsValid(other))
            {
                TriggerEnter(other);
                triggerEnterEvent.Invoke(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (CollisionIsValid(other))
            {
                TriggerExit(other);
                triggerExitEvent.Invoke(other);
            }
        }

        private bool CollisionIsValid(Collider other)
        {
            // Compare tags
            if (triggerTags.Length == 0 || triggerTags.Contains(other.tag))
            {
                // Compare Layers
                if (triggerLayers == 0 || ((1 << other.gameObject.layer) & triggerLayers) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        protected abstract void TriggerEnter(Collider other);
        protected abstract void TriggerExit(Collider other);
    }
}