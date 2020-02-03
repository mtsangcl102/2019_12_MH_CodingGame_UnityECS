using UnityEngine;
namespace CodingGame
{
    [System.Serializable]
    public class GameConfig : MonoBehaviour
    {
        [Header("Terrain")]
        
        public int TerrainSize;
        public Mesh BlockMesh;
        public Material[] BlockMaterials;

        public float RockRatio = 0f;
        public float TreeRatio = 0f;
        public float HighTreeRatio = 0f;
        
        [Header("Player")]
        
        public int PlayerCount;
        public Mesh PlayerMesh;
        public Material[] PlayerMaterials;

        public float PlayerSpeedMin;
        public float PlayerSpeedMax;

        [Header("Explosion")] 
        public bool CanExplosion;
        public Mesh ParticleMesh;
        public int ParticleCount;
        public float ParticleVelocityMax;
        public float ParticleVelocityMin;
        public float ParticleSizeMin;
        public float ParticleSizeMax;
    }
}