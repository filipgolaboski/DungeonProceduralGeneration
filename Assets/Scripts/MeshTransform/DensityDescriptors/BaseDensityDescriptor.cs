using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseDensityDescriptor : ScriptableObject
{
    protected float [,,] localDensity;

    protected virtual void Init(Vector3Int position)
    {

    }


    public virtual void InsertDensity(Vector3Int position,ref float[,,] density){
        density = null;
    }

    public float[,,] GetDensity(Vector3Int position){
        Init(position);
        return localDensity;
    }

}
