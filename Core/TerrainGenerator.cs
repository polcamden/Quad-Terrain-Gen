using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace SimpleTerrainGenerator {
    public class TerrainGenerator : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField] private TerrainMap terrainMap;
        [Header("Chunk Settings")]
        [SerializeField]
        private int mainChunkWorldSize = 4096;
        [SerializeField]
        [Tooltip("How many verticies a chunk will have across")]
        private int chunkResolution = 64;
        [Header("Level of Detail")]
        [SerializeField]
        [Tooltip("How many chunks are made from the camera to the ")]
        private int innerWorldSize = 3;
        [SerializeField]
        [Tooltip("How far away from a chunk the camera needs to be " +
            "for a chunk to be divided. Greatest to Least")]
        private int[] lodLevelsDistance = { 2048, 1024, 512 };
        [SerializeField]
        [Tooltip("what transform does the level of detail follow")]
        private Transform lodCenter;
        //-privates----------------------------------------------------
        private List<MainChunk> worldChunks;
        
        //-accessors---------------------------------------------------

        public int mainChunkSize
        {
            get { return (int)Mathf.Pow(2, lodLevelsDistance.Length); }
        }

        private void Awake()
        {
            worldChunks = new List<MainChunk>();

            if (!GetCamera())
            {
                Debug.LogError("Main camera not found");
                return; //camera is needed for lods
            }

            if(lodCenter != null)
            {
                CreateMainChunks();
                MeshUpdate();
            }
        }

        private void Update()
        {
            if(lodCenter == null && !GetCamera())
            {
                Debug.LogError("Main camera not found");
                return; //camera is needed for lods
            }
            else
            {
                //CreateMainChunks();
                LodUpdate();
                MeshUpdate();
            }
        }

        private void CreateMainChunks()
        {
            for (int x = innerWorldSize; x >= -innerWorldSize; x--)
            {
                for (int z = -innerWorldSize; z <= innerWorldSize; z++)
                {
                    Vector2Int position = new Vector2Int(x * mainChunkSize, z * mainChunkSize);
                    MainChunk chunk = new MainChunk(terrainMap, position, mainChunkSize, chunkResolution, mainChunkWorldSize);

                    worldChunks.Add(chunk);
                }
            }

            for (int i = 0; i < worldChunks.Count; i++)
            {
                Vector2Int position = worldChunks[i].ChunkPosition;
                for (int x = 0; x < 4; x++)
                {
                    Vector2Int adjacentPos = position + Common.neighborPositions[x] * mainChunkSize;

                    MainChunk adjacent = null;
                    foreach (MainChunk chunk in worldChunks)
                    {
                        if(chunk.ChunkPosition == adjacentPos)
                        {
                            adjacent = chunk;
                            break;
                        }
                    }

                    if(adjacent != null)
                        worldChunks[i].ReplaceNeighbor(null, adjacent, (AdjacentDirections)x);
                }
            }

            //Vector2Int position = new Vector2Int(0, 0);
            //MainChunk chunk = new MainChunk(terrainMap, position, mainChunkSize, chunkResolution, mainChunkWorldSize);

            //worldChunks.Add(chunk);
        }

        [ContextMenu("split test")]
        void SplitMainChunk()
        {
            worldChunks[4].Split();
        }

        [ContextMenu("merge test")]
        void MergeMainChunk()
        {
            worldChunks[4].Merge();
        }

		[ContextMenu("transition test")]
		void TransitionUpdateChunk()
		{
			worldChunks[4].transitionUpdate();
		}

		private void LodUpdate()
        {

        }

        private void MeshUpdate()
        {
            foreach(MainChunk chunk in worldChunks)
            {
                chunk.MeshUpdate();
            }
        }

        /// <summary>
        /// Trys to get main camera
        /// </summary>
        /// <returns>Is camera found</returns>
        private bool GetCamera()
        {
            if (Camera.main != null)
            {
                lodCenter = Camera.main.transform;
                return true;
            }
            return false;
        }
    
        private void GenerateNewChunk()
        {

        }

        private void OnDrawGizmos()
        {
            if (worldChunks == null || worldChunks.Count == 0)
                return;

            worldChunks[4].Debugger();

            /*for (int i = 0; i < worldChunks.Count; i++)
            {
                Handles.Label(worldChunks[i].WorldCenter, $"{i}");
            }*/

			/*for (int i = 0; i < worldChunks.Count; i++)
            {
                worldChunks[i].Debugger();
            }*/
		}
    }
}