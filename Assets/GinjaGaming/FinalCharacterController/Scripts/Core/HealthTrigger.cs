using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core
{
    public class HealthTrigger : CharacterTrigger
    {
        [SerializeField] private float healthModifier;
        [SerializeField] private bool isContinous;
        [SerializeField] private bool destroyAfterApply;
        #region Class Methods
        protected override void TriggerEnter(Collider other)
        {
            CharacterHealth characterHealth = other.GetComponent<CharacterHealth>();
            if (characterHealth)
            {
                characterHealth.ModifyHealth(healthModifier);
                if (destroyAfterApply)
                {
                    Destroy(gameObject);
                }
            }
        }

        protected override void TriggerExit(Collider other)
        {
        }
        #endregion
    }
}