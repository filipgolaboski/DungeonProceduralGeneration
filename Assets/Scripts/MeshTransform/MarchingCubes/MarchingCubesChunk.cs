using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VoxelData{
    public List<Vector3> vertexData = new List<Vector3>();
    public List<int> triangleData = new List<int>();

    public List<Vector3> normalData = new List<Vector3>();
}

public class MarchingCubesChunk
{
    public float isolevel = 0.5f;
    int size;
    float [,,] densities_matrix;

    public void Init(DensityChunkData chunkData){
        this.isolevel = chunkData.ISOLevel;
        this.size = chunkData.size.x;
        this.densities_matrix = chunkData.chunkDensity;
    }

    public Vector3[] GetVoxelCube(Vector3 position)
    {
        Vector3[] cubeVertices = new Vector3[8];
        for(int i=0;i<8;i++){
            cubeVertices[i] = position + MarchingCubesLookupTables.CubeCorners[i];
        }
        return cubeVertices;
    }

    public int CalculateCubeIndex(float[] voxelDensity, float isoLevel)
    {
        int cubeIndex = 0;
        if(voxelDensity[0] < isoLevel) cubeIndex |= 1;
        if(voxelDensity[1] < isoLevel) cubeIndex |= 2;
        if(voxelDensity[2] < isoLevel) cubeIndex |= 4;
        if(voxelDensity[3] < isoLevel) cubeIndex |= 8;
        if(voxelDensity[4] < isoLevel) cubeIndex |= 16;
        if(voxelDensity[5] < isoLevel) cubeIndex |= 32;
        if(voxelDensity[6] < isoLevel) cubeIndex |= 64;
        if(voxelDensity[7] < isoLevel) cubeIndex |= 128;
        return cubeIndex;
    }


    public Vector3 VertexInterpolate(Vector3 p1, Vector3 p2, float v1, float v2, float isoLevel)
    {
        float t = (isoLevel - v1)/(v2-v1);
        Vector3 v = p1 + t*(p2-p1);
        return v;
    }

    public List<Vector3> GenerateVertexList(float[] voxelDensity, Vector3 localPosition, int edgeIndex, float isoLevel){
        List<Vector3> vertexList = new List<Vector3>();

        for(int i=0;i<12;i++){
           // if((edgeIndex & (1<<i))==0){continue;}

            int edgeStart = MarchingCubesLookupTables.EdgeIndexTable[2*i];
            int edgeEnd = MarchingCubesLookupTables.EdgeIndexTable[2*i+1];

            Vector3 corner1 = localPosition + MarchingCubesLookupTables.CubeCorners[edgeStart];
            Vector3 corner2 = localPosition + MarchingCubesLookupTables.CubeCorners[edgeEnd];

            float density1 = voxelDensity[edgeStart];
            float density2 = voxelDensity[edgeEnd];

            vertexList.Add(VertexInterpolate(corner1,corner2,density1,density2, isoLevel));
        }

        return vertexList;
    }


    public float[,,] GenerateDensities(int size){
        float[,,] densities = new float[size,size,size];

        for(int i=0;i<size;i++)
        {
            for(int j=0;j<size;j++)
            {
                for(int k=0;k<size;k++)
                {
                    float t = j/(size);
                    if(Random.Range(0f,1f) >.5f){
                        densities[i,j,k] = t+Random.Range(0,.1f);
                    }else{
                        densities[i,j,k] = t;
                    }
                    
                }
            }
        }

        return densities;
    }

    public float[] GetDensity(float[,,] densities, int x, int y, int z, int size)
    {
        float[] d = new float[8];

        for(int i=0;i<d.Length;i++)
        {
            Vector3 corner = MarchingCubesLookupTables.CubeCorners[i];
            d[i] = densities[x+(int)corner.x,y+(int)corner.y,z+(int)corner.z];
        }

        return d;
    }

    public VoxelData Generate(){
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        VoxelData voxelData = new VoxelData();

        for(int x=0;x<size;x++){
            for(int y=0;y<size;y++)
            {
                for(int z=0;z<size;z++)
                {

                    if(x >=size-1 || y >= size-1 || z >= size-1){
                        break;
                    }

                    float[] densities = GetDensity(densities_matrix,x,y,z,4);

                    int cubeIndex = CalculateCubeIndex(densities,isolevel);
                    if(cubeIndex == 0 || cubeIndex == 255){
                        continue;
                    }

                    int edgeIndex = MarchingCubesLookupTables.EdgeTable[cubeIndex];

                    List<Vector3> vertexList = GenerateVertexList(densities,new Vector3(x,y,z),edgeIndex,isolevel);

                    int rowIndex = 15 * cubeIndex;

                    for(int i=0;MarchingCubesLookupTables.TriangleTable[rowIndex+i] != -1 && i<15; i+=3){
                        Vector3 vertex1 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i]];
                        Vector3 vertex2 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 1]];
                        Vector3 vertex3 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 2]];
                        Vector3 normal = Vector3.Normalize(Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1));
                        if(!vertex1.Equals(vertex2) && 
                        !vertex1.Equals(vertex3) && !vertex2.Equals(vertex3)){
                            voxelData.vertexData.Add(vertex1);
                            voxelData.normalData.Add(normal);
                            voxelData.triangleData.Add(voxelData.vertexData.Count-1);
                            voxelData.vertexData.Add(vertex2);
                            voxelData.normalData.Add(normal);
                            voxelData.triangleData.Add(voxelData.vertexData.Count-1);
                            voxelData.vertexData.Add(vertex3);
                            voxelData.normalData.Add(normal);
                            voxelData.triangleData.Add(voxelData.vertexData.Count-1);
                        }
                    }
                }
            }
        }

        return voxelData;
    }
}
