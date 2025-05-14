using UnityEngine;

namespace SimpleTerrainGenerator
{
    [CreateAssetMenu(fileName = "TerrainMap", menuName = "HotAirBalloon/TerrainMap")]
    public class TerrainMap : ScriptableObject
    {
        [SerializeField] private Material material;

        public Material Material
        {
            get { return material; }
        }

        public float[,] GetHeightMap(Vector2Int mainPos)
        {
            return new float[4096, 4096];
        }
    }
}