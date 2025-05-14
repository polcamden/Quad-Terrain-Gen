using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

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
        private GameObject terrainObject;

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

        public void LodUpdate()
        {

        }

        public void MeshUpdate()
        {
            if (isQuad)
            {
                for (int i = 0; i < subChunks.Length; i++)
                {
                    subChunks[i].MeshUpdate();
                }
            }
            else
            {
                if (updateMesh)
                {
                    if (terrainObject == null)
                    {
                        terrainObject = new GameObject("Terrain-" + chunkPos, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                        terrainObject.transform.position = new Vector3(chunkPos.x * (worldSize / chunkSize), 0, chunkPos.y * (worldSize / chunkSize));
                        terrainObject.GetComponent<MeshRenderer>().material = mainChunk.Material;
                    }

                    Mesh mesh = terrainObject.GetComponent<MeshFilter>().sharedMesh;

                    if(mesh == null)
                    {
                        mesh = new Mesh();
                        terrainObject.GetComponent<MeshFilter>().sharedMesh = mesh;
                    }

                    GenerateMesh(mesh);

                    updateMesh = false;
                }
            }
        }

        private Mesh GenerateMesh(Mesh mesh)
        {
            mesh.Clear();
            Vector3[] vertices = new Vector3[meshResolution * meshResolution];
            int[] triangles = new int[vertices.Length * 6];

            float cellSize = worldSize / meshResolution;

            //place verts
            int vertIndex = 0;
            int trigIndex = 0;
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

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            return mesh;
        }

        public void Split()
        {
            if (isQuad == true)
            {
                Debug.LogError("Trying to split chunk while already split");
            }
            else
            {
                if (terrainObject != null)
                {
                    ScriptableObject.Destroy(terrainObject);
                }

                subChunks = new Chunk[4];
                int subChunkWidth = chunkSize / 2;
                for (int i = 0; i < subChunks.Length; i++)
                {
                    subChunks[i] = new Chunk(mainChunk, chunkPos + Common.subChunkPositions[i] * subChunkWidth, subChunkWidth, meshResolution, worldSize / 2f);
                }

                //gives new chunks the adjacents of the subchunks
                //TODO make new Chunks with given adjacents and tell adjacents this has changed
                for (int i = 0; i < subChunks.Length; i++)
                {
                    int relLeft = (i + 3) % 4;
                    int relRight = (i + 1) % 4;
                    int relDown = (i + 2) % 4;
                    int relUp = i;

                    //subchunk Neighbors
                    subChunks[i].ReplaceNeighbor(null, subChunks[relLeft], (AdjacentDirections)relLeft);
                    subChunks[i].ReplaceNeighbor(null, subChunks[relRight], (AdjacentDirections)relDown);
                    //transfering this chunks neighbors
                    subChunks[i].ReplaceNeighbor(null, neighbors[relRight][0], (AdjacentDirections)relRight);
                    subChunks[i].ReplaceNeighbor(null, neighbors[relUp][0], (AdjacentDirections)relUp);
                }

                neighbors = null;
                isQuad = true;
            }
        }

        public void Merge()
        {
            if (isQuad == false)
            {
                Debug.LogError("Trying to merge chunk while already merged");
            }
            else
            {
                InharitSubChunksNeighbors();

                subChunks = null;
                updateMesh = true;
                isQuad = false;
            }
        }

        private List<Chunk>[] Destroy(Chunk parent)
        {
            if (isQuad)
            {
                Debug.LogError("can not recursivly merge");
                //InharitSubChunksNeighbors();
            }
            else
            {
                if (terrainObject != null)
                {
                    ScriptableObject.Destroy(terrainObject);

                    //tell neighbors that our chunk has been destroy and that the parent of this is the new neighbor
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < neighbors[i].Count; j++)
                        {
                            //neighbors[i][j].NeighborChange(this, parent, (AdjacentDirections)((i + 2) % 4));
                        }
                    }
                }
            }

            return neighbors;
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
        }



        private void InharitSubChunksNeighbors()
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
                }
            }

            //tell neightbors that this chunk is the new neighbor to it



            subChunks = null;
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
        }
    }
}