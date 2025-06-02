using UnityEngine;

namespace QuadTerrainGen
{
    public static class Common
    {
        public static readonly Vector2Int[] subChunkPositions = {
            new Vector2Int(1,1),
            new Vector2Int(1,0),
            new Vector2Int(0,0),
            new Vector2Int(0,1),
        };

        public static readonly Vector2Int[] neighborPositions = {
            new Vector2Int(0 , 1),
            new Vector2Int(1 , 0),
            new Vector2Int(0 ,-1),
            new Vector2Int(-1, 0)
        };

        public static readonly Vector2Int[] chunkAdjacentDirections = {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };
    }

    public enum AdjacentDirections
    {
        forward,
        right,
        backward,
        left
    }
}