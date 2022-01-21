using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseDensityDescriptor : ScriptableObject
{
    public Vector3Int size;
    public Vector3Int position;

    public virtual float GetDensity(Vector3Int point) 
    {
        return 0;
    }

    public virtual float GetDensity(Vector3Int point, Vector3Int position)
    {
        return GetDensity(point + position);
    }

}
