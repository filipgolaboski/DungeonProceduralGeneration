using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/PlaneDensityDescriptor")]
public class PlaneDensityDescriptor : BaseDensityDescriptor
{
    public Vector3Int size;
    public float height;

    protected override void Init(Vector3Int position)
    {
        localDensity = new float[size.x,size.y,size.z];

        for(int x=0;x<size.x;x++)
        {
            for(int y=0;y<size.y;y++)
            {
                for(int z=0;z<size.z;z++){
                    localDensity[x,y,z] = (float)y/(size.y*height);
                }
            }
        }
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
                    density[x,y,z] += localDensity[lx,ly,lz];
                }
            }
        }  
    }

}
