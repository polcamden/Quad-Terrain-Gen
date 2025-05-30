using UnityEngine;

public abstract class MapComponent : ScriptableObject
{
    public abstract float[,] loadData(Vector2Int position, Vector2Int size);
}
