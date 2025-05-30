using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace QuadTerrainGen
{
    [CreateAssetMenu(fileName = "TerrainMap", menuName = "HotAirBalloon/TerrainMap")]
    public class TerrainMap : ScriptableObject
    {
        [SerializeField] private Material material;
        [Space]
        [SerializeField] private List<MapComponent> components;

        public Material Material
        {
            get { return material; }
        }

        public float[,] GetHeightMap(Vector2Int mainPos, Vector2Int size)
        {
            return new float[4096, 4096];
        }
    }
}