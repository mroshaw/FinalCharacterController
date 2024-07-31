using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GinjaGaming.FinalCharacterController.Core
{
    public class CharacterHealth : MonoBehaviour
    {
        #region Class Variables
        [Header("Default Settings")]
        [SerializeField] private float maxHealth = 100.0f;
        [SerializeField] private float startingHealth = 100.0f;
        [SerializeField] [Tooltip("A second trigger will fire, to reset a character for example, after the number of seconds specified here")] private float delayHealthGoneTriggerWait = 5.0f;
        [SerializeField] private float _currentHealth;
        [SerializeField] private bool _isDead;

        [Header("Events")]
        public UnityEvent<float> healthChangedEvent;
        public UnityEvent healthDecreasedEvent;
        public UnityEvent healthIncreasedEvent;
        public UnityEvent healthGoneEvent;
        public UnityEvent healthGoneDelayedEvent;

        #endregion

        #region Startup

        private void Awake()
        {
            _currentHealth = startingHealth;
        }
        #endregion

        #region Class Methods

        public void ModifyHealth(float healthAmount)
        {
            if (healthAmount > 0)
            {
                IncreaseHealth(healthAmount);
            }
            else
            {
                DecreaseHealth(-healthAmount);
            }
        }

        public void DecreaseHealth(float healthAmount)
        {
            _currentHealth -= healthAmount;
            healthDecreasedEvent.Invoke();
            HealthChanged();
        }

        public void IncreaseHealth(float healthAmount)
        {
            _currentHealth = (_currentHealth + healthAmount > maxHealth ? maxHealth : _currentHealth + healthAmount);
            healthIncreasedEvent.Invoke();
            HealthChanged();
        }

        private void HealthGone()
        {
            _isDead = true;
            healthGoneEvent.Invoke();
            StartCoroutine(CallHealthGoneDelayEventAfterDelay());

        }

        public bool IsDead()
        {
            return _isDead;
        }

        private void HealthChanged()
        {
            if (_currentHealth <= 0)
            {
                HealthGone();
            }
            healthChangedEvent.Invoke(_currentHealth);
        }

        private IEnumerator CallHealthGoneDelayEventAfterDelay()
        {
            yield return new WaitForSeconds(delayHealthGoneTriggerWait);
            healthGoneDelayedEvent.Invoke();
        }
        #endregion
    }
}