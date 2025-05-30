using Unity.VisualScripting;
using UnityEngine;

namespace QuadTerrainGen
{
    public class MainChunk : Chunk
    {
        TerrainMap map;
        float[,] heightMap;

        public Material Material
        {
            get { return map.Material; }
        }

        public MainChunk(TerrainMap map, Vector2Int chunkPos, int chunkSize, int meshResolution, int worldSize) : base(null, chunkPos, chunkSize, meshResolution, worldSize)
        {
            this.map = map;
            mainChunk = this;
            //heightMap = ;
        }
    }
}