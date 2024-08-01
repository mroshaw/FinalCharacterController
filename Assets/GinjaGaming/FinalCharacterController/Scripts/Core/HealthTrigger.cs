using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core
{
    public class HealthTrigger : CharacterTrigger
    {
        #region Class Variables
        [SerializeField] private float healthModifier;
        [SerializeField] [Tooltip("Set this to true to apply continuous damage while in the collider.")] private bool isContinuous;
        [SerializeField] private float continuousDelay = 2.0f;
        [SerializeField] [Tooltip("Set this to true to destroy the parent GameObject after applying the health modifier.")] private bool destroyAfterApply;

        private bool _isInTrigger;
        private CharacterHealth _characterHealth;
        private float _delayTimer;

        #endregion

        #region Class Methods
        protected override void TriggerEnter(Collider other)
        {
            _characterHealth = other.GetComponent<CharacterHealth>();
            if (_characterHealth)
            {
                _isInTrigger = true;
                _characterHealth.ModifyHealth(healthModifier);
                if (destroyAfterApply)
                {
                    _isInTrigger = false;
                    _characterHealth = null;
                    Destroy(gameObject);
                }
            }
        }

        protected override void TriggerExit(Collider other)
        {
            _isInTrigger = false;
            _characterHealth = null;
        }

        protected override void TriggerStay(Collider other)
        {
            _isInTrigger = true;
        }

        #region Update
        private void Update()
        {
            if (!_isInTrigger || !isContinuous)
            {
                return;
            }

            if (_delayTimer < continuousDelay)
            {
                _delayTimer += Time.deltaTime;
                return;
            }

            if (_delayTimer >= continuousDelay)
            {
                _characterHealth.ModifyHealth(healthModifier);
                _delayTimer = 0;
            }
        }
        #endregion

        #endregion
    }
}