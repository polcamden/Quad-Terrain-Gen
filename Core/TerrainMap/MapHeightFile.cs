using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace QuadTerrainGen
{
	[CreateAssetMenu(fileName = "Height File", menuName = "QuadTerrainGen/Height File", order = 100)]
	public class MapHeightFile : MapComponent
	{
		[Tooltip("folder of .trn files")]
		[SerializeField] private string path;
		[Tooltip("Offsets the height of terrain")]
		[SerializeField] float offset;

		public override void loadOntoData(ref float[,] heightMap, Vector2Int mainPosition)
		{
			string finalPath = $"{path}/{mainPosition}.trn";

			if (File.Exists(finalPath))
			{
				using (BinaryReader reader = new BinaryReader(File.Open(finalPath, FileMode.Open)))
				{
					for (int x = 0; x < heightMap.GetLength(0); x++)
					{
						for (int y = 0; y < heightMap.GetLength(1); y++)
						{
							heightMap[x, y] = reader.ReadSingle();
						}
					}
				}
			}
			else
			{
				Debug.LogError("Trn file not found");
			}
		}
	}
}