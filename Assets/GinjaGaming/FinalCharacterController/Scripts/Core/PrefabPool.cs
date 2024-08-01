using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace GinjaGaming.FinalCharacterController.Core
{
    /// <summary>
    /// Implements a generic 'Prefab Poo' using Unity Pooling. This allows for 're-use' of a number of prefab instances
    /// that avoid unnecessary garbage generation and collection.
    /// </summary>
    public class PrefabPool : MonoBehaviour
    {
        #region Class Variables

        [Header("Prefab Settings")]
        [SerializeField] private GameObject poolPrefab;
        [SerializeField] private Transform instanceContainer;
        [SerializeField] private float lifeTimeInSeconds;
        [Header("Pool Settings")]
        [SerializeField] private int poolMaxSize = 5;
        [SerializeField] private int poolInitialSize = 10;
        [Header("Debug")]
        [SerializeField] private int poolActiveCountDebug;
        [SerializeField] private int poolInactiveCountDebug;

        private ObjectPool<GameObject> _prefabInstancePool;
        #endregion

        #region Startup
        private void Start()
        {
            _prefabInstancePool = new ObjectPool<GameObject>(CreatePrefabInstancePoolItem, PrefabInstanceOnTakeFromPool,
                PrefabInstanceOnReturnToPool, PrefabInstanceOnDestroyPoolObject, false,
                poolInitialSize, poolMaxSize);
        }
        #endregion

        #region Update
        private void Update()
        {
            poolActiveCountDebug = _prefabInstancePool.CountActive;
            poolInactiveCountDebug = _prefabInstancePool.CountInactive;
        }
        #endregion

        #region Class methods
        public GameObject SpawnInstance(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            GameObject prefabInstance = _prefabInstancePool.Get();
            prefabInstance.transform.position = spawnPosition;
            prefabInstance.transform.rotation = spawnRotation;
            StartCoroutine(ReturnToPoolAfterDelay(prefabInstance));
            return prefabInstance;
        }

        private IEnumerator ReturnToPoolAfterDelay(GameObject prefabInstance)
        {
            yield return new WaitForSeconds(lifeTimeInSeconds);
            _prefabInstancePool.Release(prefabInstance);
        }

        private GameObject CreatePrefabInstancePoolItem()
        {
            GameObject prefabInstance = Instantiate(poolPrefab, instanceContainer, false);
            prefabInstance.name = $"{poolPrefab.name}-New";
            return prefabInstance;
        }

        private void PrefabInstanceOnTakeFromPool(GameObject prefabInstance)
        {
            prefabInstance.name = $"{poolPrefab.name}-Taken";
            prefabInstance.SetActive(true);
        }

        private void PrefabInstanceOnReturnToPool(GameObject prefabInstance)
        {
            prefabInstance.name = $"{poolPrefab.name}-Returned";
            prefabInstance.transform.position = Vector3.zero;
            prefabInstance.transform.rotation = Quaternion.identity;
            prefabInstance.SetActive(false);
        }

        private void PrefabInstanceOnDestroyPoolObject(GameObject prefabInstance)
        {
            Destroy(prefabInstance);
        }
        #endregion
    }
}