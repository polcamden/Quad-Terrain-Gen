using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

namespace SimpleTerrainGenerator
{
	public class Chunk
    {
        public MainChunk mainChunk;

        private Vector2Int chunkPos;
        private int chunkSize;
        private float worldSize;

        private int meshResolution;

        //Quad
        private bool isQuad;
        private Chunk[] subChunks;
        //Quad mesh data
        private GameObject terrainObject;
        private Mesh terrainMesh;
        private int TransitionBackwardStart;
		private int TransitionRightStart;

        //neighbors
        private List<Chunk>[] neighbors;

        private bool updateMesh;

        public Vector2Int ChunkPosition
        {
            get{ return chunkPos; }
        }

        public Vector3 WorldCenter
        {
            get {
                return new Vector3(
                    chunkPos.x * (worldSize / chunkSize) + worldSize / 2,
                    0,
                    chunkPos.y * (worldSize / chunkSize) + worldSize / 2);
            }
        }

        public Vector3 WorldCorner
        {
            get
            {
				return new Vector3(
					chunkPos.x * (worldSize / chunkSize),
					0,
					chunkPos.y * (worldSize / chunkSize));
			}
        }

        public float CellSize
        {
            get
            {
                return worldSize / meshResolution;
			}
        }

        public Chunk(MainChunk mainChunk, Vector2Int chunkPos, int chunkSize, int meshResolution, float worldSize)
        {
            this.mainChunk = mainChunk;
            this.chunkPos = chunkPos;
            this.chunkSize = chunkSize;
            this.meshResolution = meshResolution;
            this.worldSize = worldSize;

            neighbors = new List<Chunk>[4];
            for (int i = 0; i < neighbors.Length; i++)
            {
                neighbors[i] = new List<Chunk>();
            }

            updateMesh = true;
        }

        /// <summary>
        /// Called by TerrainGenerator to check for merge/split 
        /// </summary>
        public void LodUpdate()
        {

        }

        /// <summary>
        /// Called by TerrainGenerator. Recursivly goes thew chunks to check for update
        /// </summary>
        public void MeshUpdate()
        {
            if (isQuad)
            {
                for (int i = 0; i < subChunks.Length; i++)
                {
                    subChunks[i].MeshUpdate();
                }
            }
            else if (updateMesh)
            {
                if (terrainObject == null) //Create mesh
                {
                    terrainObject = new GameObject("Terrain-" + chunkPos, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                    terrainObject.transform.position = new Vector3(chunkPos.x * (worldSize / chunkSize), 0, chunkPos.y * (worldSize / chunkSize));
                    terrainObject.GetComponent<MeshRenderer>().material = mainChunk.Material;
                    terrainMesh = terrainObject.GetComponent<MeshFilter>().sharedMesh;

					terrainMesh = new Mesh();
				    terrainObject.GetComponent<MeshFilter>().sharedMesh = terrainMesh;
				}

                GenerateMesh();

                updateMesh = false;
            }
        }

        public void transitionUpdate()
        {
			if (neighbors[(int)AdjacentDirections.backward].Count != 0)
			{
				AddMeshTransition(false);
			}

			if (neighbors[(int)AdjacentDirections.right].Count != 0)
			{
				AddMeshTransition(true);
			}
			//AddMeshTransition(true);
		}

		/// <summary>
		/// Clears the current mesh (Todo: optimize) and remakes it
		/// </summary>
		/// <param name="mesh">The current mesh of terrainObject</param>
		private void GenerateMesh()
        {
			terrainMesh.Clear();
			// AdjacentDirections.right  AdjacentDirections.backward
			/*int mergeBackwardVerts = 0;
			//loop threw AdjacentDirection.backwards to find neighbor resolution added up
			foreach (Chunk neighbor in neighbors[(int)AdjacentDirections.backward])
			{
				mergeBackwardVerts += neighbor.meshResolution;
				mergeBackwardVerts++;
			}

			int mergeRightVerts = 0;
            //loop threw AdjacentDirection.right to find neighbor resolution added up
            foreach (Chunk neighbor in neighbors[(int)AdjacentDirections.right])
            {
                mergeRightVerts += neighbor.meshResolution;
			}*/

            //Debug.Log($"right: {mergeRightVerts}");
            //Debug.Log($"backward: {mergeBackwardVerts}");

			Vector3[] vertices = new Vector3[meshResolution * meshResolution];
            int[] triangles = new int[6 * (meshResolution - 1) * (meshResolution - 1)];

            float cellSize = CellSize;

            int vertIndex = 0;
            int trigIndex = 0;

            //generate interior mesh
            for (int x = 0; x < meshResolution; x++)
            {
                for (int y = 0; y < meshResolution; y++)
                {
                    vertices[vertIndex] = new Vector3(x * cellSize, 0, y * cellSize);

                    if (x > 0 && y > 0)
                    {
                        int left = vertIndex - 1;
                        int up = vertIndex - meshResolution;
                        int leftUp = vertIndex - meshResolution - 1;

                        //changes triangle directions for |X| faces
                        if (y % 2 == x % 2) //forward triangles
                        {
                            triangles[trigIndex] = vertIndex;
                            triangles[trigIndex + 1] = leftUp;
                            triangles[trigIndex + 2] = up;

                            triangles[trigIndex + 3] = left;
                            triangles[trigIndex + 4] = leftUp;
                            triangles[trigIndex + 5] = vertIndex;
                        }
                        else //backward triangles
                        {
                            triangles[trigIndex] = vertIndex;
                            triangles[trigIndex + 1] = left;
                            triangles[trigIndex + 2] = up;

                            triangles[trigIndex + 3] = left;
                            triangles[trigIndex + 4] = leftUp;
                            triangles[trigIndex + 5] = up;
                        }

                        trigIndex += 6;
                    }

                    vertIndex++;
                }
            }

            /*int mergStart = vertIndex;
            //generate backward merge
			foreach (Chunk neighbor in neighbors[(int)AdjacentDirections.backward])
			{
				for (int i = neighbor.meshResolution - 1; i < neighbor.meshResolution * neighbor.meshResolution; i += neighbor.meshResolution)
				{
					Vector3 vert = neighbor.terrainMesh.vertices[i] + Vector3.back * worldSize;
                    vertices[vertIndex] = vert;
                    vertIndex++;
				}
			}

            if (mergStart != vertIndex)
            {
                for (int i = 0; i < meshResolution-1; i++)
                {
                    int vert = i * meshResolution;
                    int left = vert + meshResolution;
                    int up = mergStart + i;
                    int down = up + 1;

                    triangles[trigIndex] = vert;
                    triangles[trigIndex + 1] = left;
                    triangles[trigIndex + 2] = up;

                    triangles[trigIndex + 3] = left;
                    triangles[trigIndex + 4] = down;
                    triangles[trigIndex + 5] = up;
                    trigIndex += 6;
				}
            }

			mergStart = vertIndex;
			//generate backward merge
			foreach (Chunk neighbor in neighbors[(int)AdjacentDirections.right])
			{
				for (int i = 0; i < neighbor.meshResolution; i++)
				{
					vertices[vertIndex] = neighbor.terrainMesh.vertices[i] + Vector3.right * worldSize;
					vertIndex++;
				}
			}

			if (mergStart != vertIndex)
			{
				for (int i = 0; i < meshResolution - 1; i++)
				{
					int vert = i + (meshResolution - 1) * meshResolution;
					int left = vert+1;
					int up = mergStart + i;
					int down = up + 1;

					triangles[trigIndex] = vert;
					triangles[trigIndex + 1] = left;
					triangles[trigIndex + 2] = up;

					triangles[trigIndex + 3] = left;
					triangles[trigIndex + 4] = down;
					triangles[trigIndex + 5] = up;
					trigIndex += 6;
				}
			}*/

			

			terrainMesh.vertices = vertices;
			terrainMesh.triangles = triangles;

			Debug.Log($"vert missMatch: {vertices.Length - vertIndex}");
			Debug.Log($"Trig missMatch: {triangles.Length - trigIndex}");
			//return mesh;
		}

		/// <summary>
		/// AddsMergeToMesh or Remakes Merge
		/// </summary>
		/// <param name="isRight">isRight ? AdjacentDirections.right : AdjacentDirections.backwards</param>
		private void AddMeshTransition(bool isRight)
        {
            AdjacentDirections dir = isRight ? AdjacentDirections.right : AdjacentDirections.forward;

            int vertsCount = 0;
            foreach(Chunk neighbor in neighbors[(int)dir])
            {
				vertsCount += neighbor.meshResolution;
				vertsCount++;
			}
            if (!isRight) //when right add extra vert for corner
            {
				vertsCount--;
			}

            Vector3[] vertices = new Vector3[vertsCount];
            int[] triangles = new int[64 * 24]; //Todo: find equation for this
            int vertOffset = terrainMesh.vertices.Length;

            int vertIndex = 0;
            int trigIndex = 0;
            for (int i = 0; i < neighbors[(int)dir].Count; i++)
            {
				Chunk neighbor = neighbors[(int)dir][i];

				int neighborStart = vertOffset + vertIndex;
				int neighborEnd = neighborStart + neighbor.meshResolution;

                float neighborsRatio = 1f / neighbors[(int)dir].Count;
				int ourStart = (int)(neighborsRatio * meshResolution) * i;
				int ourEnd = (int)(neighborsRatio * meshResolution) * (i + 1);

                Debug.Log($"{neighborStart} -> {neighborEnd}");
                Debug.Log($"{ourStart} -> {ourEnd}");

                //create vertices for neighbor
				for (int j = 0; j < neighbor.meshResolution; j++)
				{
					if (isRight)
					{
						float forwardOffset = i == 0 ? 0 : (worldSize / (i + 1));
						vertices[vertIndex] = neighbor.terrainMesh.vertices[j] + Vector3.right * worldSize + Vector3.forward * forwardOffset;
					}
					else
					{
                        float rightOffset = i == 0 ? 0 : (worldSize / (i + 1));
                        Debug.Log(rightOffset);
						vertices[vertIndex] = neighbor.terrainMesh.vertices[j * meshResolution] + Vector3.forward * worldSize + Vector3.right * rightOffset;
					}

					vertIndex++;
				}

                //triangle between us and neighbor vertices
				if (neighbor.CellSize == CellSize) //equal res
				{
					trigIndex = TransitionEquivalent(ref triangles, trigIndex, neighborStart, neighborEnd, ourStart, ourEnd, isRight);
				}
				else if (neighbor.CellSize < CellSize) //low res to high res
				{
                    Debug.Log("transiton");
                    trigIndex = TransitionLowHigh(ref triangles, trigIndex, neighborStart, neighborEnd, ourStart, ourEnd, isRight);
				}
				else if (neighbor.CellSize > CellSize) //high res to low res
				{

				}
			}


            //Todo: remove old transition and if backward grab left merge
            Vector3[] finalVertices = new Vector3[terrainMesh.vertexCount + vertices.Length];
            int[] finalTriangles = new int[terrainMesh.triangles.Length + triangles.Length];

            Array.Copy(terrainMesh.vertices, 0, finalVertices, 0, terrainMesh.vertices.Length);
            Array.Copy(vertices, 0, finalVertices, terrainMesh.vertices.Length, vertices.Length);

			Array.Copy(terrainMesh.triangles, 0, finalTriangles, 0, terrainMesh.triangles.Length);
			Array.Copy(triangles, 0, finalTriangles, terrainMesh.triangles.Length, triangles.Length);

            Debug.Log($"final: {finalVertices.Length}, original: {terrainMesh.vertices.Length}");
            Debug.Log($"final: {finalTriangles.Length}, original: {terrainMesh.triangles.Length}");

            terrainMesh.vertices = finalVertices;
            terrainMesh.triangles = finalTriangles;
		}

        /// <summary>
        /// Makes triangles between the start and end vertex indices, for when us and neighbor are equal sizes and resolution
        /// </summary>
        /// <param name="triangles">Ref to triangles</param>
        /// <param name="trigIndex">Trianlge index</param>
        /// <param name="neighborStart">Index start of the neighbor vertices</param>
        /// <param name="neighborEnd">Index end of the neighbors vertices</param>
        /// <param name="ourStart">Index start of our vertices</param>
        /// <param name="ourEnd">Index end of our vertices</param>
        /// <param name="isRight">Right or forward</param>
        /// <returns>The new trigIndex</returns>
        private int TransitionEquivalent(ref int[] triangles, int trigIndex, int neighborStart, int neighborEnd, int ourStart, int ourEnd, bool isRight)
        {
            int count = ourEnd - ourStart;

            if(count != neighborEnd - neighborStart)
            {
                Debug.LogError("us and neighbor are not equivalent, but equivalent transition was called");
                return 100000;
            }

            for (int i = 1; i < count; i++)
            {
                int neighbor0 = neighborStart + i;
                int neighbor1 = neighbor0 - 1;
				if (isRight)
				{
					int our0 = i + (meshResolution - 1) * meshResolution;
					int our1 = our0 - 1;

					triangles[trigIndex] = our1;
					triangles[trigIndex + 1] = neighbor0;
					triangles[trigIndex + 2] = neighbor1;

					triangles[trigIndex + 3] = our1;
					triangles[trigIndex + 4] = our0;
					triangles[trigIndex + 5] = neighbor0;
                    trigIndex += 6;
				}
				else
				{
					int our0 = i * meshResolution + meshResolution - 1;
					int our1 = our0 - meshResolution;

					triangles[trigIndex] = our0;
					triangles[trigIndex + 1] = neighbor1;
					triangles[trigIndex + 2] = neighbor0;

					triangles[trigIndex + 3] = our0;
					triangles[trigIndex + 4] = our1;
					triangles[trigIndex + 5] = neighbor1;
                    trigIndex += 6;
				}
			}

            return trigIndex;
		}

		/// <summary>
		///  Makes triangles between the start and end vertex indices, for when neighbor has a higher resolution that us
		/// </summary>
		/// <param name="triangles">Ref to triangles</param>
		/// <param name="trigIndex">Trianlge index</param>
		/// <param name="neighborStart">Index start of the neighbor vertices</param>
		/// <param name="neighborEnd">Index end of the neighbors vertices</param>
		/// <param name="ourStart">Index start of our vertices</param>
		/// <param name="ourEnd">Index end of our vertices</param>
		/// <param name="isRight">Right or forward</param>
		/// <returns>The new trigIndex</returns>
		private int TransitionLowHigh(ref int[] triangles, int trigIndex, int neighborStart, int neighborEnd, int ourStart, int ourEnd, bool isRight)
        {
			int count = ourEnd - ourStart;
            int vertsPerCount = (neighborEnd - neighborStart) / count;

            //Debug.Log(neighborStart);

            int i = 0;
            int our0 = isRight ? ourStart + (meshResolution - 1) * meshResolution : ourStart * meshResolution + meshResolution - 1;
            for (int neighborVert = neighborStart; neighborVert < neighborEnd; neighborVert++)
            {
                Debug.Log(neighborVert);

                if (i >= vertsPerCount)
                {
                    int our1 = our0 + (isRight ? 1 : meshResolution);


						triangles[trigIndex] = our0;
						triangles[trigIndex + 1] = our1;

                    triangles[trigIndex + 2] = neighborVert - (vertsPerCount / 2);
                    trigIndex += 3;

                    our0 = our1;
					i = 0;
				}

                if (neighborVert >= vertsPerCount / 2 + neighborStart)
                {
                    triangles[trigIndex] = our0;
					if (isRight)
					{
						triangles[trigIndex + 1] = neighborVert;
						triangles[trigIndex + 2] = neighborVert - 1;
					}
					else
					{
						triangles[trigIndex + 1] = neighborVert - 1;
						triangles[trigIndex + 2] = neighborVert;
					}

					trigIndex += 3;
                }

				i++;
            }

            return trigIndex;
		}

        private void TranitionHighLow()
        {

        }

		/// <summary>
		/// Sub-divides chunk
		/// </summary>
		public void Split()
        {
            if (isQuad == true)
            {
                Debug.LogError("Trying to split chunk while already split");
            }
            else
            {
                if (terrainObject != null)
                    ScriptableObject.Destroy(terrainObject);

                subChunks = new Chunk[4];
                int subChunkWidth = chunkSize / 2;
                for (int i = 0; i < subChunks.Length; i++)
                {
                    subChunks[i] = new Chunk(mainChunk, chunkPos + Common.subChunkPositions[i] * subChunkWidth, subChunkWidth, meshResolution, worldSize / 2f);
                }

                RenounceNeighborsToSubChunks();

				neighbors = null;
                isQuad = true;
            }
        }

        /// <summary>
        /// Adds Sub-chunks to this chunk
        /// </summary>
        public void Merge()
        {
            if (isQuad == false)
            {
                Debug.LogError("Trying to merge chunk while already merged");
            }
            else
            {
                InheritNeighborsFromSubChunks();

                subChunks = null;
                updateMesh = true;
                isQuad = false;
            }
        }

        public virtual List<Chunk>[] Destroy(Chunk parent)
        {
            if (isQuad)
            {
                Debug.LogError("Can not recursivly merge");
                //InharitSubChunksNeighbors();
            }
            else
            {
                if (terrainObject != null)
                {
                    ScriptableObject.Destroy(terrainObject);
                    terrainMesh.Clear();

                    //tell neighbors that our chunk has been destroy and that the parent of this is the new neighbor
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < neighbors[i].Count; j++)
                        {
                            neighbors[i][j].ReplaceNeighbor(this, parent, (AdjacentDirections)((i + 2) % 4));
                        }
                    }
                }
            }

            return neighbors;
        }

		/// <summary>
		/// Called when a merge occurs to grab neighbors from sub-chunks and change neighbors to this chunk
		/// </summary>
		private void InheritNeighborsFromSubChunks()
		{
			neighbors = new List<Chunk>[4];
			for (int i = 0; i < neighbors.Length; i++)
			{
				neighbors[i] = new List<Chunk>();
				Debug.Log(neighbors[i].Count);
			}

			for (int i = 0; i < subChunks.Length; i++)
			{
				List<Chunk>[] subNeighbors = subChunks[i].Destroy(this);

				for (int dir = 0; dir < 4; dir++)
				{
					int i2 = (i + 1) % 4;
					neighbors[i].AddRange(subNeighbors[i].Except(neighbors[i]));
					neighbors[i2].AddRange(subNeighbors[i2].Except(neighbors[i2]));

                    neighbors[i].Remove(this);
                    neighbors[i2].Remove(this);
				}
			}

			subChunks = null;
		}

		/// <summary>
		/// Called when a split occurs to give neighbors to SubChunks and change neighbors to the new subchunks
		/// </summary>
		private void RenounceNeighborsToSubChunks()
        {
			for (int i = 0; i < subChunks.Length; i++)
			{
				int relLeft = (i + 3) % 4;
				int relRight = (i + 1) % 4;
				int relDown = (i + 2) % 4;
				int relUp = i;

				//subchunk Neighbors
				subChunks[i].ReplaceNeighbor(null, subChunks[relLeft], (AdjacentDirections)relLeft);
				subChunks[i].ReplaceNeighbor(null, subChunks[relRight], (AdjacentDirections)relDown);
				//Give our neighbors to children Todo: fix for split neighbors
                if(neighbors[relRight].Count != 0)
                {
					subChunks[i].ReplaceNeighbor(null, neighbors[relRight][0], (AdjacentDirections)relRight);
				}
				if (neighbors[relUp].Count != 0)
				{
					subChunks[i].ReplaceNeighbor(null, neighbors[relUp][0], (AdjacentDirections)relUp);
				}
				

                //tell neighbors of new children
                foreach(Chunk rightNeightbor in neighbors[relRight])
                {
                    rightNeightbor.ReplaceNeighbor(this, subChunks[i], (AdjacentDirections)relLeft);
                }
                foreach(Chunk upNeighbor in neighbors[relUp])
                {
					upNeighbor.ReplaceNeighbor(this, subChunks[i], (AdjacentDirections)relDown);
				}
			}
		}

		/// <summary>
		/// Removes a chunk and replaces it with a new one
		/// </summary>
		/// <param name="remove"></param>
		/// <param name="replacement"></param>
		/// <param name="direction">The direction of the chunk that is being removed relative to the chunk were on</param>
		public void ReplaceNeighbor(Chunk remove, Chunk replacement, AdjacentDirections direction)
        {
            if (remove != null && !neighbors[(int)direction].Remove(remove))
            {
                //Debug.LogError("Trying to remove a neighbor " + remove + " but neighbor could not be found");
            }

            if (neighbors[(int)direction].IndexOf(replacement) == -1)
            {
                neighbors[(int)direction].Add(replacement);
            }

            if(direction == AdjacentDirections.right || direction == AdjacentDirections.backward)
            {
                updateMesh = true;
			}
        }

        public void Debugger()
        {
            if (isQuad)
            {
                for (int i = 0; i < subChunks.Length; i++)
                {
                    subChunks[i].Debugger();
                }
            }
            else
            {
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.UpperCenter;
                style.fontSize = 10;
                style.normal.textColor = Color.white;

                float halfSize = worldSize / 2;
                Vector3 worldPos = new Vector3(chunkPos.x * (worldSize / chunkSize) + halfSize, 0, chunkPos.y * (worldSize / chunkSize) + halfSize);
                halfSize /= 2;
                for (int i = 0; i < neighbors.Length; i++)
                {
                    Vector3 labelPos = worldPos + new Vector3(Common.neighborPositions[i].x * halfSize, 0, Common.neighborPositions[i].y * halfSize);
                    
                    Handles.Label(labelPos, $"[{i}] neighbors: {neighbors[i].Count}", style);

                    switch (i)
                    {
                        case 0:
                            Handles.color = new Color(1f, 0f, 0f, 0.8f);
                            break;
                        case 1:
                            Handles.color = new Color(0f, 1f, 0f, 0.8f);
                            break;
                        case 2:
                            Handles.color = new Color(0f, 0f, 1f, 0.8f);
                            break;
                        default:
                            Handles.color = new Color(1f, 0f, 1f, 0.8f);
                            break;
                    }

                    for (int x = 0; x < neighbors[i].Count; x++)
                    {
                        Handles.DrawLine(labelPos, neighbors[i][x].WorldCenter + Vector3.up * 100);
                        Handles.DrawSolidDisc(labelPos, Vector3.up, 64);
                    }
                }
            }

            // neighbor right
            Chunk neighbor = neighbors[(int)AdjacentDirections.right][0];

            /*for (int i = 0; i < neighbor.meshResolution; i++)
            {
				int index = i;
				Vector3 pos = neighbor.terrainMesh.vertices[index] + Vector3.right * worldSize + WorldCorner;
                Handles.Label(pos, $"{index}");
			}

			neighbor = neighbors[(int)AdjacentDirections.forward][0];
			for (int i = 0; i < neighbor.meshResolution; i++)
			{
                int index = i * meshResolution;
				Vector3 pos = neighbor.terrainMesh.vertices[index] + Vector3.forward * worldSize + WorldCorner;
				Handles.Label(pos, $"{index}");
			}*/

			// our right
			/*for (int i = 0; i < meshResolution; i++)
            {
                int index = i + (meshResolution - 1) * meshResolution;
				Vector3 ourVert = terrainMesh.vertices[index] + WorldCorner;
				Handles.Label(ourVert, $"{index}");
			}

            //our forward
			for (int i = 0; i < meshResolution; i++)
			{
                int index = i * meshResolution + meshResolution - 1;
				Vector3 ourVert = terrainMesh.vertices[index] + WorldCorner;
				Handles.Label(ourVert, $"{index}");
			}*/
		}
    }
}