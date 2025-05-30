using UnityEngine;

namespace QuadTerrainGen
{
	[CreateAssetMenu(fileName = "Height File", menuName = "HotAirBalloon/HeightFile", order = 100)]
	public class MapHeightFile : MapComponent
	{
		[Tooltip("folder of .trn files")]
		[SerializeField] private string path;
		[Tooltip("distance in meters between points in .trn files")]
		[SerializeField] int resolution = 30;
		[Tooltip("Offsets the height of terrain")]
		[SerializeField] float offset;

		public override float[,] loadData(Vector2Int position, Vector2Int size)
		{
			throw new System.NotImplementedException();
		}
	}
}