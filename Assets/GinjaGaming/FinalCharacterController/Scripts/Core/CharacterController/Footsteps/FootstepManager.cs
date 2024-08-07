using GinjaGaming.FinalCharacterController.Core.Extensions;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core.CharacterController.Footsteps
{
    public class FootstepManager : MonoBehaviour
    {
        #region Class Variables

        [Header("Settings")]
        [SerializeField] private LayerMask triggerLayerMask;
        [SerializeField] private FootstepTrigger[] footstepTriggers;
        [SerializeField] private FootstepSurface defaultSurface;
        [SerializeField] private FootstepSurface[] footstepSurfaces;

        [Header("Spawn Settings")] [Tooltip("Tick this to align spawned footprint decals to the terrain slope.")] public bool alignToTerrainSlope;

        [Header("Pool Settings")]
        [SerializeField] private FootstepPoolManager poolManager;

        [Header("Debug")][SerializeField] private bool debugTextureName;

        private TerrainData _terrainData;
        private bool _terrainDetected;
        #endregion

        #region Startup
        private void Awake()
        {
            // Register the triggers
            foreach (FootstepTrigger trigger in footstepTriggers)
            {
                trigger.FootstepManager = this;
            }

            _terrainDetected = !(Terrain.activeTerrain == null);

            if (_terrainDetected)
            {
                _terrainData = Terrain.activeTerrain.terrainData;
            }
        }
        #endregion
        #region Class methods
        public void SetPoolManager(FootstepPoolManager newPoolManager)
        {
            poolManager = newPoolManager;
        }
        public void SpawnFootStepParticleFx(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (!poolManager.ParticlePool)
            {
                return;
            }
            poolManager.ParticlePool.SpawnInstance(spawnPosition, spawnRotation);
        }

        public void SpawnFootprint(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (!poolManager.FootprintPool)
            {
                return;
            }

            poolManager.FootprintPool.SpawnInstance(spawnPosition, spawnRotation);
        }

        /// <summary>
        /// Using the collider collision information, try to identify either a Terrain or a Mesh from
        /// which we can derive a texture. Using that texture, we can lookup the available FootstepSurfaces in order
        /// to play an appropriate AudioClip and spawn a particle and decal.
        /// </summary>
        public void GetSurfaceFromCollision(Transform footTransform, Collider otherCollider,
            out FootstepSurface footstepSurface, out Vector3 spawnPosition)
        {
            if (otherCollider is TerrainCollider)
            {
                Vector3 collisionPosition = footTransform.position;
                if (!FindTerrainTextureAtPosition(footTransform.position, out var terrainTextureName))
                {
                    footstepSurface = defaultSurface;
                    spawnPosition = collisionPosition;
                }
                float terrainHeight = Terrain.activeTerrain.SampleHeight(collisionPosition);
                spawnPosition = new Vector3(collisionPosition.x, terrainHeight, collisionPosition.z);
                footstepSurface = FindSurfaceFromTexture(terrainTextureName);
                return;

            }
            spawnPosition = otherCollider is MeshCollider { convex: true } or BoxCollider or SphereCollider or CapsuleCollider ? otherCollider.ClosestPoint(footTransform.position) : footTransform.position;

            if (FindMaterialTextureFromCollider(otherCollider, out var meshTextureName))
            {
                footstepSurface = FindSurfaceFromTexture(meshTextureName);
                return;
            }
            footstepSurface = defaultSurface;
            spawnPosition = footTransform.position;
        }

        /// <summary>
        /// Iterates across the registered FootstepSurface instances to see if one matches the provided textureName. If
        /// found, then the FootstepSurface instance is returned, otherwise null.
        /// </summary>
        private FootstepSurface FindSurfaceFromTexture(string textureName)
        {
            foreach (FootstepSurface currSurface in footstepSurfaces)
            {
                if (currSurface.ContainsTextureName(textureName) && currSurface.AudioClips.Length > 0)
                {
                    return currSurface;
                }
            }
            return defaultSurface;
        }

        /// <summary>
        /// Tries to find the primary texture on the material of a (non-terrain) Mesh or primitive collider.
        /// </summary>
        private bool FindMaterialTextureFromCollider(Collider other, out string textureName)
        {
            if (other.isTrigger)
            {
                textureName = "";
                return false;
            }

            MeshRenderer meshRender = other.GetComponent<MeshRenderer>();
            if (!meshRender)
            {
                textureName = "";
                return false;
            }
            Material meshMaterial = meshRender.material;
            if (!meshMaterial)
            {
                textureName = "";
                return false;
            }
            textureName = meshMaterial.mainTexture.name;
            if (debugTextureName)
            {
                Debug.Log($"FootstepManager: Mesh texture is : {textureName}");
            }
            return true;
        }

        /// <summary>
        /// Uses TerrainData and terrain splat maps to find the 'primary' layer texture at the point of the
        /// collision with the footstep trigger.
        /// </summary>
        /// <param name="collisionPosition"></param>
        /// <param name="textureName"></param>
        /// <returns></returns>
        private bool FindTerrainTextureAtPosition(Vector3 collisionPosition, out string textureName)
        {
            textureName = "";

            Vector3 terrainSize = Terrain.activeTerrain.terrainData.size;
            Vector2 textureSize = new Vector2(Terrain.activeTerrain.terrainData.alphamapWidth,
                Terrain.activeTerrain.terrainData.alphamapHeight);

            int alphaX = (int)((collisionPosition.x / terrainSize.x) * textureSize.x + 0.5f);
            int alphaY = (int)((collisionPosition.z / terrainSize.z) * textureSize.y + 0.5f);

            float[,,] terrainMaps = Terrain.activeTerrain.terrainData.GetAlphamaps(alphaX, alphaY, 1, 1);

            float[] textures = new float[terrainMaps.GetUpperBound(2) + 1];

            for (int n = 0; n < textures.Length; n++)
            {
                textures[n] = terrainMaps[0, 0, n];
            }

            if (textures.Length == 0)
            {
                return false;
            }

            // Looking for the texture with the highest 'mix'
            float textureMaxMix = 0;
            int textureMaxIndex = 0;

            for (int currTexture = 0; currTexture < textures.Length; currTexture++)
            {
                if (textures[currTexture] > textureMaxMix)
                {
                    textureMaxIndex = currTexture;
                    textureMaxMix = textures[currTexture];
                }
            }

            // Texture is at index textureMaxIndex
            textureName = (_terrainData != null && _terrainData.terrainLayers.Length > 0) ? (_terrainData.terrainLayers[textureMaxIndex]).diffuseTexture.name : "";

            if (debugTextureName)
            {
                Debug.Log($"FootstepManager: Terrain texture is : {textureName}");
            }
            return true;
        }
        #endregion
        #region Editor methods
        #if UNITY_EDITOR
        public void SetFootstepTriggers(FootstepTrigger[] newFootstepTriggers)
        {
            footstepTriggers = newFootstepTriggers;
        }
        #endif
        public LayerMask TriggerLayerMask => triggerLayerMask;
        #endregion

        #region Editor
        #if UNITY_EDITOR
        public void ConfigureFootstepTriggers()
        {
            Animator animator = GetComponent<Animator>();

            if (!animator || !animator.isHuman)
            {
                return;
            }

            Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            FootstepTrigger[] newFootstepTriggers = new[] { ConfigureFoot(leftFoot.gameObject), ConfigureFoot(rightFoot.gameObject) };
            footstepTriggers = newFootstepTriggers;
        }

        private FootstepTrigger ConfigureFoot(GameObject footGameObject)
        {
            FootstepTrigger existingFootTrigger = footGameObject.GetComponentInChildren<FootstepTrigger>();
            if (existingFootTrigger)
            {
                DestroyImmediate(existingFootTrigger.gameObject);
            }

            // Check if there is a child, and use that instead
            Transform[] childTransforms = footGameObject.GetComponentsInChildren<Transform>();
            if (childTransforms.Length > 1)
            {
                footGameObject = childTransforms[^1].gameObject;
            }

            GameObject footstepTriggerGameObject = new GameObject($"Footstep Trigger {footGameObject.name}")
            {
                transform =
                {
                    position = footGameObject.transform.position,
                },
                layer = footGameObject.transform.root.gameObject.layer
            };

            SphereCollider footCollider = footstepTriggerGameObject.EnsureComponent<SphereCollider>();
            footCollider.isTrigger = true;
            footCollider.radius = 0.1f;

            AudioSource audioSource = footstepTriggerGameObject.EnsureComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialize = true;
            audioSource.spatialBlend = 1.0f;

            Rigidbody footRb = footstepTriggerGameObject.EnsureComponent<Rigidbody>();
            footRb.useGravity = false;
            footRb.isKinematic = false;

            FootstepTrigger footstepTrigger = footstepTriggerGameObject.EnsureComponent<FootstepTrigger>();
            footstepTrigger.SetLayers(triggerLayerMask);

            footstepTriggerGameObject.transform.SetParent(footGameObject.transform, true);

            return footstepTrigger;
        }
        #endif
        #endregion
    }
}