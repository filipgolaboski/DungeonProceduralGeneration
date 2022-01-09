using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChunkData{
    public List<Vector3> vertexData = new List<Vector3>();
    public List<int> triangleData = new List<int>();

    public List<Vector3> normalData = new List<Vector3>();
    public List<List<Vector3>> normals = new List<List<Vector3>>();

    public Vector3Int chunkPosition;
}

public class VertexInternalData{
    public Vector3 vertex;
    public Vector3 normal;

    public int ID;
}

public class MarchingCubesChunk
{
    public float isolevel = 0.5f;
    int size;
    float [,,] densities_matrix;
    Vector3Int chunkPosition;

    public void Init(DensityChunkData chunkData){
        this.isolevel = chunkData.ISOLevel;
        this.size = chunkData.size.x;
        this.densities_matrix = chunkData.chunkDensity;
        chunkPosition = chunkData.position;
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


    public Vector3 CalculateNormal(Vector3Int p1)
    {
        
        float dx = GetDensity(p1+Vector3Int.right)-GetDensity(p1+Vector3Int.left);
        float dy = GetDensity(p1+Vector3Int.up)-GetDensity(p1+Vector3Int.down);
        float dz = GetDensity(p1+Vector3Int.forward)-GetDensity(p1+Vector3Int.back);
        return new Vector3(dx,dy,dz);
    }

    public float GetDensity(Vector3Int pos)
    {
        return densities_matrix[pos.x,pos.y,pos.z];
    }

    public VertexInternalData VertexInterpolate(Vector3Int p1, Vector3Int p2, float v1, float v2, float isoLevel)
    {

        float t = (isoLevel - v1)/(v2-v1);
        VertexInternalData v = new VertexInternalData();
        v.vertex = p1 + t*((Vector3)p2-p1);
        Vector3 p1n = CalculateNormal(p1);
        Vector3 p2n = CalculateNormal(p2);
        v.normal = Vector3.Normalize(p1n + t*(p2n-p1n));
        return v;
    }


    public VertexInternalData CreateVertex(Vector3Int corner1, Vector3Int corner2){
        float density1 = densities_matrix[corner1.x,corner1.y,corner1.z];
        float density2 = densities_matrix[corner2.x,corner2.y,corner2.z];

        return VertexInterpolate(corner1,corner2,density1,density2,isolevel);
    }


    public float[] GetCubeDensities(float[,,] densities, Vector3Int position)
    {
        float[] d = new float[8];

        for(int i=0;i<d.Length;i++)
        {
            Vector3Int corner = MarchingCubesLookupTables.CubeCorners[i];
            corner+=position;
            d[i] = GetDensity(corner);
        }

        return d;
    }

    public void AddVertex(ref ChunkData chunkData, ref Dictionary<Vector3,int> vertIndexer,Vector3 vertex, Vector3 normal,int x,int y, int z){
        if(!vertIndexer.ContainsKey(vertex)){
                chunkData.vertexData.Add(vertex);
                vertIndexer.Add(vertex,chunkData.vertexData.Count-1);
                chunkData.normals.Add(new List<Vector3>());
                chunkData.normals[chunkData.vertexData.Count-1].Add(normal);
                //chunkData.normalData.Add(normal);
                chunkData.triangleData.Add(chunkData.vertexData.Count-1);
        }else{
                chunkData.triangleData.Add(vertIndexer[vertex]);
                chunkData.normals[vertIndexer[vertex]].Add(normal);

        }
    }

    public List<Vector3Int> GetVoxelCorners(Vector3Int position){
        List<Vector3Int> corners = new List<Vector3Int>();
        for(int i=0;i<8;i++){
            corners.Add(position+MarchingCubesLookupTables.CubeCorners[i]);
        }

        return corners;
    }

    public ChunkData Generate(){
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        Dictionary<Vector3,int> vertIndexer = new Dictionary<Vector3, int>();
        ChunkData chunkData = new ChunkData();

        for(int x=1;x<size-2;x++){
            for(int y=1;y<size-2;y++)
            {
                for(int z=1;z<size-2;z++)
                {
                    Vector3Int position = new Vector3Int(x,y,z);
                    float[] densities = GetCubeDensities(densities_matrix,position);

                    int cubeIndex = CalculateCubeIndex(densities,isolevel);
                    if(cubeIndex == 0 || cubeIndex == 255){
                        continue;
                    }

                    int[] edgeIndex = MarchingCubesLookupTables.triangulation[cubeIndex];
                    List<Vector3Int> cubeCorners = GetVoxelCorners(position);
                    for(int i=0;i<16;i+=3){
                        if(edgeIndex[i] == -1){break;};

                        int edgeIndexA = edgeIndex[i];
                        int a0 = MarchingCubesLookupTables.EdgeIndexTableA[edgeIndexA];
                        int a1 = MarchingCubesLookupTables.EdgeIndexTableB[edgeIndexA];

                        int edgeIndexB = edgeIndex[i+1];
                        int b0 = MarchingCubesLookupTables.EdgeIndexTableA[edgeIndexB];
                        int b1 = MarchingCubesLookupTables.EdgeIndexTableB[edgeIndexB];

                        int edgeIndexC = edgeIndex[i+2];
                        int c0 = MarchingCubesLookupTables.EdgeIndexTableA[edgeIndexC];
                        int c1 = MarchingCubesLookupTables.EdgeIndexTableB[edgeIndexC];

                        VertexInternalData vertex1 = CreateVertex(cubeCorners[a0],cubeCorners[a1]);
                        VertexInternalData vertex2 = CreateVertex(cubeCorners[b0],cubeCorners[b1]);
                        VertexInternalData vertex3 = CreateVertex(cubeCorners[c0],cubeCorners[c1]);
                        if(!vertex1.vertex.Equals(vertex2.vertex) && 
                                !vertex1.vertex.Equals(vertex3.vertex) && !vertex2.vertex.Equals(vertex3.vertex)){
                                    AddVertex(ref chunkData,ref vertIndexer,vertex1.vertex,vertex1.normal,x,y,z);
                                    AddVertex(ref chunkData,ref vertIndexer,vertex2.vertex,vertex2.normal,x,y,z);
                                    AddVertex(ref chunkData,ref vertIndexer,vertex3.vertex,vertex3.normal,x,y,z); 
                            }
                    }
                   /*  List<VertexInternalData> vertexList = GenerateVertexList(densities,position,edgeIndex,isolevel);

                    int rowIndex = 15 * cubeIndex;

                    for(int i=0;MarchingCubesLookupTables.TriangleTable[rowIndex+i] != -1 && i<15; i+=3){
                        VertexInternalData vertex1 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i]];
                        VertexInternalData vertex2 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 1]];
                        VertexInternalData vertex3 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 2]];
                            if(!vertex1.Equals(vertex2) && 
                                !vertex1.Equals(vertex3) && !vertex2.Equals(vertex3)){
                                    AddVertex(ref chunkData,ref vertIndexer,vertex1.vertex,vertex1.normal,x,y,z);
                                    AddVertex(ref chunkData,ref vertIndexer,vertex2.vertex,vertex2.normal,x,y,z);
                                    AddVertex(ref chunkData,ref vertIndexer,vertex3.vertex,vertex2.normal,x,y,z); 
                            }
                    } */
                }
            }
        }


        for(int i=0;i<chunkData.vertexData.Count;i++){
            chunkData.normalData.Add(chunkData.normals[i][0]);
            for(int j=1;j<chunkData.normals[i].Count;j++)
            {
                chunkData.normalData[i] += chunkData.normals[i][j];
            }
            chunkData.normalData[i] /= chunkData.normals[i].Count;
            chunkData.normalData[i].Normalize();
            //chunkData.normalData[i] = Vector3.Normalize(chunkData.normalData[i]);
        }

        chunkData.chunkPosition = chunkPosition;
        return chunkData;
    }
}
