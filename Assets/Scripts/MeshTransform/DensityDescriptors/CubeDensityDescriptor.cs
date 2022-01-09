using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/CubeDensity")]
public class CubeDensityDescriptor : BaseDensityDescriptor
{
    public Vector3Int size;
    public float ISO = 0.5f;

    protected override void Init(Vector3Int position)
    {
        localDensity = new float[size.x,size.y,size.z];

        for(int x=0;x<size.x;x++)
        {
            for(int y=0;y<size.y;y++)
            {
                for(int z=0;z<size.z;z++){
                    localDensity[x,y,z] = Mathf.Abs(SDF_Cube(new Vector3(x,y,z)-size/2, size)/100);
                }
            }
        }
    }

    float SDF_Cube(Vector3 p, Vector3 b){
        Vector3 q = absVector(p)-b;
        return Vector3.Magnitude(Vector3.Max(q, Vector3.zero)) + Mathf.Min(Mathf.Max(q.x,Mathf.Max(q.y,q.z)),0.0f);
    }

    Vector3 absVector(Vector3 v){
        return new Vector3(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z));
    }

    public override void InsertDensity(Vector3Int position, ref float[,,] density)
    {
        Init(position);

        for(int x=position.x, lx=x-position.x; x<density.GetLength(0) && lx<size.x;x++,lx++)
        {
            for(int y=position.y, ly=y-position.y; x<density.GetLength(1) && ly<size.y;y++,ly++)
            {
                for(int z=position.z, lz=z-position.z; z<density.GetLength(2) && lz<size.z;z++,lz++)
                {
                    density[x,y,z] = Mathf.Max(density[x,y,z],localDensity[lx,ly,lz]);
                }
            }
        }  
    }


}
