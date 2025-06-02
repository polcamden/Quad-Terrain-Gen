using UnityEngine;

public abstract class MapComponent : ScriptableObject
{
    public abstract void loadOntoData(ref float[,] heightMap, Vector2Int mainPosition);
}
