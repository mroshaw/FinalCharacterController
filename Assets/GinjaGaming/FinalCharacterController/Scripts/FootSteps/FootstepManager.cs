using UnityEngine;

namespace GinjaGaming.FinalCharacterController.FootSteps
{
    public class FootstepManager : MonoBehaviour
    {
        #region Class Variables
        [Header("Settings")]
        [SerializeField] private FootstepTrigger[] footstepTriggers;
        [SerializeField] private FootStepAudio[] footStepAudios;

        [Header("Spawn Settings")] public bool alignToTerrainSlope;

        [Header("Pool Settings")]
        [SerializeField] private PrefabPool particleFxPool;
        [SerializeField] private PrefabPool decalPool;

        private TerrainData _terrainData;
        #endregion

        #region Startup
        private void Awake()
        {
            // Register the triggers
            foreach (FootstepTrigger trigger in footstepTriggers)
            {
                trigger.FootstepManager = this;
            }

            _terrainData = Terrain.activeTerrain.terrainData;
        }
    
        private void Start()
        {

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

        public FootStepAudio GetFootStepForPosition(Vector3 position)
        {
            string textureName = GetTerrainTextureAtPosition(position);
            if (string.IsNullOrEmpty(textureName))
            {
                return null;
            }

            FootStepAudio footStepAudio = GetAudioForTexture(textureName);
            if (footStepAudio == null)
            {
                return null;
            }

            return footStepAudio;
        }

        private FootStepAudio GetAudioForTexture(string textureName)
        {
            foreach (FootStepAudio footstepAudio in footStepAudios)
            {
                if (footstepAudio.ContainsTextureName(textureName) && footstepAudio.audioClips.Length > 0)
                {
                    return footstepAudio;
                }
            }
            return null;
        }

        private string GetTerrainTextureAtPosition(Vector3 position)
        {
            Vector3 terrainSize = Terrain.activeTerrain.terrainData.size;
            Vector2 textureSize = new Vector2(Terrain.activeTerrain.terrainData.alphamapWidth,
                Terrain.activeTerrain.terrainData.alphamapHeight);

            // Lookup texture we are standing on:
            int alphaX = (int)((position.x/terrainSize.x)*textureSize.x+0.5f);
            int alphaY = (int)((position.z/terrainSize.z)*textureSize.y+0.5f);

            float[,,] terrainMaps = Terrain.activeTerrain.terrainData.GetAlphamaps(alphaX, alphaY,1 ,1);

            // extract the 3D array data to a 1D array:
            float[] textures = new float[terrainMaps.GetUpperBound(2) + 1];

            for (int n = 0; n < textures.Length; n++)
            {
                textures[n] = terrainMaps[0, 0, n];
            }

            if (textures.Length == 0)
            {
                return "";
            }

            float maxMix = 0;
            int maxIndex = 0;

            // loop through each mix value and find the maximum
            for (int n = 0; n < textures.Length; n++)
            {
                if (textures[n] > maxMix)
                {
                    maxIndex = n;
                    maxMix = textures[n];
                }
            }

            // Texture is at index maxIndex
            string textureName = (_terrainData != null && _terrainData.terrainLayers.Length > 0) ? (_terrainData.terrainLayers[maxIndex]).diffuseTexture.name : "";
            return textureName;
        }
        #endregion
    }
}