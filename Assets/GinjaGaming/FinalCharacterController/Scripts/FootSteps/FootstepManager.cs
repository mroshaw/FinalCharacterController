using UnityEngine;

namespace GinjaGaming.FinalCharacterController.FootSteps
{
    public class FootstepManager : MonoBehaviour
    {
        #region Class Variables

        [Header("Settings")]
        [SerializeField] private FootstepTrigger[] footstepTriggers;
        [SerializeField] private FootstepSurface defaultSurface;
        [SerializeField] private FootstepSurface[] footStepAudios;

        [Header("Spawn Settings")] public bool alignToTerrainSlope;

        [Header("Pool Settings")]
        [SerializeField] private PrefabPool particleFxPool;
        [SerializeField] private PrefabPool decalPool;

        [Header("Debug")] [SerializeField] private bool debugTextureName;

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
        public void SpawnFootStepParticleFx(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            GameObject particleFxInstance = particleFxPool.SpawnInstance(spawnPosition, spawnRotation);
        }

        public void SpawnFootStepDecal(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            GameObject decalInstance = decalPool.SpawnInstance(spawnPosition, spawnRotation);
        }

        public void GetSurfaceFromCollision(Transform footTransform, Collider otherCollider,
            out FootstepSurface footstepSurface, out Vector3 spawnPosition)
        {

            if (otherCollider is TerrainCollider)
            {
                string terrainTextureName;

                Vector3 collisionPosition = otherCollider.ClosestPoint(footTransform.position);
                if(!FindTerrainTextureFromCollision(otherCollider, out terrainTextureName))
                {
                    footstepSurface = defaultSurface;
                    spawnPosition = collisionPosition;
                }
                float terrainHeight =  Terrain.activeTerrain.SampleHeight(collisionPosition);
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

        private FootstepSurface FindSurfaceFromTexture(string textureName)
        {
            foreach (FootstepSurface currSurface in footStepAudios)
            {
                if (currSurface.ContainsTextureName(textureName) && currSurface.audioClips.Length > 0)
                {
                    return currSurface;
                    ;
                }
            }
            return defaultSurface;
        }

        private bool FindMaterialTextureFromCollider(Collider other, out string textureName)
        {
            textureName = "";

            MeshRenderer meshRender = other.GetComponent<MeshRenderer>();
            if (!meshRender)
            {
                return false;
            }
            Material meshMaterial = meshRender.material;
            if (!meshMaterial)
            {
                return false;
            }
            textureName = meshMaterial.mainTexture.name;
            if (debugTextureName)
            {
                Debug.Log($"FootstepManager: Mesh texture is : {textureName}");
            }
            return true;
        }

        private bool FindTerrainTextureFromCollision(Collider other, out string textureName)
        {
            textureName = "";

            Vector3 collisionPosition = other.ClosestPoint(transform.position);

            Vector3 terrainSize = Terrain.activeTerrain.terrainData.size;
            Vector2 textureSize = new Vector2(Terrain.activeTerrain.terrainData.alphamapWidth,
                Terrain.activeTerrain.terrainData.alphamapHeight);

            int alphaX = (int)((collisionPosition.x/terrainSize.x)*textureSize.x+0.5f);
            int alphaY = (int)((collisionPosition.z/terrainSize.z)*textureSize.y+0.5f);

            float[,,] terrainMaps = Terrain.activeTerrain.terrainData.GetAlphamaps(alphaX, alphaY,1 ,1);

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
    }
}