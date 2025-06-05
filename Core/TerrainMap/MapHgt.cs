using System.IO;
using UnityEngine;

namespace QuadTerrainGen
{
	[CreateAssetMenu(fileName = "Hgt File", menuName = "QuadTerrainGen/Hgt File", order = 100)]
	public class MapHgt : MapComponent
	{
		[SerializeField] private string hgtPath;
		[SerializeField] private bool isOneArcSec = true;
		[SerializeField] private bool fixGaps = true;
		[Space]
		[SerializeField] private Vector2Int chunkOffset;

		public override void loadOntoData(ref float[,] heightMap, Vector2Int mainPosition)
		{
			int spacing = isOneArcSec ? 30 : 92;
			int gridSize = isOneArcSec ? 3601 : 1201;
			int hgtWidth = heightMap.GetLength(0) / spacing;
			Vector2Int hgtCorner = (mainPosition + chunkOffset) * hgtWidth;

			if (hgtCorner.x < 0 || hgtCorner.y < 0) //Todo: check other direction
			{
				return;
			}

			using (var fs = new FileStream(hgtPath, FileMode.Open, FileAccess.Read))
			using (var br = new BinaryReader(fs))
			{
				for (int row = 0; row <= hgtWidth; row++)
				{
					long offset = ((hgtCorner.y + row) * gridSize + hgtCorner.x) * 2L; // 2 bytes per sample
					fs.Seek(offset, SeekOrigin.Begin);

					for (int col = 0; col <= hgtWidth; col++)
					{
						byte high = br.ReadByte(); // HGT files are big-endian
						byte low = br.ReadByte();
						short value = (short)((high << 8) | low);
						//heightMap[row * spacing, col * spacing] = value;

						int heightX = col * spacing;
						int heightY = row * spacing;

						for (int x = heightX; x < heightX + spacing; x++)
						{
							for (int y = heightY; y < heightY + spacing; y++)
							{
								if (x < heightMap.GetLength(0) && y < heightMap.GetLength(0))
								{
									heightMap[x, y] += value;
								}

								
							}
						}

					}
				}
			}
		}
	}
}