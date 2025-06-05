using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace QuadTerrainGen
{
    [CreateAssetMenu(fileName = "TerrainMap", menuName = "QuadTerrainGen/TerrainMap")]
    public class TerrainMap : ScriptableObject
    {
        [SerializeField] private Material material;
        [Space]
        [SerializeField] private List<MapComponent> components;

        public Material Material
        {
            get { return material; }
        }

        public float[,] GetHeightMap(Vector2Int mainPosition, int size)
        {
            float[,] heightMap = new float[size,size];

            for (int i = components.Count - 1; i >= 0; i--)
            {
                components[i].loadOntoData(ref heightMap, mainPosition);
            }
            
            return heightMap;
        }
    }
}