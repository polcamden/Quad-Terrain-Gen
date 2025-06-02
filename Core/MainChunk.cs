using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

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

			CacheHeightMap();
        }

        public float GetHeight(int x, int y)
        {
            if(heightMap == null)
            {
                return 0;
            }
            else
            {
                x -= chunkPos.x / chunkSize * worldSize;
                y -= chunkPos.y / chunkSize * worldSize;

				return heightMap[x, y];
			}
               
        }

        private void CacheHeightMap()
        {
            Vector2Int mainPosition = new Vector2Int(chunkPos.x / chunkSize, chunkPos.y / chunkSize);
            
            heightMap = map.GetHeightMap(mainPosition, worldSize);
		}
    }
}