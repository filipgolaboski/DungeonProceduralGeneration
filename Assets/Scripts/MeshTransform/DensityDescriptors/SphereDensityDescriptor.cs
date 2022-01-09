using System.Collections;
using System.Collections.Generic;
using NoiseTest;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/SphereDensity")]
public class SphereDensityDescriptor : BaseDensityDescriptor
{
    public int radius = 3;
    public Vector3 offset;

    protected override void Init(Vector3Int position)
    {
        localDensity = new float[radius,radius,radius];
        for(int x=0;x<radius;x++)
        {
            for(int y=0;y<radius;y++)
            {
                for(int z=0;z<radius;z++)
                {
                   // localDensity[x,y,z] = Mathf.Abs(SDF_Sphere(new Vector3(x,y,z)+offset-(Vector3.one*radius)/2,radius)/radius);
                   localDensity[x,y,z] = 1 - Mathf.Abs(radius - Vector3.Distance(new Vector3(x,y,z), new Vector3(radius/2,radius/2,radius/2)))/10;
                }
            }
        }

    }


    public override void InsertDensity(Vector3Int position, ref float[,,] density)
    {
        Init(position);
        OpenSimplexNoise ops = new OpenSimplexNoise(12312431413);
        for(int x=position.x, lx=x-position.x; x<density.GetLength(0) && lx<radius;x++,lx++)
        {
            for(int y=position.y, ly=y-position.y; x<density.GetLength(1) && ly<radius;y++,ly++)
            {
                for(int z=position.z, lz=z-position.z; z<density.GetLength(2) && lz<radius;z++,lz++)
                {
                   float noise = (float)(ops.Evaluate(x*5.0,y*5.0,z*5.0)/3.0);
                   density[x,y,z] = Mathf.Min(density[x,y,z], localDensity[lx,ly,lz] + noise) ;
                }
            }
        }
    }

    float SDF_Sphere(Vector3 position, float radius){
        return position.magnitude-radius;
    }

}
