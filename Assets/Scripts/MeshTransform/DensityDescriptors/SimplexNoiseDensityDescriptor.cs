using System.Collections;
using System.Collections.Generic;
using NoiseTest;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/SimplexNoiseDensity")]
public class SimplexNoiseDensityDescriptor : BaseDensityDescriptor
{
    
  
    public Vector3Int size;
    public int octaves;
    public double persistence;
    public double amplitude;
    public float lacunarity;
    public Vector3 frequency;

    OpenSimplexNoise openSimplexNoise;

    protected override void Init(Vector3Int position)
    {
        openSimplexNoise = new OpenSimplexNoise(Random.Range(0,int.MaxValue));
        localDensity = new float[size.x,size.y,size.z];
        for(int x=0;x<size.x;x++)
        {
            for(int y=0;y<size.y;y++)
            {
                for(int z=0;z<size.z;z++)
                {
                    double total = 0;
                    double maxValue = 0;
                    double lAmplitude = amplitude;
                    Vector3 lFrequency = frequency;

                    for(int i=1;i<=octaves;i++){
                       total+= openSimplexNoise.Evaluate(x*lFrequency.x,z*lFrequency.z)*lAmplitude;
                       maxValue += lAmplitude;
                       lAmplitude *= persistence;
                       lFrequency *=lacunarity;
                    }
                    
                    localDensity[x,y,z] = (float)(total/maxValue);
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
                    density[x,y,z] = localDensity[lx,ly,lz];
                }
            }
        }
    }


}
