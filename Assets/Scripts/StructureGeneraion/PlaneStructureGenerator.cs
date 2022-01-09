using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneStructureGenerator : StructureGenerator
{
    public SimplexNoiseDensityDescriptor simplexNoiseDensityDescriptor;

    public override void Generate()
    {
        float[,,] densityData = new float[size.x,size.y,size.z];
        densityDescriptor.InsertDensity(Vector3Int.zero,ref densityData);
        
        if(simplexNoiseDensityDescriptor){
           float[,,] noiseDensity = simplexNoiseDensityDescriptor.GetDensity(Vector3Int.zero);

           for(int x=0;x<size.x;x++){
               for(int y=0;y<size.y;y++)
               {
                   for(int z=0;z<size.z;z++)
                   {
                       densityData[x,y,z] = Mathf.Min(densityData[x,y,z],densityData[x,y,z] + noiseDensity[x,y,z]);
                   }
               }
           }
        }

        marchingCubes.InitMeshGeneration(size,densityData);
        List<ChunkData> chunksData = marchingCubes.StartMarchingCubes();
        GenerateMesh(chunksData);
    }
}
